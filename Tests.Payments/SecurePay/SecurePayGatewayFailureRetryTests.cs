using System;
using System.Collections.Generic;

using NSubstitute;

using NUnit.Framework;

using Payments.SecurePay;

namespace Tests.Payments.SecurePay
{
    [TestFixture]
    public class SecurePayGatewayFailureRetryTests
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

        private ICommunicate _fakeCommunicationMechanism;

        [TestFixtureSetUp]
        public void Fixture()
        {
            _fakeCommunicationMechanism = Substitute.For<ICommunicate>();

            _gateway = new SecurePayGateway(_fakeCommunicationMechanism, "ABC0001", "abc123", ApiPeriodic);

            _card = new SecurePayCardInfo { Number = "4444333322221111", ExpiryMonth = 10, ExpiryYear = 15 };
        }

        [Test]
        public void GateWayReportsUnableToConnect_ScaleBackSlowlyFor5TriesToSeeIfItIsTransient()
        {
            // Arrange
            SecurePayMessage response = null;
            var clientId = SecurePayGateway.CreateClientId();

            var payment = new SecurePayPayment { Amount = 1151, Currency = "AUD" };

            const string Unable = "Unable To Connect To Server";
            // NOTE: we have max tries of 5, so set up 4 bad, then last will just work
            _fakeCommunicationMechanism.HttpPost("", "").ReturnsForAnyArgs(
                new SecurePayMessage { Status = new SecurePayStatus { StatusCode = 110, StatusDescription = Unable } },
                new SecurePayMessage { Status = new SecurePayStatus { StatusCode = 110, StatusDescription = Unable } },
                new SecurePayMessage { Status = new SecurePayStatus { StatusCode = 110, StatusDescription = Unable } },
                new SecurePayMessage { Status = new SecurePayStatus { StatusCode = 110, StatusDescription = Unable } },
                new SecurePayMessage { Status = new SecurePayStatus { StatusCode = 0, StatusDescription = "Normal" } });

            // Logic for retries
            const int MaxiumTries = 5;
            var tryCount = 0;
            var keepTrying = true;
            
            // RETRY CASES (see codes above)
            var validRetryCodes = new List<int> { 110, 123 };

            // Act
            while (keepTrying)
            {
                tryCount++;
                try
                {
                    Console.WriteLine("tryCount " + tryCount);
                    Console.WriteLine("Sleep for milliseconds before next try " + Math.Floor(Math.Pow((tryCount * 2 ), 1.5) * 2000));

                    response = _gateway.CreateCustomerWithCharge(clientId, _card, payment);

                    keepTrying = (response.Status.StatusCode != 0) || tryCount < MaxiumTries;
                }
                catch (SecurePayException ex)
                {
                    keepTrying = validRetryCodes.Contains(ex.StatusCode);
                }
            }

            Assert.True(tryCount == MaxiumTries);
            Assert.NotNull(response);
            Assert.That(response.Status.StatusCode, Is.EqualTo(0));
        }
    }
}