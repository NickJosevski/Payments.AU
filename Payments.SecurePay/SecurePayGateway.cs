using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Payments.SecurePay
{
    public enum ActionType
    {
        // Must be lowercase
        add,
        trigger,
        delete
    }

    public static class StringExtensions
    {
        public static SecureString ToSecureString(this string src)
        {
            var secure = new SecureString();

            src.ToCharArray().ToList().ForEach(secure.AppendChar);

            return secure;
        }
    }

    public class SecurePayGateway
    {
        private int _connectionTimeoutSeconds;

        private readonly string _merchantId;

        private readonly string _password;

        //public const string SecurePay = "https://test.securepay.com.au/xmlapi/periodic";
        //public const string SecurePay = "https://test.securepay.com.au/xmlapi/periodic";
        public string ApiEndpoint = "https://test.securepay.com.au/xmlapi/payment";

        public SecurePayGateway()
        {
            //TODO: get from configuration / secure location
            _merchantId = "ABC0001";
            _password = "abc123";
            _connectionTimeoutSeconds = 30;
        }

        public SecurePayGateway(string merchantId, string merchantPassword)
        {
            //TODO: get from configuration / secure location
            _merchantId = merchantId;
            _password = merchantPassword;
            _connectionTimeoutSeconds = 30;
        }

        public SecurePayGateway(string url)
        {
            ApiEndpoint = url;
        }

        /// <summary>
        /// Test Method, where message is crafted externally (i.e. unit tests).
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public string SendMessage(string requestMessage)
        {
            return HttpPost(ApiEndpoint, requestMessage);
        }

        public bool SingleCharge(CardInfo card, decimal amount, string referenceId)
        {
            var request = SinglePaymentXml(card, amount.ToCents(), referenceId);

            var response = SendMessage(request);

            return response.Contains("<statusCode>0") && response.Contains("<statusDescription>Normal");
        }

        public bool CreateCustomerWithCharge(string clientId, CardInfo card, Payment payment)
        {
            var request = CreateReadyToTriggerPaymentXml(card, clientId, payment);

            var response = SendMessage(request);

            return response.Contains("<statusCode>0") && response.Contains("<statusDescription>Normal");
        }

        public bool ChargeExistingCustomer(string clientId, Payment payment)
        {
            var request = TriggerPeriodicPaymentXml(clientId, payment);

            var response = SendMessage(request);

            return response.Contains("<statusCode>0") && response.Contains("<statusDescription>Normal");
        }

        public string CreateReadyToTriggerPaymentXml(CardInfo card, string customerId, Payment payment)
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
                                new XElement("amount", payment.Amount.ToCents()),
                                new XElement("currency", payment.Currency.ToUpper()),
                                new XElement("periodicType", "4") // << Triggered Payment
                                ))))).ToStringWithDeclaration();
        }

        public string TriggerPeriodicPaymentXml(string customerId, Payment payment)
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
                                new XElement("amount", payment.Amount.ToCents()),
                                new XElement("currency", payment.Currency.ToUpper()),
                                new XElement("periodicType", "4") // << Triggered Payment
                                ))))).ToStringWithDeclaration();
        }

        public string TriggerPeriodicPaymentXmlWithMessageId(string messageId, string customerId, Payment payment)
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
                                new XElement("amount", payment.Amount.ToCents()),
                                new XElement("currency", payment.Currency.ToUpper()),
                                new XElement("periodicType", "4") // << Triggered Payment
                                ))))).ToStringWithDeclaration();
        }

        public string CreateScheduledPaymentXml(CardInfo card, string customerId, Payment payment, DateTime startDate)
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
                                new XElement("amount", payment.Amount.ToCents()),
                                new XElement("currency", payment.Currency.ToUpper()),
                                new XElement("startDate", "20120901"),
                                new XElement("paymentInterval", "3"),
                                new XElement("numberOfPayments", 12),
                                new XElement("periodicType", "3") // << Triggered Payment
                                ))))).ToStringWithDeclaration();
        }

        public static string CreateMessageId()
        {
            return Guid.NewGuid().ToString().Replace("-", "").Substring(0, 30);
        }

        public static string SinglePaymentXml(CardInfo card, int amount, string purchaseOrderNo)
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
                        new XElement("merchantID", "ABC0001"),
                        new XElement("password", "abc123")),
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

        public string HttpPost(string uri, string message)
        {
            ApiDebug(uri);
            var request = (HttpWebRequest)WebRequest.Create(uri);

            request.Method = "POST";
            request.ContentType = "application/xml";
            request.Accept = "application/xml";

            var bytes = Encoding.UTF8.GetBytes(message);

            request.ContentLength = bytes.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(bytes, 0, bytes.Length);
            }

            using (var response = (HttpWebResponse)request.GetResponse())
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                return reader.ReadToEnd().Trim();
            }
        }

        private void ValidatePayment(Payment payment)
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

        public void ApiDebug(string msg)
        {
            Console.WriteLine("");
            Console.WriteLine("####   ####   ####   ####");
            Console.WriteLine("communicating via:[{0}]", msg);
            Console.WriteLine("####   ####   ####   ####");
            Console.WriteLine("");
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

        public static string GetClientId()
        {
            return Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
        }
    }

    public static class XmlHelpers
    {
        public static string ToStringWithDeclaration(this XDocument doc)
        {
            if (doc == null)
            {
                throw new ArgumentNullException("doc");
            }
            var builder = new StringBuilder();
            using (var writer = new Utf8StringWriter(builder))
            {
                doc.Save(writer);
            }
            return builder.ToString();
        }

        public static string ExceptBlanks(this string str)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < str.Length; i++)
            {
                var c = str[i];
                switch (c)
                {
                    case '\r':
                    case '\n':
                    case '\t':
                    case ' ':
                        continue;
                    default:
                        sb.Append(c);
                        break;
                }
            }
            return sb.ToString();
        }
    }

    public sealed class Utf8StringWriter : StringWriter
    {
        public Utf8StringWriter()
        { }

        public Utf8StringWriter(StringBuilder sb)
            : base(sb) { }

        public override Encoding Encoding { get { return Encoding.UTF8; } }
    }

    public static class NumericExtensions
    {
        public static int ToCents(this decimal amount)
        {
            return (int)(Math.Round(amount, 2) * 100);
        }
    }
}
