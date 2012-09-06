using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

using NUnit.Framework;

using Payments.SecurePay;

namespace Tests.Payments.SecurePay
{
    [TestFixture]
    public class SecurePayGatewayTests
    {

        public static string TestUrl = "https://payment.securepay.com.au/test/v2/invoice";

        //very temporary way to send the message will build this up propery asap
        public string requestMessage =
@"<?xml version=""1.0"" encoding=""UTF-8""?>
<SecurePayMessage>
    <MessageInfo>
        <messageID>757a5be5b84b4d8ab84ec03ebd24af</messageID>
        <messageTimestamp>20121006055444843500+660</messageTimestamp>
        <timeoutValue>6</timeoutValue>
        <apiVersion>xml-4.2</apiVersion>
    </MessageInfo>
    <MerchantInfo>
        <merchantID>ABC0001</merchantID>
        <password>abc123</password>
    </MerchantInfo>
    <RequestType>Periodic</RequestType>
    <Payment>
      <TxnList count=""1"">
       <Txn ID=""1"">
        <txnType>0</txnType>
        <txnSource>0</txnSource>
        <amount>133700</amount>
        <purchaseOrderNo>MountFranklin</purchaseOrderNo>
        <CreditCardInfo>
                <cardNumber>4444333322221111</cardNumber>
                <expiryDate>10/15</expiryDate>
        </CreditCardInfo>
       </Txn>
      </TxnList>
    </Payment>
</SecurePayMessage>";

        private string BuildViaXdoc()
        {
            return new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement("SecurePayMessage",
                    new XElement("MessageInfo",
                        new XElement("messageID", "757a5be5b84b4d8ab84ec03ebd24af"),
                        new XElement("messageTimestamp", "20121006055444843500+660"),
                        new XElement("timeoutValue", "13987654"),
                        new XElement("apiVersion", "xml-4.2")),
                    new XElement("MerchantInfo",
                        new XElement("merchantID", "ABC0001"),
                        new XElement("password", "abc123")),
                    new XElement("RequestType", "Periodic"),
                    new XElement("Payment",
                        new XElement("TxnList", new XAttribute("count", "1"),
                            new XElement("Txn", new XAttribute("ID", "1"),
                                new XElement("txnType", "0"),
                                new XElement("txnSource", "0"),
                                new XElement("amount", "133700"),
                                new XElement("purchaseOrderNo", "MountFranklin"),
                                    new XElement("CreditCardInfo",
                                        new XElement("cardNumber", "4444333322221111"),
                                        new XElement("expiryDate", "10/15"))))))).ToStringWithDeclaration();
        }


        [Serializable]
        public class SecurePayMessage
        {
            [XmlElement]
            public MessageInfo MessageInfo { get; set; }
        }

        [Serializable]
        public class MessageInfo
        {
            [XmlAttribute]
            public string messageID { get; set; }
            [XmlAttribute]
            public string messageTimestamp { get; set; }
            [XmlAttribute]
            public string timeoutValue { get; set; }
        }


        private string BuildXmlFrom(SecurePayMessage securePayRequest)
        {
            using (var s = new Utf8StringWriter())
            {
                var foo = new SecurePayMessage { MessageInfo = new MessageInfo
                    {
                        messageID = "hello",
                        messageTimestamp = "serialize_plz",
                        timeoutValue = "w0000000000000"
                    }};

                var overrides = new XmlAttributeOverrides();
                new XmlSerializer(typeof(SecurePayMessage), "").Serialize(s, foo);
                return s.ToString();
            }
        }

        [Test]
        public void Test_BuildXmlFrom()
        {
            var spm = new SecurePayMessage();
            var r = BuildXmlFrom(spm);

            Console.WriteLine(PrintXml(r));
            Assert.That(r, Is.StringContaining("serialize_plz"));
        }

