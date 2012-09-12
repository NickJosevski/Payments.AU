using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

using NUnit.Framework;

using Payments.SecurePay;

namespace Tests.Payments.SecurePay
{
    [TestFixture]
    public class SecurePayGatewayTests : GatewayTests
    {
        private SecurePayGateway _gateway;

        public static string ApiInvoice = "https://payment.securepay.com.au/test/v2/invoice";

        public const string ApiPeriodic = "https://test.securepay.com.au/xmlapi/periodic";

        public const string ApiPayment = "https://test.securepay.com.au/xmlapi/payment";

        [TestFixtureSetUp]
        public void Fixture()
        {
            _gateway = new SecurePayGateway(new SecurePayWebCommunication(), "ABC0001","abc123", ApiPeriodic);
            
            SetupCardsAndChargeAmounts();
        }

        /*
        - Periodic Types
            1 Once Off Payment
            2 Day Based Periodic Payment
            3 Calendar Based Periodic Payment
            4 Triggered Payment
        */

        [Test]
        public void SecurePayGateway_OneOffPayemt()
        {
            //NOTE: usage of separate instance of SecurePayGateway() with 'ApiPayment' and NOT 'ApiPeriodic'
            var oneOffPaymentGateway = new SecurePayGateway(new SecurePayWebCommunication(), "ABC0001", "abc123", ApiPayment);

            var p = new SecurePayPayment { Amount = ChargeAmount1, Currency = "AUD" };

            var oneOffPayment = oneOffPaymentGateway.SinglePaymentXml(ValidCard, p, "OneOffInc");
            DebugDisplay(oneOffPayment);

            var r = oneOffPaymentGateway.SendMessage(oneOffPayment, "unit test");

            // Assert
            Console.WriteLine("Response:");
            Console.WriteLine(r.Print());

            Assert.IsNotNull(r);
            Assert.That(r.Status.StatusDescription, Is.Not.ContainsSubstring("Unable to connect to server"));
            Assert.That(r.Status.StatusDescription, Is.EqualTo("Normal"));
            Assert.That(r.Status.StatusCode, Is.EqualTo(0));
        }

        [Test]
        public void SecurePayGateway_PeriodicCharge_Setup_Then_Charge()
        {
            var p = new SecurePayPayment { Amount = ChargeAmount2, Currency = "AUD" };
            var id = SecurePayGateway.CreateClientId();
            var request = _gateway.CreateReadyToTriggerPaymentXml(ValidCard, id, p);
            DebugDisplay(request);

            var r = _gateway.SendMessage(request, "Setup_Then_Charge");

            Console.WriteLine("First Response");
            Console.WriteLine(r.Print());

            // Assert
            AssertStatusGoodSuccessMarkerNoConnectionIssuesForPeriodicPayment(r);

            request = _gateway.TriggerPeriodicPaymentXml(id, p);
            DebugDisplay(request);

            r = _gateway.SendMessage(request, "Setup_Then_Charge");

            Console.WriteLine("Second Response");
            Console.WriteLine(r.Print());

            // Assert
            AssertStatusGoodSuccessMarkerNoConnectionIssuesForPeriodicPayment(r);
        }

        [Test]
        public void SecurePayGateway_PeriodicCharge_Setup_Then_Charge_DuplicateMessageIds()
        {
            var p = new SecurePayPayment { Amount = ChargeAmount2, Currency = "AUD" };
            var customerId = SecurePayGateway.CreateClientId();
            var request = _gateway.CreateReadyToTriggerPaymentXml(ValidCard, customerId, p);
            DebugDisplay(request);

            var r = _gateway.SendMessage(request, "unit test");

            // Assert
            Console.WriteLine("First Response");
            Console.WriteLine(r.Print());

            AssertStatusGoodSuccessMarkerNoConnectionIssuesForPeriodicPayment(r);

            var msgId = SecurePayGateway.CreateMessageId();

            request = _gateway.TriggerPeriodicPaymentXmlWithMessageId(msgId, customerId, p);
            DebugDisplay(request);
            r = _gateway.SendMessage(request, "unit test");

            Console.WriteLine("Second Response");
            Console.WriteLine(r.Print());

            // Assert
            AssertStatusGoodSuccessMarkerNoConnectionIssuesForPeriodicPayment(r);

            request = _gateway.TriggerPeriodicPaymentXmlWithMessageId(msgId, customerId, p);
            DebugDisplay(request);
            r = _gateway.SendMessage(request, "unit test");

            Console.WriteLine("Third Response");
            Console.WriteLine(r.Print());

            // Assert
            AssertStatusGoodSuccessMarkerNoConnectionIssuesForPeriodicPayment(r);
        }

