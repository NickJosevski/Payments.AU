using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Payments.eway.RapidAPI;

namespace Payments.eway
{
    public class EwayPayment
    {
        public static void Real()
        {
            // Create a request to retrieve the results from eWAY
            var request = new GetAccessCodeResultRequest
                {
                    //AccessCode = Request.QueryString["AccessCode"].ToString(),
                    //AccessCode = ConfigurationManager.AppSettings["Payment.AccessCode"]
                };

            // Authentication
            var auth = new Authentication
                {
                    Username = ConfigurationManager.AppSettings["Payment.Username"],
                    Password = ConfigurationManager.AppSettings["Payment.Password"],
                    CustomerID = Convert.ToInt32(ConfigurationManager.AppSettings["Payment.CustomerID"])
                };

            request.Authentication = auth;

        }

        public static bool ReturnsTrue()
        {
            return true;
        }

        /*
         * Response Codes (Based on input dolar amount)
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

        public static void TestWith()
        {

        }

        public static EwayResponse CreateCustomer(String accessCode)
        {
            var auth = new Authentication
            {
                Username = ConfigurationManager.AppSettings["Payment.Username"],
                Password = ConfigurationManager.AppSettings["Payment.Password"],
                CustomerID = Convert.ToInt32(ConfigurationManager.AppSettings["Payment.CustomerID"])
            };

            var request = new GetAccessCodeResultRequest
            {
                Authentication = auth,
                AccessCode = accessCode,
            };

            return CreateCustomer(request);
        }

        public static EwayResponse CreateCustomer(GetAccessCodeResultRequest request)
        {
            // Create a new instance of the RapidAPI service and send the request
            using (var service = new RapidAPISoapClient())
            {
                /*try
                {*/
                    // GetAccessCodeResult via Gateway
                    var response = service.GetAccessCodeResult(request);

                    return new EwayResponse
                    {
                        // Use the response object to display the details returned by eWAY
                        AccessCode = response.AccessCode,
                        AuthorisationCode = response.AuthorisationCode,
                        BeagleScore = response.BeagleScore.HasValue ? response.BeagleScore.Value.ToString() : string.Empty,
                        InvoiceNumber = response.InvoiceNumber,
                        InvoiceReference = response.InvoiceReference,
                        Option1 = response.Option1,
                        Option2 = response.Option2,
                        Option3 = response.Option3,
                        ResponseCode = response.ResponseCode,
                        ResponseMessage = response.ResponseMessage,

                        TokenCustomerID = response.TokenCustomerID != null ? response.TokenCustomerID.ToString() : string.Empty,
                        TotalAmount = response.TotalAmount != null ? response.TotalAmount.Value.ToString() : string.Empty,
                        TransactionID = response.TransactionID != null ? response.TransactionID.Value.ToString() : string.Empty,
                        TransactionStatus = response.TransactionStatus != null ? response.TransactionStatus.Value.ToString() : string.Empty,
                    };
                /*}
                catch (Exception ex)
                {
                    return new EwayResponse
                        {
                            ResponseCode = "Exception",
                            ResponseMessage = ex.Message
                        };
                }*/
            }
        }
    }
}