        [Test]
        public void Test_BuildViaXdoc()
        {
            var expected = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<SecurePayMessage>
    <MessageInfo>
        <messageID>757a5be5b84b4d8ab84ec03ebd24af</messageID>
        <messageTimestamp>20121006055444843500+660</messageTimestamp>
        <timeoutValue>6</timeoutValue>
        <apiVersion>xml-4.2</apiVersion>
    </MessageInfo>
    <MerchantInfo>
        <merchantID>ABC0001</merchantID>
        <password>abc123</password>
    </MerchantInfo>
    <RequestType>Periodic</RequestType>
    <Payment>
      <TxnList count=""1"">
       <Txn ID=""1"">
        <txnType>0</txnType>
        <txnSource>0</txnSource>
        <amount>133700</amount>
        <purchaseOrderNo>MountFranklin</purchaseOrderNo>
        <CreditCardInfo>
                <cardNumber>4444333322221111</cardNumber>
                <expiryDate>10/15</expiryDate>
        </CreditCardInfo>
       </Txn>
      </TxnList>
    </Payment>
</SecurePayMessage>";

            var r = BuildViaXdoc();

            Console.WriteLine(PrintXml(r));
            Assert.That(r, Is.StringContaining(@"<TxnList count=""1"""));
            Assert.That(r, Is.StringContaining(@"utf-8"));
            Assert.That(r, Is.StringContaining(@"<?xml version=""1.0"" encoding="));
            Assert.That(r, Is.StringContaining(@"<merchantID>ABC0001</merchantID>"));
            Assert.That(r, Is.StringContaining(@"<cardNumber>4444333322221111</cardNumber>"));
            Assert.That(r, Is.StringContaining(@"</SecurePayMessage>"));
        }

        [Test]
        public void Sha1_Hash_SecurePayCheckField()
        {
            
            // Arrange
            var input = "ABC0010|txnpassword|0|Test Reference|100|20110616221931";

            // Act

            var result = SecurePayGateway.Sha1SecurePayDetailsHexString(input);

            // Assert
            Assert.AreEqual("770258D70DF0DFA6E186D5B6F7635D870ABFEA2B", result);
        }

        [Test]
        public void SecurePayGateway_ChargeCustomer()
        {
            // Arrange
            var g = new SecurePayGateway();

            // Act

            // getting 510 on periodic payments! wtf...
            var r = g.ChargeCustomer(new CardInfo(), requestMessage);

            // Assert
            Console.WriteLine(PrintXml(r));

            Assert.IsNotNullOrEmpty(r);
            Assert.That(r, Is.Not.ContainsSubstring("Unable to connect to server"));
        }

        public static string PrintXml(string xml)
        {
            using(var stream = new MemoryStream())
            using (var writer = new XmlTextWriter(stream, Encoding.Unicode))
            {
                var document = new XmlDocument();

                // Create an XmlDocument with the xml
                document.LoadXml(xml);

                writer.Formatting = Formatting.Indented;

                // Write the XML into a formatting XmlTextWriter
                document.WriteContentTo(writer);
                writer.Flush();
                stream.Flush();

                // Have to rewind the MemoryStream in order to read the content
                stream.Position = 0;

                // Read MemoryStream contents into a StreamReader
                var sReader = new StreamReader(stream);

                // Extract the text from the StreamReader
                return sReader.ReadToEnd();
            }
        }
    }

    public sealed class Utf8StringWriter : StringWriter
    {
        public Utf8StringWriter()
        { }

        public Utf8StringWriter(StringBuilder sb)
            :base(sb) { }

        public override Encoding Encoding { get { return Encoding.UTF8; } }
    }

    public static class XmlHelpers
    {
        public static string ToStringWithDeclaration(this XDocument doc)
        {
            if (doc == null)
            {
                throw new ArgumentNullException("doc");
            }
            var builder = new StringBuilder();
            using (var writer = new Utf8StringWriter(builder))
            {
                doc.Save(writer);
            }
            return builder.ToString();
        }
    }
}
