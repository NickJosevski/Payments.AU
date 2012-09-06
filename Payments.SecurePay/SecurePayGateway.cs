using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Payments.SecurePay
{
    public class SecurePayGateway
    {
        public const string SecurePay = "https://test.securepay.com.au/xmlapi/periodic";
        //public const string SecurePay = "https://test.securepay.com.au/xmlapi/periodic";
        //public const string SecurePay = "https://test.securepay.com.au/xmlapi/payment";

        public string ChargeCustomer(CardInfo card, string requestMessage/*remove this param we will construct it*/)
        {
            return HttpPost(SecurePay, requestMessage);
        }

        public string HttpPost(string uri, string message)
        {
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
    }

    public class CardInfo
    {
        
    }
}