        [Test]
        public void SecurePayGateway_PeriodicCharge_Setup_Charge1stTime_Charge2ndTimeDiffValue()
        {
            var id = SecurePayGateway.CreateClientId();
            var p = new SecurePayPayment { Amount = ChargeAmount2, Currency = "AUD" };
            var request = _gateway.CreateReadyToTriggerPaymentXml(ValidCard, id, p);
            DebugDisplay(request);

            // Charge Setup
            var r = _gateway.SendMessage(request, "unit test");

            Console.WriteLine("First Response");
            Console.WriteLine(r.Print());

            // Assert
            AssertStatusGoodSuccessMarkerNoConnectionIssuesForPeriodicPayment(r);

            request = _gateway.TriggerPeriodicPaymentXml(id, p);
            DebugDisplay(request);

            // Charge 1
            r = _gateway.SendMessage(request, "unit test");

            Console.WriteLine("Second Response");
            Console.WriteLine(r.Print());

            // Assert
            AssertStatusGoodSuccessMarkerNoConnectionIssuesForPeriodicPayment(r);

            // Charge 2
            p.Amount += 100;
            request = _gateway.TriggerPeriodicPaymentXml(id, p);
            r = _gateway.SendMessage(request, "unit test");

            Console.WriteLine("Third Response");
            Console.WriteLine(r.Print());

            // Assert
            AssertStatusGoodSuccessMarkerNoConnectionIssuesForPeriodicPayment(r);
        }

        [Test]
        public void SecurePayGateway_PeriodicCharge_1Setup_2ChargeFailsCustomerDoesntExist_ExpectException()
        {
            var p = new SecurePayPayment { Amount = ChargeAmount2, Currency = "AUD" };
            var request = _gateway.CreateReadyToTriggerPaymentXml(ValidCard, SecurePayGateway.CreateClientId(), p);
            DebugDisplay(request);

            var r = _gateway.SendMessage(request, "unit test");

            // Assert
            Console.WriteLine("First Response");
            Console.WriteLine(r.Print());

            AssertStatusGoodSuccessMarkerNoConnectionIssuesForPeriodicPayment(r);


            // NOTE: new id, so won't find customer
            request = _gateway.TriggerPeriodicPaymentXml(SecurePayGateway.CreateClientId(), p);
            DebugDisplay(request);

            var ex = Assert.Throws<SecurePayException>(() => _gateway.SendMessage(request, "unit test"));

            // Assert
            Assert.IsNotNull(ex);
            Assert.That(ex.StatusDescription, Is.StringContaining("Payment not found"));
        }

        [Test]
        public void SecurePayGateway_PeriodicCharge_Setup_ThenSetupForFuture()
        {
            Console.WriteLine("Future Monthly");
            var p = new SecurePayPayment { Amount = ChargeAmount2, Currency = "AUD" };
            var id = SecurePayGateway.CreateClientId();

            var request = _gateway.CreateScheduledPaymentXml(ValidCard, id, p, new DateTime());
            DebugDisplay(request);

            var r = _gateway.SendMessage(request, "unit test");

            Console.WriteLine("First Response");
            Console.WriteLine(r.Print());

            // Assert
            AssertStatusGoodSuccessMarkerNoConnectionIssuesForPeriodicPayment(r);
        }

        [Test]
        public void SecurePayGateway_PeriodicCharge_Setup_ThenSetupForFuture_EditAmountByDoubling()
        {
            Console.WriteLine("Future Monthly");
            var p = new SecurePayPayment { Amount = ChargeAmount2, Currency = "AUD" };
            var id = SecurePayGateway.CreateClientId();
            var request = _gateway.CreateScheduledPaymentXml(ValidCard, id, p, new DateTime());
            DebugDisplay(request);

            var r = _gateway.SendMessage(request, "unit test");

            // Assert
            Console.WriteLine("First Response");
            Console.WriteLine(r.Print());

            AssertStatusGoodSuccessMarkerNoConnectionIssuesForPeriodicPayment(r);

            DebugDisplay(request);
            p.Amount *= 2;
            request = _gateway.TriggerPeriodicPaymentXml(id, p);
            r = _gateway.SendMessage(request, "unit test");

            Console.WriteLine("Second Response");
            Console.WriteLine(r.Print());

            // Assert
            AssertStatusGoodSuccessMarkerNoConnectionIssuesForPeriodicPayment(r);
        }
    }
}
