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
        private int _chargeAmount;

        [TestFixtureSetUp]
        public void Fixture()
        {
            _chargeAmount = Int32.Parse(ReadFromFile(@"..\..\increasing-amount.txt").Trim());
            _chargeAmount++;
            WriteToFile(@"..\..\increasing-amount.txt", _chargeAmount);
        }


        public string ReadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new InvalidOperationException("ReadFromFile() - Unable to find " + Environment.CurrentDirectory + @"\" + filePath);

            using (var reader = new StreamReader(File.OpenRead(filePath)))
            {
                return reader.ReadToEnd();
            }
        }

        public void WriteToFile(string filePath, int amount)
        {
            if (!File.Exists(filePath))
                throw new InvalidOperationException("WriteToFile() - Unable to find " + Environment.CurrentDirectory + @"\" + filePath);

            using (var writer = new StreamWriter(filePath, false))
            {
                writer.WriteLine(amount);
            }
        }

        public static string ApiInvoice = "https://payment.securepay.com.au/test/v2/invoice";
        
        public const string ApiPeriodic = "https://test.securepay.com.au/xmlapi/periodic";
        public const string ApiPayment = "https://test.securepay.com.au/xmlapi/payment";

        [Test]
        public void CanIncrementFileOnEachMainRun()
        {
            // Arrange
            
            // Act
            var currentResult = Int32.Parse(ReadFromFile(@"..\..\increasing-amount.txt").Trim());

            // Assert
            Console.WriteLine("Amount: " + currentResult);
            Assert.That(currentResult, Is.GreaterThan(1337));
        }

        /*
        Periodic Types
        1 Once Off Payment
        2 Day Based Periodic Payment
        3 Calendar Based Periodic Payment
        4 Triggered Payment
        */

        private string RegularPaymentXml()
        {
            return new XDocument(
                new XDeclaration("1.0", "utf-8", "no"),
                new XElement("SecurePayMessage",
                    new XElement("MessageInfo",
                        new XElement("messageID", "757a5be5b84b4d8ab84ec03ebd24af"),
                        new XElement("messageTimestamp", SecurePayGateway.GetTimeStamp(DateTime.Now)),
                        new XElement("timeoutValue", "30"),
                        new XElement("apiVersion", "xml-4.2")),
                    new XElement("MerchantInfo",
                        new XElement("merchantID", "ABC0001"),
                        new XElement("password", "abc123")),
                    new XElement("RequestType", "Payment"),
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

        private string BuildXmlFrom(SecurePayMessage securePayRequest)
        {
            using (var s = new Utf8StringWriter())
            {
                var foo = new SecurePayMessage { MessageInfo = new MessageInfo
                    {
                        messageID = "hello",
                        messageTimestamp = SecurePayGateway.GetTimeStamp(DateTime.Now),
                        timeoutValue = "w0000000000000"
                    }};

                var overrides = new XmlAttributeOverrides();
                new XmlSerializer(typeof(SecurePayMessage), "").Serialize(s, foo);
                return s.ToString();
            }
        }

        [Test]
        public void SecurePayGateway_RegularPayemt()
        {
            SendingDebug(RegularPaymentXml());

            var r = new SecurePayGateway(ApiPayment)
                .SendMessage(RegularPaymentXml());

            // Assert
            Console.WriteLine(PrintXml(r));

            Assert.IsNotNullOrEmpty(r);
            Assert.That(r, Is.Not.ContainsSubstring("Unable to connect to server"));
        }

        [Test]
        public void SecurePayGateway_PeriodicCharge_1Setup_2Charge()
        {
            var id = SecurePayGateway.GetClientId();
            var request = SecurePayGateway.PeriodicPaymentXml("add", id);
            SendingDebug(request);

            var r = new SecurePayGateway(ApiPeriodic).SendMessage(request);

            // Assert
            Console.WriteLine("First Response");
            Console.WriteLine(PrintXml(r));

            Assert.IsNotNullOrEmpty(r);
            Assert.That(r, Is.Not.ContainsSubstring("Unable to connect to server"));

            request = SecurePayGateway.PeriodicPaymentXml("trigger", id);
            SendingDebug(request);

            r = new SecurePayGateway(ApiPeriodic).SendMessage(request);

            // Assert
            Console.WriteLine("Second Response");
            Console.WriteLine(PrintXml(r));

            Assert.IsNotNullOrEmpty(r);
            Assert.That(r, Is.Not.ContainsSubstring("Unable to connect to server"));
        }

        [Test]
        public void SecurePayGateway_PeriodicCharge_1Setup_2ChargeFailsCustomerDoesntExist()
        {
            var request = SecurePayGateway.PeriodicPaymentXml("add", SecurePayGateway.GetClientId());
            SendingDebug(request);

            var r = new SecurePayGateway(ApiPeriodic).SendMessage(request);

            // Assert
            Console.WriteLine("First Response");
            Console.WriteLine(PrintXml(r));

            Assert.IsNotNullOrEmpty(r);
            Assert.That(r, Is.Not.ContainsSubstring("Unable to connect to server"));

            // NOTE: new id, so won't find customer
            request = SecurePayGateway.PeriodicPaymentXml("trigger", SecurePayGateway.GetClientId());
            SendingDebug(request);

            r = new SecurePayGateway(ApiPeriodic).SendMessage(request);

            // Assert
            Console.WriteLine("Second Response");
            Console.WriteLine(PrintXml(r));

            Assert.IsNotNullOrEmpty(r);
            Assert.That(r, Is.StringContaining("Payment not found"));
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
        <messageTimestamp>20120709110813287000+600</messageTimestamp>
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

            var r = SecurePayGateway.PeriodicPaymentXml("add", SecurePayGateway.GetClientId());

            Console.WriteLine(PrintXml(r));
            Assert.That(r, Is.StringContaining(@"<TxnList count=""1"""));
            Assert.That(r, Is.StringContaining(@"utf-8"));
            Assert.That(r, Is.StringContaining(@"<?xml version=""1.0"" encoding="));
            Assert.That(r, Is.StringContaining(@"<merchantID>ABC0001</merchantID>"));
            Assert.That(r, Is.StringContaining(@"<cardNumber>4444333322221111</cardNumber>"));
            Assert.That(r, Is.StringContaining(@"</SecurePayMessage>"));
        }

        private void SendingDebug(string xml)
        {
            Console.WriteLine("REQUEST:");
            Console.WriteLine("********************************************************************************");
            Console.WriteLine(PrintXml(xml));
            Console.WriteLine("********************************************************************************");
            Console.WriteLine("");
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
    <RequestType>Payment</RequestType>
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

}
