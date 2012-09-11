using System.IO;
using System.Net;
using System.Text;

namespace Payments.SecurePay
{
    /// <summary>
    /// I Communicate - wrapping the WebRequest and HttpWebResponse
    /// Allows for use without real web connection
    /// </summary>
    public interface ICommunicate
    {
        SecurePayMessage HttpPost(string uri, string message);
    }

    /// <summary>
    /// Purpose is to create WebRequest and receive HttpWebResponse
    /// </summary>
    public class SecurePayWebCommunication : ICommunicate
    {
        public SecurePayMessage HttpPost(string uri, string message)
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
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    return reader.ReadToEnd().Trim().As<SecurePayMessage>();
                }
            }
        }
    }
}