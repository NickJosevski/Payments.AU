using System;

using Payments.eway.RapidAPI;

namespace Payments.eway
{
    public interface IEwayPaymentGateway
    {
        /// <summary>
        /// # STEP 1 -- From Guide
        /// </summary>
        EwayCustomerDetails CreateAndBillCustomer(string redirectUrl, bool redirect, EwayCustomerDetails customer, EwayPayment payment);

        /// <summary>
        /// # STEP 3 -- From Guide
        /// </summary>
        EwayResponse GetAccessCodeResult(String accessCode);

        /// <summary>
        /// # STEP 3 -- From Guide
        /// </summary>
        EwayResponse GetAccessCodeResult(GetAccessCodeResultRequest request);
    }
}