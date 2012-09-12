using System;

using NUnit.Framework;

using Payments.SecurePay;

namespace Tests.Payments.SecurePay
{
    [TestFixture]
    public class SecurePayGatewayResponseTests
    {
        public const string ApiPeriodic = "https://test.securepay.com.au/xmlapi/periodic";

        private SecurePayCardInfo _card;

        private SecurePayGateway _gateway;

        [TestFixtureSetUp]
        public void Fixture()
        {
            _gateway = new SecurePayGateway(new SecurePayWebCommunication(), "ABC0001", "abc123", ApiPeriodic);

            _card = new SecurePayCardInfo { Number = "4444333322221111", Expiry = "10/15" };
        }

        [Test]
        public void Periodic_CreateAndCharge_SuccessfulCase()
        {
            // Arrange
            var clientId = SecurePayGateway.CreateClientId();

            var payment = new SecurePayPayment { Amount = 1000, Currency = "AUD" };

            _gateway.CreateCustomerWithCharge(clientId, _card, payment);

            // Act
            var response = _gateway.ChargeExistingCustomer(clientId, payment);

            Console.WriteLine(response.Print());
            // Assert

            Assert.That(response.Status.StatusCode, Is.EqualTo(0));
            Assert.That(response.Status.StatusDescription, Is.EqualTo("Normal"));
        }

        [Test]
        public void Periodic_CreateAndCharge_UnSuccessfulCase_InvalidAmount()
        {
            // Arrange
            var clientId = SecurePayGateway.CreateClientId();

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
            var clientId = SecurePayGateway.CreateClientId();

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
            var clientId = SecurePayGateway.CreateClientId();

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
            var clientId = SecurePayGateway.CreateClientId();

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
}