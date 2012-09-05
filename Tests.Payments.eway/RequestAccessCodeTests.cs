﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using Payments.eway;
using Payments.eway.RapidAPI;

namespace Tests.Payments.eway
{
    [TestFixture]
    public class RequestAccessCodeTests
    {
        private EwayPaymentGateway _eway;

        [SetUp]
        public void Setup()
        {
            _eway = new EwayPaymentGateway();
        }

        [Test]
        public void BasicConnectionTest()
        {
            var auth = new Authentication
            {
                Username = "test@eway.com.au",
                Password = "test123",
                CustomerID = 87654321
            };

            var request = new GetAccessCodeResultRequest
            {
                Authentication = auth,
                AccessCode = "AF9802EkhvFnYNhzOr5HWHrvGAp8PxIVKc4rb3RrzHFN8743hd5",
            };

            var result = _eway.GetAccessCodeResult(request);

            var responseCode = result.ResponseCode;
            var token = result.TokenCustomerID;

            Assert.NotNull(responseCode);
            Assert.NotNull(token);
        }

        /*
         * Response Codes (Based on input dollar amount)
         * 00 - Transaction Approved
         * 06 - Error
         * 51 - Insufficient Funds
         * 96 - System Error
         * 54 - Expired Pin
         */

        /*
        AccessCodes:  
         * AF9802EkhvFnYNhzOr5HWHrvGAp8PxIVKc4rb3RrzHFN8743hd5 (returns an Approved result)
         * DOq0XSxG4PilMm501JxHVAFSYLO2UT72wBn25bIzz27345873df (returns a Declined result)
         * E1HEZyH7uC3ROFOQMV791FrWXarSiK9igYgKJ8m0qAAAoP22bjB (returns an Invalid Card error)
         * E2pHRUx0dP1cW2n1tb9dJOB21NzcPgZ4EBAro_w6wXYNmV1Oh3p (returns an Invalid Card Holder error)
         * E3DD7Ci0UQqlBmO3xCPRd7g94BOAG0bgrez5sl28xnQsIVkXOND (returns an Invalid Expiry Date error)
         * E4hMb9PYwqJEFXDLjohvAeUdJhGzMKMRaDtBovrXPRFmSegVyk0 (returns an Invalid CVN error)
         */


        //[TestCase("AF9802EkhvFnYNhzOr5HWHrvGAp8PxIVKc4rb3RrzHFN8743hd5", "00")]
        //[TestCase("DOq0XSxG4PilMm501JxHVAFSYLO2UT72wBn25bIzz27345873df", "05")]
        [TestCase("C3AB9uGyo6QusEpJk0oVOz-VEcyKxeCd0-iepdwO_PUJti6f3SwZvu-cfWmUxvaHs3E9yqf9_i1UYzYLN3ghHgisrBiCd7Gfdej0tWOQE3oOyWjlLB1IgHBFrAQ-krfE96mObKoOhjCgwEvwnZVDDyd7bsw==", "00")]
        //[TestCase("E1HEZyH7uC3ROFOQMV791FrWXarSiK9igYgKJ8m0qAAAoP22bjB")]
        //[TestCase("E2pHRUx0dP1cW2n1tb9dJOB21NzcPgZ4EBAro_w6wXYNmV1Oh3p")]
        //[TestCase("E3DD7Ci0UQqlBmO3xCPRd7g94BOAG0bgrez5sl28xnQsIVkXOND")]
        //[TestCase("E4hMb9PYwqJEFXDLjohvAeUdJhGzMKMRaDtBovrXPRFmSegVyk0")]
        public void GetAccessCodeResult_ViaAccessCode(string accessCode, string expectedCode)
        {
            var result = _eway.GetAccessCodeResult(accessCode);

            var responseCode = result.ResponseCode;
            var token = result.TokenCustomerID;

            Console.WriteLine("Token: " + token);
            Assert.IsEmpty(result.ResponseMessage);
            Assert.AreEqual(expectedCode, responseCode);
        }

        [Test]
        public void EwayPaymentGateway_CreateAndBillCustomer()
        {
            // Arrange
            var customer = new EwayCustomerDetails
            {
                Title = "Mr.",
                FirstName = "Just",
                LastName = "SetupTheCustomer",
                Country = "au",
            };

            var payment = new EwayPayment
            {
                InvoiceDescription = "Customer Created",
                InvoiceNumber = Guid.NewGuid().ToString(),
                InvoiceReference = Guid.NewGuid().ToString(),
                TotalAmount = 50000
            };

            // Act
            var result = _eway.CreateCustomerWithPaymentRequirement("http://localhost:51868/PaymentComplete/Good", true, customer, payment);

            // Assert
            Assert.IsNotNull(result);
            Console.WriteLine("AccessCode: " + result.AccessCode);
            Console.WriteLine("Token: " + result.Token);
        }

    }
}
