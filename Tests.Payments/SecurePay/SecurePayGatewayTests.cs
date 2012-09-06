using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

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


}
