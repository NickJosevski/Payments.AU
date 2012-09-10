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
            var oneOffPayment = SecurePayGateway.SinglePaymentXml(_card, _chargeAmount1, "OneOffInc");
            SendingDebug(oneOffPayment);

            var r = new SecurePayGateway(ApiPayment).SendMessage(oneOffPayment);

            // Assert
            Console.WriteLine(PrintXml(r));

            Assert.IsNotNullOrEmpty(r);
            Assert.That(r, Is.Not.ContainsSubstring("Unable to connect to server"));
            Assert.That(r, Is.StringContaining("<statusCode>0"));
        }

        [Test]
        public void SecurePayGateway_PeriodicCharge_Setup_Then_Charge()
        {
            var p = new Payment { Amount = _chargeAmount2, Currency = "AUD" };
            var id = SecurePayGateway.GetClientId();
            var request = _gateway.CreatePeriodicPaymentXml(_card, id, p);
            SendingDebug(request);

            var r = new SecurePayGateway(ApiPeriodic).SendMessage(request);

            // Assert
            Console.WriteLine("First Response");
            Console.WriteLine(PrintXml(r));

            Assert.IsNotNullOrEmpty(r);
            Assert.That(r, Is.Not.ContainsSubstring("Unable to connect to server"));
            Assert.That(r, Is.StringContaining("<statusCode>0"));

            request = _gateway.TriggerPeriodicPaymentXml(id, p);
            SendingDebug(request);

            r = new SecurePayGateway(ApiPeriodic).SendMessage(request);

            // Assert
            Console.WriteLine("Second Response");
            Console.WriteLine(PrintXml(r));

            Assert.IsNotNullOrEmpty(r);
            Assert.That(r, Is.Not.ContainsSubstring("Unable to connect to server"));
            Assert.That(r, Is.StringContaining("<statusCode>0"));
        }

        [Test]
        public void SecurePayGateway_PeriodicCharge_Setup_Charge1stTime_Charge2ndTimeDiffValue()
        {
            var id = SecurePayGateway.GetClientId();
            var p = new Payment { Amount = _chargeAmount2, Currency = "AUD" };
            var request = _gateway.CreatePeriodicPaymentXml(_card, id, p);
            SendingDebug(request);

            // Charge Setup
            var r = new SecurePayGateway(ApiPeriodic).SendMessage(request);

            // Assert
            Console.WriteLine("First Response");
            Console.WriteLine(PrintXml(r));

            Assert.IsNotNullOrEmpty(r);
            Assert.That(r, Is.Not.ContainsSubstring("Unable to connect to server"));
            Assert.That(r, Is.StringContaining("<statusCode>0"));

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
        }

        [Test]
        public void SecurePayGateway_PeriodicCharge_1Setup_2ChargeFailsCustomerDoesntExist()
        {
            var p = new Payment { Amount = _chargeAmount2, Currency = "AUD" };
            var request = _gateway.CreatePeriodicPaymentXml(_card, SecurePayGateway.GetClientId(), p);
            SendingDebug(request);

            var r = new SecurePayGateway(ApiPeriodic).SendMessage(request);

            // Assert
            Console.WriteLine("First Response");
            Console.WriteLine(PrintXml(r));

            Assert.IsNotNullOrEmpty(r);
            Assert.That(r, Is.Not.ContainsSubstring("Unable to connect to server"));
            Assert.That(r, Is.StringContaining("<statusCode>0"));
            

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

        [Test]
        public void Test_BuildViaXdoc()
        {
            var p = new Payment { Amount = _chargeAmount2, Currency = "AUD" };
            var r = _gateway.CreatePeriodicPaymentXml(_card, SecurePayGateway.GetClientId(), p);

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
