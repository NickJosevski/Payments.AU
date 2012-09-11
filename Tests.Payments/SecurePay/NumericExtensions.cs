using System;

namespace Tests.Payments.SecurePay
{
    public static class NumericExtensions
    {
        public static int ToCents(this decimal amount)
        {
            return (int)(Math.Round(amount, 2) * 100);
        }
    }
}