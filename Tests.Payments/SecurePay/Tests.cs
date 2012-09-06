using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

using Payments.SecurePay;

namespace Tests.Payments.SecurePay
{
    [TestFixture]
    public class Tests
    {
        public static string TestUrl = "https://payment.securepay.com.au/test/v2/invoice";

        [Test]
        public void Sha1_Hash_SecurePayCheckField()
        {
            // Arrange
            var input = "ABC0001|abc123|0|Picnic-Buy|1337.00|20120906140000";

            // Act

            var result = Core.Sha1SecurePayDetailsHexString(input);

            // Assert
            Assert.AreEqual("770258D70DF0DFA6E186D5B6F7635D870ABFEA2B", result);
        }

        [Test]
        public void TestName()
        {
            // Arrange
            
            // Act

            // Assert
        }
    }
}
