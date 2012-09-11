using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Payments.SecurePay
{
    public class SecurePayGateway
    {
        private readonly int _connectionTimeoutSeconds;

        private readonly ICommunicate _endpoint;

        private readonly string _merchantId;

        private readonly string _password;

        private readonly string _apiUri;

        public SecurePayGateway(ICommunicate endpoint, string merchantId, string merchantPassword, string apiUri)
        {
            _endpoint = endpoint;
            _merchantId = merchantId;
            _password = merchantPassword;
            _connectionTimeoutSeconds = 60;
            _apiUri = apiUri;
        }

        public SecurePayMessage SendMessage(string requestMessage, string callingMethod)
        {
            var response = HttpPost(_apiUri, requestMessage);

            ValidateReponse(response, callingMethod);

            return response;
        }

        private void ValidateReponse(SecurePayMessage response, string callingMethod)
        {
            Defend(response.Status.StatusCode != (int)SecurePayStatusCodes.Normal, callingMethod, response);

            var p = response.Periodic;

            if(p == null || p.PeriodicList == null || p.PeriodicList.PeriodicItem == null)
            {
                return;
            }

            foreach(var pi in p.PeriodicList.PeriodicItem)
            {
                Defend(pi.ResponseCode != 0, callingMethod, pi.ResponseCode, pi.ResponseText);
            }
        }

        /// <summary>
        /// Test Method, where message is crafted externally (i.e. unit tests).
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public string SendMessageXml(string requestMessage)
        {
            var response = HttpPost(_apiUri, requestMessage);

            // because this is a test helper we're converting back (those tests were written to check text)
            return response.SerializeObject();
        }

        public SecurePayMessage SingleCharge(SecurePayCardInfo card, int amount, string referenceId)
        {
            var request = SinglePaymentXml(card, amount, referenceId);

            var response = SendMessage(request, "SingleCharge");

            return response;
        }

        public SecurePayMessage CreateCustomerWithCharge(string clientId, SecurePayCardInfo card, SecurePayPayment payment)
        {
            var request = CreateReadyToTriggerPaymentXml(card, clientId, payment);

            var response = SendMessage(request, "CreateCustomerWithCharge");

            return response;
        }

        public SecurePayMessage ChargeExistingCustomer(string clientId, SecurePayPayment payment)
        {
            var request = TriggerPeriodicPaymentXml(clientId, payment);

            var response = SendMessage(request, "ChargeExistingCustomer");

            return response;
        }

        private static void Defend(bool condition, string method, SecurePayMessage response)
        {
            Defend(condition, method, response.Status.StatusCode, response.Status.StatusDescription);
        }

        private static void Defend(bool condition, string method, int statusCode, string statusDescription)
        {
            if (condition)
            {
                throw new SecurePayException(method + " action was unsuccessful", statusCode, statusDescription);
            }
        }

        public string CreateReadyToTriggerPaymentXml(SecurePayCardInfo card, string customerId, SecurePayPayment payment)
        {
            ValidatePayment(payment);

            return new XDocument(
                new XDeclaration("1.0", "utf-8", "no"),
                new XElement("SecurePayMessage",
                    new XElement("MessageInfo",
                        new XElement("messageID", CreateMessageId()),
                        new XElement("messageTimestamp", GetTimeStamp(DateTime.Now)),
                        new XElement("timeoutValue", _connectionTimeoutSeconds),
                        new XElement("apiVersion", "spxml-3.0")), // NOTE <-- Different to Single payments
                    new XElement("MerchantInfo",
                        new XElement("merchantID", _merchantId),
                        new XElement("password", _password)),
                    new XElement("RequestType", "Periodic"), // NOTE <--DIFF
                    new XElement("Periodic",
                        new XElement("PeriodicList", new XAttribute("count", "1"),
                            new XElement("PeriodicItem", new XAttribute("ID", "1"),
                                new XElement("actionType", "add"),
                                new XElement("clientID", customerId),
                                new XElement("CreditCardInfo",
                                    new XElement("cardNumber", card.Number),
                                    new XElement("expiryDate", card.Expiry)),
                                new XElement("amount", payment.Amount),
                                new XElement("currency", payment.Currency.ToUpper()),
                                new XElement("periodicType", "4") // << Triggered Payment
                                ))))).ToStringWithDeclaration();
        }

        public string TriggerPeriodicPaymentXml(string customerId, SecurePayPayment payment)
        {
            ValidatePayment(payment);

            return new XDocument(
                new XDeclaration("1.0", "utf-8", "no"),
                new XElement("SecurePayMessage",
                    new XElement("MessageInfo",
                        new XElement("messageID", CreateMessageId()),
                        new XElement("messageTimestamp", GetTimeStamp(DateTime.Now)),
                        new XElement("timeoutValue", _connectionTimeoutSeconds),
                        new XElement("apiVersion", "spxml-3.0")), // NOTE <-- Different to Single payments
                    new XElement("MerchantInfo",
                        new XElement("merchantID", _merchantId),
                        new XElement("password", _password)),
                    new XElement("RequestType", "Periodic"), // NOTE <--DIFF
                    new XElement("Periodic",
                        new XElement("PeriodicList", new XAttribute("count", "1"),
                            new XElement("PeriodicItem", new XAttribute("ID", "1"),
                                new XElement("actionType", "trigger"),
                                new XElement("clientID", customerId),
                                new XElement("amount", payment.Amount),
                                new XElement("currency", payment.Currency.ToUpper()),
                                new XElement("periodicType", "4") // << Triggered Payment
                                ))))).ToStringWithDeclaration();
        }

        public string TriggerPeriodicPaymentXmlWithMessageId(string messageId, string customerId, SecurePayPayment payment)
        {
            ValidatePayment(payment);

            return new XDocument(
                new XDeclaration("1.0", "utf-8", "no"),
                new XElement("SecurePayMessage",
                    new XElement("MessageInfo",
                        new XElement("messageID", messageId),
                        new XElement("messageTimestamp", GetTimeStamp(DateTime.Now)),
                        new XElement("timeoutValue", _connectionTimeoutSeconds),
                        new XElement("apiVersion", "spxml-3.0")), // NOTE <-- Different to Single payments
                    new XElement("MerchantInfo",
                        new XElement("merchantID", _merchantId),
                        new XElement("password", _password)),
                    new XElement("RequestType", "Periodic"), // NOTE <--DIFF
                    new XElement("Periodic",
                        new XElement("PeriodicList", new XAttribute("count", "1"),
                            new XElement("PeriodicItem", new XAttribute("ID", "1"),
                                new XElement("actionType", "trigger"),
                                new XElement("clientID", customerId),
                                new XElement("amount", payment.Amount),
                                new XElement("currency", payment.Currency.ToUpper()),
                                new XElement("periodicType", "4") // << Triggered Payment
                                ))))).ToStringWithDeclaration();
        }

        public string CreateScheduledPaymentXml(SecurePayCardInfo card, string customerId, SecurePayPayment payment, DateTime startDate)
        {
            ValidatePayment(payment);

            return new XDocument(
                new XDeclaration("1.0", "utf-8", "no"),
                new XElement("SecurePayMessage",
                    new XElement("MessageInfo",
                        new XElement("messageID", CreateMessageId()),
                        new XElement("messageTimestamp", GetTimeStamp(DateTime.Now)),
                        new XElement("timeoutValue", _connectionTimeoutSeconds),
                        new XElement("apiVersion", "spxml-3.0")), // NOTE <-- Different to Single payments
                    new XElement("MerchantInfo",
                        new XElement("merchantID", _merchantId),
                        new XElement("password", _password)),
                    new XElement("RequestType", "Periodic"), // NOTE <--DIFF
                    new XElement("Periodic",
                        new XElement("PeriodicList", new XAttribute("count", "1"),
                            new XElement("PeriodicItem", new XAttribute("ID", "1"),
                                new XElement("actionType", "add"),
                                new XElement("clientID", customerId),
                                new XElement("CreditCardInfo",
                                    new XElement("cardNumber", card.Number),
                                    new XElement("expiryDate", card.Expiry)),
                                new XElement("amount", payment.Amount),
                                new XElement("currency", payment.Currency.ToUpper()),
                                new XElement("startDate", "20120901"),
                                new XElement("paymentInterval", "3"),
                                new XElement("numberOfPayments", 12),
                                new XElement("periodicType", "3") // << Triggered Payment
                                ))))).ToStringWithDeclaration();
        }

        public string SinglePaymentXml(SecurePayCardInfo card, int amount, string purchaseOrderNo)
        {
            return new XDocument(
                new XDeclaration("1.0", "utf-8", "no"),
                new XElement("SecurePayMessage",
                    new XElement("MessageInfo",
                        new XElement("messageID", "757a5be5b84b4d8ab84ec03ebd24af"),
                        new XElement("messageTimestamp", GetTimeStamp(DateTime.Now)),
                        new XElement("timeoutValue", "30"),
                        new XElement("apiVersion", "xml-4.2")),
                    new XElement("MerchantInfo",
                        new XElement("merchantID", _merchantId),
                        new XElement("password", _password)),
                    new XElement("RequestType", "Payment"),
                    new XElement("Payment",
                        new XElement("TxnList", new XAttribute("count", "1"),
                            new XElement("Txn", new XAttribute("ID", "1"),
                                new XElement("txnType", "0"),
                                new XElement("txnSource", "0"),
                                new XElement("amount", amount),
                                new XElement("purchaseOrderNo", purchaseOrderNo),
                                    new XElement("CreditCardInfo",
                                        new XElement("cardNumber", card.Number),
                                        new XElement("expiryDate", card.Expiry))))))).ToStringWithDeclaration();
        }

        public static string GetTimeStamp(DateTime timeStamp)
        {
            const string Format = "{0:yyyy}{0:dd}{0:MM}{0:hh}{0:mm}{0:ss}{0:fff}000{1}{2:000}";

            var offset = TimeZone.CurrentTimeZone.GetUtcOffset(timeStamp);
            var sign = offset.TotalMinutes > 0 ? "+" : "-";

            return String.Format(Format, timeStamp, sign, offset.TotalMinutes);
        }

        public SecurePayMessage HttpPost(string uri, string message)
        {
            return _endpoint.HttpPost(uri, message);
        }

        private void ValidatePayment(SecurePayPayment payment)
        {
            if (payment.Amount <= 0)
            {
                throw new ArgumentException("payment.Amount was zero or negative", "payment");
            }

            if (string.IsNullOrWhiteSpace(payment.Currency) || payment.Currency.Length > 3)
            {
                throw new ArgumentException("payment.Currency is not valid", "payment");
            }
        }

        public static string Sha1SecurePayDetails(string merchantId, string transxPassword, string transxType, string primaryRef, int amount, DateTime timestamp)
        {
            return Sha1SecurePayDetailsHexString(
                merchantId,
                transxPassword,
                transxPassword,
                primaryRef,
                amount.ToString("0.00"),
                timestamp.ToString("YYYYmmDDHHMMSS"));
        }

        public static string Sha1SecurePayDetailsHexString(string merchantId, string transxPassword, string transxType, string primaryRef, string amount, string fpTimestamp)
        {
            var input = merchantId + "|" + transxPassword + "|" + transxType + "|" + primaryRef + "|" + amount + "|" + fpTimestamp;

            return Sha1SecurePayDetailsHexString(input);
        }

        public static string Sha1SecurePayDetailsHexString(string input)
        {
            var algorithm = new SHA1CryptoServiceProvider();

            var hashBytes = algorithm.ComputeHash(Encoding.UTF8.GetBytes(input));

            var hex = BitConverter.ToString(hashBytes);

            return hex.Replace("-", "");
        }

        public static string CreateMessageId()
        {
            return Guid.NewGuid().ToString().Replace("-", "").Substring(0, 30);
        }

        public static string GetClientId()
        {
            return Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
        }
    }
}
