using System;
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

            var result = EwayPayment.GetAccessCodeResult(request);

            var responseCode = result.ResponseCode;
            var token = result.TokenCustomerID;

            Assert.NotNull(responseCode);
            Assert.NotNull(token);
        }

        [TestCase("AF9802EkhvFnYNhzOr5HWHrvGAp8PxIVKc4rb3RrzHFN8743hd5", "00")]
        [TestCase("DOq0XSxG4PilMm501JxHVAFSYLO2UT72wBn25bIzz27345873df", "05")]
        //[TestCase("E1HEZyH7uC3ROFOQMV791FrWXarSiK9igYgKJ8m0qAAAoP22bjB")]
        //[TestCase("E2pHRUx0dP1cW2n1tb9dJOB21NzcPgZ4EBAro_w6wXYNmV1Oh3p")]
        //[TestCase("E3DD7Ci0UQqlBmO3xCPRd7g94BOAG0bgrez5sl28xnQsIVkXOND")]
        //[TestCase("E4hMb9PYwqJEFXDLjohvAeUdJhGzMKMRaDtBovrXPRFmSegVyk0")]
        public void GetAccessCodeResult_ViaAccessCode(string accessCode, string expectedCode)
        {
            var result = EwayPayment.GetAccessCodeResult(accessCode);

            var responseCode = result.ResponseCode;
            var token = result.TokenCustomerID;

            Assert.AreEqual(expectedCode, responseCode);
            Assert.NotNull(token);
            Console.WriteLine("Token: " + token);
        }

        [Test]
        public void CreateCustomer_()
        {
            // Arrange
            var result = EwayPayment.CreateAndBillCustomer("http://test.com/asdf");
            
            // Act

            // Assert
            Assert.IsNotNull(result);
        }
    }
}
