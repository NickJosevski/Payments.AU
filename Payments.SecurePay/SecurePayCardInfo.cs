using System;
using System.ComponentModel;

namespace Payments.SecurePay
{
    public class SecurePayCardInfo
    {
        public string Number { get; set; }

        public int ExpiryMonth { get; set; }

        public int ExpiryYear { get; set; }

        public int ValidationCode { get; set; }

        /// <summary>
        /// Being very generous here, taking YYYY and YY (a YYY value will just work, but UI should prevent that)
        /// </summary>
        /// <returns></returns>
        public string GetExpiry()
        {
            var year = ExpiryYear > 99 ? ExpiryYear % 100 : ExpiryYear;

            return string.Format(@"{0:00}/{1:00}", ExpiryMonth, year);
        }

        public bool ValidateExpiry()
        {
            if (ExpiryMonth < 1 || ExpiryMonth > 12)
                throw new SecurePayException("The supplied Expiry Month is not between 1 and 12");

            if (ExpiryYear < 0 || ExpiryYear > 9999)
                throw new SecurePayException("The supplied Expiry Year is not valid expecting format YY or YYYY");

            return true;
        }
    }
}