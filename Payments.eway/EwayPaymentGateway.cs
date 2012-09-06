using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;

using Payments.eway.RapidAPI;

namespace Payments.eway
{
    public class EwayPaymentGateway : IEwayPaymentGateway
    {
        private static Authentication GetAuthenticationFromConfiguration()
        {
            return new Authentication
                {
                    Username = ConfigurationManager.AppSettings["Payment.Username"],
                    Password = ConfigurationManager.AppSettings["Payment.Password"],
                    CustomerID = Convert.ToInt32(ConfigurationManager.AppSettings["Payment.CustomerID"])
                };
        }

        /// <summary>
        /// # STEP 1 -- From Guide
        /// </summary>
        public EwayCustomerDetails CreateCustomerWithPaymentRequirement(string redirectUrl, bool redirect, EwayCustomerDetails customer, EwayPayment payment)
        {
            var mode = redirect ? ResponseMode.Redirect : ResponseMode.Return;

            if (string.IsNullOrWhiteSpace(redirectUrl)) throw new ArgumentNullException("redirectUrl", "eWAY requires a redirect url");

            if (payment.TotalAmount <= 0) throw new ArgumentNullException("payment", "payment.TotalAmount requires a value larger than 0");

            var auth = GetAuthenticationFromConfiguration();

            using (var service = new RapidAPISoapClient())
            {
                // When the SaveToken field is set to “true”, and the TokenCustomerID 
                // field is empty, a new Token customer will be created once the payment has been submitted.
                var response = service.CreateAccessCode(new CreateAccessCodeRequest
                    {
                        Authentication = auth,
                        Customer = new Customer
                            {
                                Title = customer.Title,
                                FirstName = customer.FirstName,
                                LastName = customer.LastName,
                                Country = customer.Country.ToLower(),
                                SaveToken = true,
                                TokenCustomerID = null,
                            },
                        RedirectUrl = redirectUrl,
                        ResponseMode = mode,
                        Payment = new Payment
                            {
                                InvoiceDescription = payment.InvoiceDescription,
                                InvoiceNumber = payment.InvoiceNumber,
                                InvoiceReference = payment.InvoiceReference,
                                TotalAmount = payment.TotalAmount
                            }
                    });

                return new EwayCustomerDetails
                    {
                        // Token will not exist yet
                        Token = response.Customer.TokenCustomerID.ToString(),
                        AccessCode = response.AccessCode
                    };
            }
        }

        // ## Step 2 - Pure client side (see Payments.Web test app)

        /// <summary>
        /// # STEP 3 -- From Guide
        /// </summary>
        public EwayResponse GetAccessCodeResult(String accessCode)
        {
            var auth = GetAuthenticationFromConfiguration();

            var request = new GetAccessCodeResultRequest
                {
                    Authentication = auth,
                    AccessCode = accessCode,
                };

            return GetAccessCodeResult(request);
        }

        /// <summary>
        /// # STEP 3 -- From Guide
        /// </summary>
        public EwayResponse GetAccessCodeResult(GetAccessCodeResultRequest request)
        {
            // Create a new instance of the RapidAPI service and send the request
            using (var service = new RapidAPISoapClient())
            {
                
                var response = service.GetAccessCodeResult(request);

                if (string.Compare(response.ResponseCode, "00", true, CultureInfo.CurrentCulture) != 0)
                {
                    var msg = string.Format("Payment Unsuccessful {2} Response Code:{0}{2}ResponseMessage:{1}{2}", response.ResponseCode, response.ResponseMessage, Environment.NewLine);
                    throw new EwayPaymentException(msg);
                }

                return CreateResponse(response);
            }
        }

        public string ChargeExistingCustomer(string token, EwayPayment payment)
        {
            var auth = GetAuthenticationFromConfiguration();

            using (var service = new RapidAPISoapClient())
            {
                // When the SaveToken field is set to “true”, and the TokenCustomerID 
                // field is empty, a new Token customer will be created once the payment has been submitted.
                var response = service.CreateAccessCode(
                        new CreateAccessCodeRequest
                            {
                                Authentication = auth,
                                ResponseMode = ResponseMode.Return,
                                RedirectUrl = "http://google.com/why-would-you-think-I-only-come-from-the-web",
                                Customer = new Customer
                                    {
                                        TokenCustomerID = Int64.Parse(token)
                                    },
                                Payment =
                                    new Payment
                                        {
                                            InvoiceDescription = payment.InvoiceDescription,
                                            InvoiceNumber = payment.InvoiceNumber,
                                            InvoiceReference = payment.InvoiceReference,
                                            TotalAmount = payment.TotalAmount
                                        }
                            });

                //return response.AccessCode;

                var outcome = GetAccessCodeResult(response.AccessCode);

                return outcome.ResponseMessage;
            }
        }

        public string HttpPost(string uri, string parameters)
        {
            // parameters: name1=value1&name2=value2	
            var webRequest = WebRequest.Create(uri);

            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.Method = "POST";
            var bytes = Encoding.ASCII.GetBytes(parameters);

            webRequest.ContentLength = bytes.Length;   //Count bytes to send

            using (var stream = webRequest.GetRequestStream())
            {
                stream.Write(bytes, 0, bytes.Length); //Send it
            }

            using (var webResponse = webRequest.GetResponse())
            {
                var sr = new StreamReader(webResponse.GetResponseStream());

                return sr.ReadToEnd().Trim();
            }
        } 

        private EwayResponse CreateResponse(GetAccessCodeResultResponse response)
        {
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
        }

        public class EwayPaymentException : Exception
        {
            public EwayPaymentException(string msg)
                :base(msg)
            {
            }

            public EwayPaymentException(string msg, Exception inner)
                : base(msg, inner)
            {
            }
        }
    }
}