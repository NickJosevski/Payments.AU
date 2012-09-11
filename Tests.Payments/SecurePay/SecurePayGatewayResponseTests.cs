using System;
using System.Collections.Generic;

using NSubstitute;

using NUnit.Framework;

using Payments.SecurePay;

namespace Tests.Payments.SecurePay
{
    [TestFixture]
    public class SecurePayGatewayResponseTests : GatewayTests
    {
        public const string ApiPeriodic = "https://test.securepay.com.au/xmlapi/periodic";

        private SecurePayCardInfo _card;

        private SecurePayGateway _gateway;

        [TestFixtureSetUp]
        public void Fixture()
        {
            _gateway = new SecurePayGateway(new SecurePayEndpoint(), ApiPeriodic);

            _card = new SecurePayCardInfo { Number = "4444333322221111", Expiry = "10/15" };
        }

        [Test]
        public void Periodic_CreateAndCharge_SuccessfulCase()
        {
            // Arrange
            var clientId = SecurePayGateway.GetClientId();

            var payment = new SecurePayPayment { Amount = 1000, Currency = "AUD" };

            _gateway.CreateCustomerWithCharge(clientId, _card, payment);

            // Act
            var response = _gateway.ChargeExistingCustomer(clientId, payment);

            Console.WriteLine(PrintXml(response));
            // Assert

            Assert.That(response.Status.StatusCode, Is.EqualTo(0));
            Assert.That(response.Status.StatusDescription, Is.EqualTo("Normal"));
        }

        [Test]
        public void Periodic_CreateAndCharge_UnSuccessfulCase_InvalidAmount()
        {
            // Arrange
            var clientId = SecurePayGateway.GetClientId();

            var payment = new SecurePayPayment { Amount = 1013, Currency = "AUD" };

            _gateway.CreateCustomerWithCharge(clientId, _card, payment);

            // Act
            var exception = Assert.Throws<SecurePayException>(
                () => _gateway.ChargeExistingCustomer(clientId, payment));

            // Assert
            Assert.That(exception.StatusCode, Is.EqualTo(13));
            Assert.That(exception.StatusDescription, Is.EqualTo("Invalid Amount"));
        }

        [Test]
        public void Periodic_CreateAndCharge_UnSuccessfulCase_InsufficientFunds()
        {
            // Arrange
            var clientId = SecurePayGateway.GetClientId();

            var payment = new SecurePayPayment { Amount = 1051, Currency = "AUD" };

            _gateway.CreateCustomerWithCharge(clientId, _card, payment);

            // Act
            var exception = Assert.Throws<SecurePayException>(
                () => _gateway.ChargeExistingCustomer(clientId, payment));

            // Assert
            Assert.That(exception.StatusCode, Is.EqualTo(51));
            Assert.That(exception.StatusDescription, Is.EqualTo("Insufficient Funds"));
        }

        [Test]
        public void Periodic_CreateAndCharge_UnSuccessfulCase_ExpiredCard()
        {
            // Arrange
            var clientId = SecurePayGateway.GetClientId();

            var payment = new SecurePayPayment { Amount = 1054, Currency = "AUD" };

            _gateway.CreateCustomerWithCharge(clientId, _card, payment);

            // Act
            var exception = Assert.Throws<SecurePayException>(
                () => _gateway.ChargeExistingCustomer(clientId, payment));

            // Assert
            Assert.That(exception.StatusCode, Is.EqualTo(54));
            Assert.That(exception.StatusDescription, Is.EqualTo("Expired Card"));
        }

        [Test]
        public void Periodic_CreateAndCharge_UnSuccessfulCase_Error()
        {
            // Arrange
            var clientId = SecurePayGateway.GetClientId();

            var payment = new SecurePayPayment { Amount = 1052, Currency = "AUD" };

            _gateway.CreateCustomerWithCharge(clientId, _card, payment);

            // Act
            var exception = Assert.Throws<SecurePayException>(
                () => _gateway.ChargeExistingCustomer(clientId, payment));

            // Assert
            Assert.That(exception.StatusCode, Is.EqualTo(52));
            Assert.That(exception.StatusDescription, Is.EqualTo("No Cheque Account"));
        }
    }

