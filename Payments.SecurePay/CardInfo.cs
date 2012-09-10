namespace Payments.SecurePay
{
    public class CardInfo
    {
        public string Number { get; set; }

        public string ExpiryMonth { get; set; }

        public string ExpiryYear { get; set; }

        public string Expiry { get; set; }
    }

    public class Payment
    {
        public decimal Amount { get; set; }

        public string Currency { get; set; }
    }
}