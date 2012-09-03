using System;

namespace Payments.eway
{
    public class EwayCustomerDetails
    {
        public String Token { get; set; }
        public String Title { get; set; }
        public String FirstName { get; set; }
        public String LastName { get; set; }
        public String Country { get; set; }
    }

    public class EwayPayment
    {
        public string InvoiceDescription { get; set; }

        public string InvoiceNumber { get; set; }

        public string InvoiceReference { get; set; }

        /// <summary>
        /// In Cents
        /// </summary>
        public int TotalAmount { get; set; }
    }
}