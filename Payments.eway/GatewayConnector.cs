using System;
using System.Configuration;
using System.IO;
using System.Net;

namespace Payments.eway
{
    /// <summary>
    /// Summary description for GatewayConnector.
    /// Copyright Web Active Corporation Pty Ltd  - All rights reserved. 1998-2006
    /// This code is for exclusive use with the eWAY payment gateway
    /// </summary>
    public class GatewayConnector
    {
        /// <summary>
        /// The Uri of the eWay payment gateway
        /// </summary>
        public string Uri { get; set; }

        public int ConnectionTimeout { get; set; }

        public RebillResponse Response { get; private set; }

        public GatewayConnector(string gatewayUri = "", int timeOut = 0)
        {
            Uri = string.IsNullOrWhiteSpace(gatewayUri) ? ConfigurationManager.AppSettings["ewayGateway"] : gatewayUri;
            ConnectionTimeout = timeOut <= 0 ? 36000 : timeOut;
        }

        /// <summary>
        /// Do the post to the gateway and retrieve the response
        /// </summary>
        /// <param name="paymentRequest">Payment details</param>
        /// <returns>true or false</returns>
        public bool ProcessRequest(RebillPaymentMessage paymentRequest)
        {
            var request = (HttpWebRequest)WebRequest.Create(Uri);
            request.Method = "POST";
            request.Timeout = ConnectionTimeout;
            request.ContentType = "application/x-www-form-urlencoded";
            request.KeepAlive = false;

            var requestBytes = System.Text.Encoding.ASCII.GetBytes(paymentRequest.AsXml());
            request.ContentLength = requestBytes.Length;

            // Send the data out over the wire

            var requestStream = request.GetRequestStream();
            requestStream.Write(requestBytes, 0, requestBytes.Length);
            requestStream.Close();

            HttpWebResponse response = null;

            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException wex)
            {
                // for timeouts etc
                if (response == null)
                {
                    throw new EPayGatewayException { Exception = wex, Error = "Suspected Timeout" };
                }

                // try and get the error text
                var error = ReadResponseStream(response);

                throw new EPayGatewayException { Exception = wex, Error = error };
            }

            // get the response
            if (response == null || response.StatusCode != HttpStatusCode.OK)
            {
                return false;
            }

            Response = new RebillResponse(ReadResponseStream(response));

            return true;
        }

        private string ReadResponseStream(WebResponse response)
        {
            using (var sr = new StreamReader(response.GetResponseStream(), System.Text.Encoding.ASCII))
            {
                var result = sr.ReadToEnd();
                sr.Close();
                return result;
            }
        }
    }

    public class EPayGatewayException : Exception
    {
        public Exception Exception { get; set; }

        public string Error { get; set; }
    }
}