﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Payments.SecurePay
{
    public interface ISecurePayGateway
    {
        SecurePayMessage SingleCharge(SecurePayCardInfo card, SecurePayPayment payment, string referenceId);

        SecurePayMessage CreateCustomerWithCharge(string clientId, SecurePayCardInfo card, SecurePayPayment payment);

        SecurePayMessage ChargeExistingCustomer(string clientId, SecurePayPayment payment);
    }

    public class SecurePayGateway : ISecurePayGateway
    {
        private readonly int _connectionTimeoutSeconds;

        private readonly ICommunicate _endpoint;

        private readonly string _merchantId;

        private readonly string _password;

        private readonly string _apiUri;

        // There may need to be a better way to handle these
        public static List<int> ValidSuccessResponseCode = new List<int>
            {
                (int)SecurePayStatusCodes.Normal,
                (int)SecurePayStatusCodes.Approved,
                (int)SecurePayStatusCodes.ApprovedAnz
            };

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
            var response = _endpoint.HttpPost(_apiUri, requestMessage);

            ValidateReponse(response, callingMethod);

            return response;
        }

        public SecurePayMessage SingleCharge(SecurePayCardInfo card, SecurePayPayment payment, string referenceId)
        {
            card.ValidateExpiry();

            var request = SinglePaymentXml(card, payment, referenceId);

            var response = SendMessage(request, "SingleCharge");

            return response;
        }

        public SecurePayMessage CreateCustomerWithCharge(string clientId, SecurePayCardInfo card, SecurePayPayment payment)
        {
            card.ValidateExpiry();

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
                                    new XElement("expiryDate", card.GetExpiry())),
                                new XElement("amount", payment.Amount),
                                new XElement("currency", payment.Currency.ToUpper()),
                                new XElement("periodicType", "4") // << Triggered Payment
                                ))))).ToStringWithDeclaration();
        }

        /// <summary>
        /// TODO: replace this with creation of a SecurePayMessage object
        /// </summary>
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

        /// <summary>
        /// TODO: replace this with creation of a SecurePayMessage object
        /// </summary>
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

        /// <summary>
        /// TODO: replace this with creation of a SecurePayMessage object
        /// </summary>
        public string CreateScheduledPaymentXml(SecurePayCardInfo card, string customerId, SecurePayPayment payment, DateTime startDate)
        {
            ValidatePayment(payment);
            card.ValidateExpiry();

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
                                    new XElement("expiryDate", card.GetExpiry())),
                                new XElement("amount", payment.Amount),
                                new XElement("currency", payment.Currency.ToUpper()),
                                new XElement("startDate", "20120901"),
                                new XElement("paymentInterval", "3"),
                                new XElement("numberOfPayments", 12),
                                new XElement("periodicType", "3") // << Triggered Payment
                            ))))).ToStringWithDeclaration();
        }

        /// <summary>
        /// TODO: replace this with creation of a SecurePayMessage object
        /// </summary>
        public string SinglePaymentXml(SecurePayCardInfo card, SecurePayPayment payment, string purchaseOrderNo)
        {
            card.ValidateExpiry();

            return new XDocument(
                new XDeclaration("1.0", "utf-8", "no"),
                new XElement("SecurePayMessage",
                    new XElement("MessageInfo",
                        new XElement("messageID", "757a5be5b84b4d8ab84ec03ebd24af"),
                        new XElement("messageTimestamp", GetTimeStamp(DateTime.Now)),
                        new XElement("timeoutValue", _connectionTimeoutSeconds),
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
                                new XElement("amount", payment.Amount),
                                new XElement("currency", payment.Currency.ToUpper()),
                                new XElement("purchaseOrderNo", purchaseOrderNo),
                                    new XElement("CreditCardInfo",
                                        new XElement("cardNumber", card.Number),
                                        new XElement("expiryDate", card.GetExpiry())
                                    )))))).ToStringWithDeclaration();
        }

        public static string GetTimeStamp(DateTime timeStamp)
        {
            const string Format = "{0:yyyy}{0:dd}{0:MM}{0:hh}{0:mm}{0:ss}{0:fff}000{1}{2:000}";

            var offset = TimeZone.CurrentTimeZone.GetUtcOffset(timeStamp);
            var sign = offset.TotalMinutes > 0 ? "+" : "-";

            return String.Format(Format, timeStamp, sign, offset.TotalMinutes);
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
            var randomArray = new byte[128];

            new RNGCryptoServiceProvider().GetBytes(randomArray);

            return RemoveNonAlphaNumeric(Convert.ToBase64String(randomArray)).Substring(0, 30);
        }

        public static string CreateClientId()
        {
            var randomArray = new byte[128];

            new RNGCryptoServiceProvider().GetBytes(randomArray);

            return RemoveNonAlphaNumeric(Convert.ToBase64String(randomArray)).Substring(0, 20);
        }

        private static string RemoveNonAlphaNumeric(string str)
        {
            return new Regex("[^a-zA-Z0-9 -]").Replace(str, "");
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

        private void ValidateReponse(SecurePayMessage response, string callingMethod)
        {
            Defend(!IsAnApprovedSuccessResponseCode(response.Status.StatusCode), callingMethod, response);

            var p = response.Periodic;

            if (p == null || p.PeriodicList == null || p.PeriodicList.PeriodicItem == null)
            {
                return;
            }

            foreach (var pi in p.PeriodicList.PeriodicItem)
            {
                Defend(!IsAnApprovedSuccessResponseCode(pi.ResponseCode), callingMethod, pi.ResponseCode, pi.ResponseText);
            }
        }

        private static bool IsAnApprovedSuccessResponseCode(int codeToCheck)
        {
            return ValidSuccessResponseCode.Contains(codeToCheck);
        }

        private static void Defend(bool condition, string method, SecurePayMessage response)
        {
            Defend(condition, method, response.Status.StatusCode, response.Status.StatusDescription);
        }

        private static void Defend(bool condition, string method, int statusCode, string statusDescription)
        {
            if (condition)
            {
                throw new SecurePayException(method + " action was unsuccessful", (SecurePayStatusCodes)statusCode, statusDescription);
            }
        }
    }
}