    [TestFixture]
    public class SecurePayGatewayFailureRetryTests : GatewayTests
    {
        /*
         * NOTE: This retry logic does not form part of the gateway logic, 
         * as I believe it makes more sense to have it in your own application logic, 
         * so you can control it better.
         * 
         * Some tests to demonstrate how it works follow.
         */

        /*
         RETRY CASES:
         >> 110 Unable To Connect To Server
               Produced by SecurePay Client API when unable to establish connection to SecurePay Payment Gateway
         >> 123 Gateway Timeout 
               Produced by SecurePay Payment Gateway when no response to the transaction has been received from bank gateway within predefined time period
         DO NOT RETRY:
         >> 124 Gateway Connection Aborted During Transaction 
               Produced by SecurePay Payment Gateway when  connection to bank gateway is lost after the payment transaction has been sent
         */

        public const string ApiPeriodic = "https://test.securepay.com.au/xmlapi/periodic";

        private SecurePayCardInfo _card;

        private SecurePayGateway _gateway;

        private ISecurePayEndpoint _fakeGateway;

        [TestFixtureSetUp]
        public void Fixture()
        {
            _fakeGateway = Substitute.For<ISecurePayEndpoint>();

            _gateway = new SecurePayGateway(_fakeGateway, ApiPeriodic);

            _card = new SecurePayCardInfo { Number = "4444333322221111", Expiry = "10/15" };
        }

        [Test]
        public void GateWayReportsUnableToConnect_ScaleBackSlowlyFor5TriesToSeeIfItIsTransient()
        {
            // Arrange
            SecurePayMessage response = null;
            var clientId = SecurePayGateway.GetClientId();

            var payment = new SecurePayPayment { Amount = 1151, Currency = "AUD" };

            // Act
            const string txt = "Unable To Connect To Server";
            // NOTE: we have max tries of 5, so set up 4 bad, then last will just work
            _fakeGateway.HttpPost("", "").ReturnsForAnyArgs(
                new SecurePayMessage { Status = new SecurePayStatus { StatusCode = 110, StatusDescription = txt } },
                new SecurePayMessage { Status = new SecurePayStatus { StatusCode = 110, StatusDescription = txt } },
                new SecurePayMessage { Status = new SecurePayStatus { StatusCode = 110, StatusDescription = txt } },
                new SecurePayMessage { Status = new SecurePayStatus { StatusCode = 110, StatusDescription = txt } },
                new SecurePayMessage { Status = new SecurePayStatus { StatusCode = 0, StatusDescription = "Normal" } });

            // 5th time good

            // Logic for re-trys
            const int tryMax = 5;
            var tryCount = 0;
            var keepTrying = true;
            
            // RETRY CASES (see codes above)
            var validRetryCodes = new List<int> { 110, 123 };
            
            while (keepTrying)
            {
                tryCount++;
                try
                {
                    Console.WriteLine("tryCount " + tryCount);
                    Console.WriteLine("Sleep for milliseconds before next try " + Math.Floor(Math.Pow((tryCount * 2 ), 1.5) * 2000));

                    response = _gateway.CreateCustomerWithCharge(clientId, _card, payment);

                    keepTrying = (response.Status.StatusCode != 0) || tryCount < tryMax;
                }
                catch (SecurePayException ex)
                {
                    if (validRetryCodes.Contains(ex.StatusCode)) keepTrying = true;
                }
            }

            Assert.True(tryCount == tryMax);
            Assert.NotNull(response);
            Assert.That(response.Status.StatusCode, Is.EqualTo(0));
        }
    }
}