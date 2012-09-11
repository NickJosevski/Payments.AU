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
        private int _chargeAmount1;

        private int _chargeAmount2;

        private CardInfo _card;

        private SecurePayGateway _gateway;

        [TestFixtureSetUp]
        public void Fixture()
        {
            var currentVal = Int32.Parse(ReadFromFile(@"..\..\increasing-amount.txt").Trim());

            _chargeAmount1 = currentVal;
            _chargeAmount2 = currentVal + 1;

            WriteToFile(@"..\..\increasing-amount.txt", currentVal + 1);

            _gateway = new SecurePayGateway();

            _card = new CardInfo { Number = "4444333322221111", Expiry = "10/15" };
        }

        public string ReadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new InvalidOperationException(
                    "ReadFromFile() - Unable to find " + Environment.CurrentDirectory + @"\" + filePath);

            using (var reader = new StreamReader(File.OpenRead(filePath)))
            {
                return reader.ReadToEnd();
            }
        }

        public void WriteToFile(string filePath, int amount)
        {
            if (!File.Exists(filePath))
                throw new InvalidOperationException(
                    "WriteToFile() - Unable to find " + Environment.CurrentDirectory + @"\" + filePath);

            using (var writer = new StreamWriter(filePath, false))
            {
                writer.WriteLine(amount);
            }
        }

        public static string ApiInvoice = "https://payment.securepay.com.au/test/v2/invoice";

        public const string ApiPeriodic = "https://test.securepay.com.au/xmlapi/periodic";

        public const string ApiPayment = "https://test.securepay.com.au/xmlapi/payment";

        [Test]
        [Ignore("file read write set up")]
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

        [Test]
        public void SecurePayGateway_OneOffPayemt()
        {
            var oneOffPayment = SecurePayGateway.SinglePaymentXml(_card, _chargeAmount1 * 100, "OneOffInc");
            SendingDebug(oneOffPayment);

            var r = new SecurePayGateway(ApiPayment).SendMessage(oneOffPayment);

            // Assert
            Console.WriteLine(PrintXml(r));

            Assert.IsNotNullOrEmpty(r);
            Assert.That(r, Is.Not.ContainsSubstring("Unable to connect to server"));
            Assert.That(r, Is.Not.ContainsSubstring("Invalid Transaction"));
            Assert.That(r, Is.StringContaining("<statusCode>0"));
        }

        [Test]
        public void SecurePayGateway_PeriodicCharge_Setup_Then_Charge()
        {
            var p = new Payment { Amount = _chargeAmount2, Currency = "AUD" };
            var id = SecurePayGateway.GetClientId();
            var request = _gateway.CreateReadyToTriggerPaymentXml(_card, id, p);
            SendingDebug(request);

            var r = new SecurePayGateway(ApiPeriodic).SendMessage(request);

            // Assert
            Console.WriteLine("First Response");
            Console.WriteLine(PrintXml(r));

            Assert.IsNotNullOrEmpty(r);
            Assert.That(r, Is.Not.ContainsSubstring("Unable to connect to server"));
            Assert.That(r, Is.StringContaining("<statusCode>0"));
            Assert.That(r, Is.StringContaining("<successful>yes"));

            request = _gateway.TriggerPeriodicPaymentXml(id, p);
            SendingDebug(request);

            r = new SecurePayGateway(ApiPeriodic).SendMessage(request);

            // Assert
            Console.WriteLine("Second Response");
            Console.WriteLine(PrintXml(r));

            Assert.IsNotNullOrEmpty(r);
            Assert.That(r, Is.Not.ContainsSubstring("Unable to connect to server"));
            Assert.That(r, Is.StringContaining("<statusCode>0"));
            Assert.That(r, Is.StringContaining("<successful>yes"));
        }

        [Test]
        public void SecurePayGateway_PeriodicCharge_Setup_Then_Charge_DuplicateMessageIds()
        {
            var p = new Payment { Amount = _chargeAmount2, Currency = "AUD" };
            var customerId = SecurePayGateway.GetClientId();
            var request = _gateway.CreateReadyToTriggerPaymentXml(_card, customerId, p);
            SendingDebug(request);

            var r = new SecurePayGateway(ApiPeriodic).SendMessage(request);

            // Assert
            Console.WriteLine("First Response");
            Console.WriteLine(PrintXml(r));

            Assert.IsNotNullOrEmpty(r);
            Assert.That(r, Is.Not.ContainsSubstring("Unable to connect to server"));
            Assert.That(r, Is.StringContaining("<statusCode>0"));
            Assert.That(r, Is.StringContaining("<successful>yes"));

            var msgId = SecurePayGateway.CreateMessageId();

            request = _gateway.TriggerPeriodicPaymentXmlWithMessageId(msgId, customerId, p);
            SendingDebug(request);
            r = new SecurePayGateway(ApiPeriodic).SendMessage(request);

            Console.WriteLine("Second Response");
            Console.WriteLine(PrintXml(r));

            // Assert
            Assert.IsNotNullOrEmpty(r);
            Assert.That(r, Is.Not.ContainsSubstring("Unable to connect to server"));
            Assert.That(r, Is.StringContaining("<statusCode>0"));
            Assert.That(r, Is.StringContaining("<successful>yes"));

            request = _gateway.TriggerPeriodicPaymentXmlWithMessageId(msgId, customerId, p);
            SendingDebug(request);
            r = new SecurePayGateway(ApiPeriodic).SendMessage(request);

            Console.WriteLine("Third Response");
            Console.WriteLine(PrintXml(r));

            // Assert

            Assert.IsNotNullOrEmpty(r);
            Assert.That(r, Is.Not.ContainsSubstring("Unable to connect to server"));
            Assert.That(r, Is.StringContaining("<statusCode>0"));
            Assert.That(r, Is.StringContaining("<successful>yes"));
        }

        [Test]
        public void SecurePayGateway_PeriodicCharge_Setup_Charge1stTime_Charge2ndTimeDiffValue()
        {
            var id = SecurePayGateway.GetClientId();
            var p = new Payment { Amount = _chargeAmount2, Currency = "AUD" };
            var request = _gateway.CreateReadyToTriggerPaymentXml(_card, id, p);
            SendingDebug(request);

            // Charge Setup
            var r = new SecurePayGateway(ApiPeriodic).SendMessage(request);

            // Assert
            Console.WriteLine("First Response");
            Console.WriteLine(PrintXml(r));

            Assert.IsNotNullOrEmpty(r);
            Assert.That(r, Is.Not.ContainsSubstring("Unable to connect to server"));
            Assert.That(r, Is.StringContaining("<statusCode>0"));
            Assert.That(r, Is.StringContaining("<successful>yes"));

            request = _gateway.TriggerPeriodicPaymentXml(id, p);
            SendingDebug(request);

            // Charge 1
            r = new SecurePayGateway(ApiPeriodic).SendMessage(request);

            // Assert
            Console.WriteLine("Second Response");
            Console.WriteLine(PrintXml(r));

            Assert.IsNotNullOrEmpty(r);
            Assert.That(r, Is.Not.ContainsSubstring("Unable to connect to server"));
            Assert.That(r, Is.StringContaining("<statusCode>0"));
            Assert.That(r, Is.StringContaining("<successful>yes"));

            // Charge 2
            p.Amount += 100;
            request = _gateway.TriggerPeriodicPaymentXml(id, p);
            r = new SecurePayGateway(ApiPeriodic).SendMessage(request);

            // Assert
            Console.WriteLine("Third Response");
            Console.WriteLine(PrintXml(r));

            Assert.IsNotNullOrEmpty(r);
            Assert.That(r, Is.Not.ContainsSubstring("Unable to connect to server"));
            Assert.That(r, Is.StringContaining("<statusCode>0"));
            Assert.That(r, Is.StringContaining("<successful>yes"));
        }

        [Test]
        public void SecurePayGateway_PeriodicCharge_1Setup_2ChargeFailsCustomerDoesntExist()
        {
            var p = new Payment { Amount = _chargeAmount2, Currency = "AUD" };
            var request = _gateway.CreateReadyToTriggerPaymentXml(_card, SecurePayGateway.GetClientId(), p);
            SendingDebug(request);

            var r = new SecurePayGateway(ApiPeriodic).SendMessage(request);

            // Assert
            Console.WriteLine("First Response");
            Console.WriteLine(PrintXml(r));

            Assert.IsNotNullOrEmpty(r);
            Assert.That(r, Is.Not.ContainsSubstring("Unable to connect to server"));
            Assert.That(r, Is.StringContaining("<statusCode>0"));
            Assert.That(r, Is.StringContaining("<successful>yes"));


            // NOTE: new id, so won't find customer
            request = _gateway.TriggerPeriodicPaymentXml(SecurePayGateway.GetClientId(), p);
            SendingDebug(request);

            r = new SecurePayGateway(ApiPeriodic).SendMessage(request);

            // Assert
            Console.WriteLine("Second Response");
            Console.WriteLine(PrintXml(r));

            Assert.IsNotNullOrEmpty(r);
            Assert.That(r, Is.StringContaining("Payment not found"));
        }

        /*
        Periodic Types
        1 Once Off Payment
        2 Day Based Periodic Payment
        3 Calendar Based Periodic Payment
        4 Triggered Payment
        */

        [Test]
        public void SecurePayGateway_PeriodicCharge_Setup_ThenSetupForFuture()
        {
            Console.WriteLine("Future Monthly");
            var p = new Payment { Amount = _chargeAmount2, Currency = "AUD" };
            var id = SecurePayGateway.GetClientId();
            var request = _gateway.CreateScheduledPaymentXml(_card, id, p, new DateTime());
            SendingDebug(request);

            var r = new SecurePayGateway(ApiPeriodic).SendMessage(request);

            // Assert
            Console.WriteLine("First Response");
            Console.WriteLine(PrintXml(r));

            Assert.IsNotNullOrEmpty(r);
            Assert.That(r, Is.Not.ContainsSubstring("Duplicate Client ID Found"));
            Assert.That(r, Is.StringContaining("<statusCode>0"));
            Assert.That(r, Is.StringContaining("<successful>yes"));
        }

        [Test]
        public void SecurePayGateway_PeriodicCharge_Setup_ThenSetupForFuture_EditAmountByDoubling()
        {
            Console.WriteLine("Future Monthly");
            var p = new Payment { Amount = _chargeAmount2, Currency = "AUD" };
            var id = SecurePayGateway.GetClientId();
            var request = _gateway.CreateScheduledPaymentXml(_card, id, p, new DateTime());
            SendingDebug(request);

            var r = new SecurePayGateway(ApiPeriodic).SendMessage(request);

            // Assert
            Console.WriteLine("First Response");
            Console.WriteLine(PrintXml(r));

            Assert.IsNotNullOrEmpty(r);
            Assert.That(r, Is.Not.ContainsSubstring("Unable to connect to server"));
            Assert.That(r, Is.StringContaining("<statusCode>0"));

            SendingDebug(request);
            p.Amount *= 2;
            request = _gateway.TriggerPeriodicPaymentXml(id, p);
            r = new SecurePayGateway(ApiPeriodic).SendMessage(request);

            Assert.IsNotNullOrEmpty(r);
            Assert.That(r, Is.Not.ContainsSubstring("Unable to connect to server"));
            Assert.That(r, Is.StringContaining("<statusCode>0"));
            Assert.That(r, Is.StringContaining("<successful>yes"));

            Console.WriteLine("Second Response");
            Console.WriteLine(PrintXml(r));
        }

        [Test]
        public void Test_BuildViaXdoc()
        {
            var p = new Payment { Amount = _chargeAmount2, Currency = "AUD" };
            var r = _gateway.CreateReadyToTriggerPaymentXml(_card, SecurePayGateway.GetClientId(), p);

            Console.WriteLine(PrintXml(r));
            Assert.That(r, Is.StringContaining(@"<PeriodicList count=""1"""));
            Assert.That(r, Is.StringContaining(@"utf-8"));
            Assert.That(r, Is.StringContaining(@"<?xml version=""1.0"" encoding="));
            Assert.That(r, Is.StringContaining(@"<merchantID>ABC0001</merchantID>"));
            Assert.That(r, Is.StringContaining(@"<cardNumber>4444333322221111</cardNumber>"));
            Assert.That(r, Is.StringContaining(@"<RequestType>Periodic</RequestType>"));
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
        [Ignore("Not matching SecurePay's expectations of the hash, not sure why")]
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
        public void TurnXmlStringIntoType()
        {
            // Arrange
            Console.WriteLine("Future Monthly");
            var p = new Payment { Amount = _chargeAmount2, Currency = "AUD" };
            var id = SecurePayGateway.GetClientId();
            var request = _gateway.CreateScheduledPaymentXml(_card, id, p, new DateTime());
            SendingDebug(request);

            var r = new SecurePayGateway(ApiPeriodic).SendMessage(request);

            // Act
            var o = FromXml<SecurePayMessage>(r);

            // Assert
            Assert.IsNotNull(o);
            Assert.IsNotNullOrEmpty(o.MessageInfo.MessageId);
        }

        [Test]
        public void UsingAnXmlStringTakenFromApiDocumenation_CreateASecurePayMessageObject_ConvertBothToString_AndDiff_ExpectIdentical()
        {
            //Very important test to ensure that the SecurePayMessage XML deserialization doesn't miss any elements

            // Arrange
            var xml = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""no""?>
<SecurePayMessage>
  <MessageInfo>
    <messageID>889a1e7f66f04169bd17013c08fb89</messageID>
    <messageTimestamp>20121009170032054000+600</messageTimestamp>
    <apiVersion>spxml-3.0</apiVersion>
  </MessageInfo>
  <RequestType>Periodic</RequestType>
  <MerchantInfo>
    <merchantID>ABC0001</merchantID>
  </MerchantInfo>
  <Status>
    <statusCode>0</statusCode>
    <statusDescription>Normal</statusDescription>
  </Status>
  <Periodic>
    <PeriodicList count=""2"">
      <PeriodicItem ID=""1"">
        <actionType>trigger</actionType>
        <clientID>first</clientID>
        <responseCode>00</responseCode>
        <responseText>Approved</responseText>
        <successful>yes</successful>
        <txnType>3</txnType>
        <amount>138400</amount>
        <currency>AUD</currency>
        <txnID>412667</txnID>
        <receipt>702295</receipt>
        <ponum>702295ca60182b75d649e8bf1f</ponum>
        <settlementDate>20120910</settlementDate>
        <CreditCardInfo>
          <pan>444433...111</pan>
          <expiryDate>10/15</expiryDate>
          <recurringFlag>no</recurringFlag>
          <cardType>6</cardType>
          <cardDescription>Visa</cardDescription>
        </CreditCardInfo>
      </PeriodicItem>
      <PeriodicItem ID=""2"">
        <actionType>trigger</actionType>
        <clientID>second</clientID>
        <responseCode>00</responseCode>
        <responseText>Approved</responseText>
        <successful>yes</successful>
        <txnType>3</txnType>
        <amount>138400</amount>
        <currency>AUD</currency>
        <txnID>412667</txnID>
        <receipt>702295</receipt>
        <ponum>702295ca60182b75d649e8bf1f</ponum>
        <settlementDate>20120910</settlementDate>
        <CreditCardInfo>
          <pan>444433...111</pan>
          <expiryDate>10/15</expiryDate>
          <recurringFlag>no</recurringFlag>
          <cardType>6</cardType>
          <cardDescription>Visa</cardDescription>
        </CreditCardInfo>
      </PeriodicItem>
    </PeriodicList>
  </Periodic>
</SecurePayMessage>
";
            // Act

            var secPayMessageCreatedFromXml = FromXml<SecurePayMessage>(xml);

            // Assert
            Assert.IsNotNull(secPayMessageCreatedFromXml);
            Assert.IsNotNullOrEmpty(secPayMessageCreatedFromXml.MessageInfo.MessageId);
            Assert.That(secPayMessageCreatedFromXml.Periodic.PeriodicList.Count, Is.EqualTo(2));
            Assert.IsNotEmpty(secPayMessageCreatedFromXml.Periodic.PeriodicList.PeriodicItem);
            Assert.IsTrue(secPayMessageCreatedFromXml.Periodic.PeriodicList.PeriodicItem.Count == 2);
            Assert.IsTrue(secPayMessageCreatedFromXml.Periodic.PeriodicList.PeriodicItem[0].Id == 1);

            Assert.IsTrue(secPayMessageCreatedFromXml.Periodic.PeriodicList.PeriodicItem[0].Id == 1);

            var builder = new StringBuilder();
            using (var writer = new Utf8StringWriter(builder))
            {
                var x = new XmlSerializer(secPayMessageCreatedFromXml.GetType());
                x.Serialize(writer, secPayMessageCreatedFromXml);
            }
            
            var stringsToRemove = new List<string>()
                {
                    @"xmlns:xsd=""http://www.w3.org/2001/XMLSchema""",
                    @"xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""",
                    @"standalone=""no"""
                };


            var result = builder.ToString().ExceptBlanks().Replace(">00<", ">0<").Replace(">000<", ">0<").ToLower();
            var source = xml.ExceptBlanks().Replace(">00<", ">0<").Replace(">000<", ">0<").ToLower();

            stringsToRemove.ForEach(s =>
                {
                    result = result.Replace(s.ToLower(), "");
                    source = source.Replace(s.ToLower(), "");
                });

            /*Console.WriteLine("Src:");
            Console.WriteLine(source);
            Console.WriteLine("Response:");
            Console.WriteLine(result);*/

            Assert.AreEqual(result, source);
        }

        public static TReturnType FromXml<TReturnType>(string xml) where TReturnType: class
        {
            using (var stringReader = new StringReader(xml))
            using (var xmlReader = new XmlTextReader(stringReader))
            {
                return new XmlSerializer(typeof(TReturnType)).Deserialize(xmlReader) as TReturnType;
            }
        }

        public static string PrintXml(string xml)
        {
            using (var stream = new MemoryStream())
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

        [Test]
        public void GuidTrim()
        {
            // Arrange
            var i = 0;

            while (i < 10)
            {
                var g = Guid.NewGuid();

                // Act
                var o = g.ToString().Replace("-", "").Substring(0, 30);

                Assert.IsTrue(o.Length <= 30, "was " + o.Length);
                Console.WriteLine(o);
                // Assert
                i++;
            }
        }
    }
}
