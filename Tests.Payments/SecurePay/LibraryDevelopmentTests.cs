using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

using NUnit.Framework;

using Payments.SecurePay;

namespace Tests.Payments.SecurePay
{
    [TestFixture]
    public class LibraryDevelopmentTests : GatewayTests
    {
        public const string ApiPeriodic = "https://test.securepay.com.au/xmlapi/periodic";

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
                        </SecurePayMessage>";
            // Act

            var secPayMessageCreatedFromXml = xml.As<SecurePayMessage>();

            // Assert
            Assert.IsNotNull(secPayMessageCreatedFromXml);
            Assert.IsNotNullOrEmpty(secPayMessageCreatedFromXml.MessageInfo.MessageId);
            Assert.That(secPayMessageCreatedFromXml.Periodic.PeriodicList.Count, Is.EqualTo(2));
            Assert.IsNotEmpty(secPayMessageCreatedFromXml.Periodic.PeriodicList.PeriodicItem);
            Assert.IsTrue(secPayMessageCreatedFromXml.Periodic.PeriodicList.PeriodicItem.Count == 2);

            Assert.IsTrue(secPayMessageCreatedFromXml.Periodic.PeriodicList.PeriodicItem[0].Id == 1);
            Assert.IsTrue(secPayMessageCreatedFromXml.Periodic.PeriodicList.PeriodicItem[1].Id == 2);

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

        [Test]
        public void Test_BuildViaXdoc()
        {
            var gateway = new SecurePayGateway(new SecurePayEndpoint(), ApiPeriodic);

            var p = new SecurePayPayment { Amount = 1000, Currency = "AUD" };
            var r = gateway.CreateReadyToTriggerPaymentXml(ValidCard, SecurePayGateway.GetClientId(), p);

            Console.WriteLine(r.Print());

            Assert.That(r, Is.StringContaining(@"<PeriodicList count=""1"""));
            Assert.That(r, Is.StringContaining(@"utf-8"));
            Assert.That(r, Is.StringContaining(@"<?xml version=""1.0"" encoding="));
            Assert.That(r, Is.StringContaining(@"<merchantID>ABC0001</merchantID>"));
            Assert.That(r, Is.StringContaining(@"<cardNumber>4444333322221111</cardNumber>"));
            Assert.That(r, Is.StringContaining(@"<RequestType>Periodic</RequestType>"));
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
            var gateway = new SecurePayGateway(new SecurePayEndpoint(), ApiPeriodic);

            Console.WriteLine("Future Monthly");
            var p = new SecurePayPayment { Amount = 1000, Currency = "AUD" };
            var id = SecurePayGateway.GetClientId();

            var request = gateway.CreateScheduledPaymentXml(ValidCard, id, p, new DateTime());

            // Act
            var r = gateway.SendMessage(request, "unit test");


            // Assert
            Assert.IsNotNull(r);
            Assert.IsNotNullOrEmpty(r.MessageInfo.MessageId);
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