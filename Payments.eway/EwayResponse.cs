namespace Payments.eway
{
    public class EwayResponse
    {
        public string AccessCode { get; set; }

        public string AuthorisationCode { get; set; }

        public string BeagleScore { get; set; }

        public string InvoiceNumber { get; set; }

        public string InvoiceReference { get; set; }

        public string Option1 { get; set; }

        public string Option2 { get; set; }

        public string Option3 { get; set; }

        public string ResponseCode { get; set; }

        public string ResponseMessage { get; set; }

        public string TokenCustomerID { get; set; }

        public string TotalAmount { get; set; }

        public string TransactionID { get; set; }

        public string TransactionStatus { get; set; }
    }
}