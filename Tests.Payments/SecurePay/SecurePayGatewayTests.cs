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
            var p = new SecurePayPayment { Amount = ChargeAmount1 * 100, Currency = "AUD" };

            var oneOffPayment = _gateway.SinglePaymentXml(ValidCard, p, "OneOffInc");
            SendingDebug(oneOffPayment);

            var r = _gateway.SendMessage(oneOffPayment, "unit test");

            // Assert
            Console.WriteLine(r.Print());

            AssertStatusGoodSuccessMarkerNoConnectionIssues(r);
        }

        [Test]
        public void SecurePayGateway_PeriodicCharge_Setup_Then_Charge()
        {
            var p = new SecurePayPayment { Amount = ChargeAmount2, Currency = "AUD" };
            var id = SecurePayGateway.CreateClientId();
            var request = _gateway.CreateReadyToTriggerPaymentXml(ValidCard, id, p);
            SendingDebug(request);

            var r = _gateway.SendMessage(request, "Setup_Then_Charge");

            Console.WriteLine("First Response");
            Console.WriteLine(r.Print());

            // Assert
            AssertStatusGoodSuccessMarkerNoConnectionIssues(r);

            request = _gateway.TriggerPeriodicPaymentXml(id, p);
            SendingDebug(request);

            r = _gateway.SendMessage(request, "Setup_Then_Charge");

            Console.WriteLine("Second Response");
            Console.WriteLine(r.Print());

            // Assert
            AssertStatusGoodSuccessMarkerNoConnectionIssues(r);
        }

        [Test]
        public void SecurePayGateway_PeriodicCharge_Setup_Then_Charge_DuplicateMessageIds()
        {
            var p = new SecurePayPayment { Amount = ChargeAmount2, Currency = "AUD" };
            var customerId = SecurePayGateway.CreateClientId();
            var request = _gateway.CreateReadyToTriggerPaymentXml(ValidCard, customerId, p);
            SendingDebug(request);

            var r = _gateway.SendMessage(request, "unit test");

            // Assert
            Console.WriteLine("First Response");
            Console.WriteLine(r.Print());

            AssertStatusGoodSuccessMarkerNoConnectionIssues(r);

            var msgId = SecurePayGateway.CreateMessageId();

            request = _gateway.TriggerPeriodicPaymentXmlWithMessageId(msgId, customerId, p);
            SendingDebug(request);
            r = _gateway.SendMessage(request, "unit test");

            Console.WriteLine("Second Response");
            Console.WriteLine(r.Print());

            // Assert
            AssertStatusGoodSuccessMarkerNoConnectionIssues(r);

            request = _gateway.TriggerPeriodicPaymentXmlWithMessageId(msgId, customerId, p);
            SendingDebug(request);
            r = _gateway.SendMessage(request, "unit test");

            Console.WriteLine("Third Response");
            Console.WriteLine(r.Print());

            // Assert
            AssertStatusGoodSuccessMarkerNoConnectionIssues(r);
        }

        [Test]
        public void SecurePayGateway_PeriodicCharge_Setup_Charge1stTime_Charge2ndTimeDiffValue()
        {
            var id = SecurePayGateway.CreateClientId();
            var p = new SecurePayPayment { Amount = ChargeAmount2, Currency = "AUD" };
            var request = _gateway.CreateReadyToTriggerPaymentXml(ValidCard, id, p);
            SendingDebug(request);

            // Charge Setup
            var r = _gateway.SendMessage(request, "unit test");

            Console.WriteLine("First Response");
            Console.WriteLine(r.Print());

            // Assert
            AssertStatusGoodSuccessMarkerNoConnectionIssues(r);

            request = _gateway.TriggerPeriodicPaymentXml(id, p);
            SendingDebug(request);

            // Charge 1
            r = _gateway.SendMessage(request, "unit test");

            Console.WriteLine("Second Response");
            Console.WriteLine(r.Print());

            // Assert
            AssertStatusGoodSuccessMarkerNoConnectionIssues(r);

            // Charge 2
            p.Amount += 100;
            request = _gateway.TriggerPeriodicPaymentXml(id, p);
            r = _gateway.SendMessage(request, "unit test");

            Console.WriteLine("Third Response");
            Console.WriteLine(r.Print());

            // Assert
            AssertStatusGoodSuccessMarkerNoConnectionIssues(r);
        }

        [Test]
        public void SecurePayGateway_PeriodicCharge_1Setup_2ChargeFailsCustomerDoesntExist_ExpectException()
        {
            var p = new SecurePayPayment { Amount = ChargeAmount2, Currency = "AUD" };
            var request = _gateway.CreateReadyToTriggerPaymentXml(ValidCard, SecurePayGateway.CreateClientId(), p);
            SendingDebug(request);

            var r = _gateway.SendMessage(request, "unit test");

            // Assert
            Console.WriteLine("First Response");
            Console.WriteLine(r.Print());

            AssertStatusGoodSuccessMarkerNoConnectionIssues(r);


            // NOTE: new id, so won't find customer
            request = _gateway.TriggerPeriodicPaymentXml(SecurePayGateway.CreateClientId(), p);
            SendingDebug(request);

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
            SendingDebug(request);

            var r = _gateway.SendMessage(request, "unit test");

            Console.WriteLine("First Response");
            Console.WriteLine(r.Print());

            // Assert
            AssertStatusGoodSuccessMarkerNoConnectionIssues(r);
        }

        [Test]
        public void SecurePayGateway_PeriodicCharge_Setup_ThenSetupForFuture_EditAmountByDoubling()
        {
            Console.WriteLine("Future Monthly");
            var p = new SecurePayPayment { Amount = ChargeAmount2, Currency = "AUD" };
            var id = SecurePayGateway.CreateClientId();
            var request = _gateway.CreateScheduledPaymentXml(ValidCard, id, p, new DateTime());
            SendingDebug(request);

            var r = _gateway.SendMessage(request, "unit test");

            // Assert
            Console.WriteLine("First Response");
            Console.WriteLine(r.Print());

            AssertStatusGoodSuccessMarkerNoConnectionIssues(r);

            SendingDebug(request);
            p.Amount *= 2;
            request = _gateway.TriggerPeriodicPaymentXml(id, p);
            r = _gateway.SendMessage(request, "unit test");

            Console.WriteLine("Second Response");
            Console.WriteLine(r.Print());

            // Assert
            AssertStatusGoodSuccessMarkerNoConnectionIssues(r);
        }
    }
}
