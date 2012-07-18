using System;
using System.IO;
using System.Xml;

namespace Payments.eway
{
    public class RebillResponse : GatewayResponse
    {
        public string Result { get; set; }

        public string ErrorSeverity { get; set; }

        public RebillResponse(string xmlString)
        {
            var xmlDoc = new XmlDocument();

            xmlDoc.LoadXml(xmlString);

            if (!xmlDoc.HasChildNodes)
            {
                return;
            }

            Result = SelectResponseDetailNodeText(xmlDoc, "Result");
            Error = SelectResponseDetailNodeText(xmlDoc, "ErrorDetails");
            ErrorSeverity = SelectResponseDetailNodeText(xmlDoc, "ErrorSeverity");
        }
    }

    public abstract class GatewayResponse
    {
        public string Error { get; internal set; }

        internal static string SelectResponseDetailNodeText(XmlNode xml, string nodeName)
        {
            var path = string.Format("/ResponseDetails/{0}", nodeName);
            var node = xml.SelectSingleNode(path);

            if (node == null)
                throw new InvalidOperationException(string.Format("SelectResponseDetailNodeText failed to extract xml path: {0}", path));

            return node.InnerText;
        }
    }

    /// <summary>
    /// Summary description for GatewayResponse.
    /// Copyright Web Active Corporation Pty Ltd  - All rights reserved. 1998-2004
    /// This code is for exclusive use with the eWAY payment gateway
    /// </summary>
    public class PaymentResponse : GatewayResponse
    {
        public string TransactionNumber { get; private set; }

        public string InvoiceReference { get; private set; }

        public string Option1 { get; private set; }

        public string Option2 { get; private set; }

        public string Option3 { get; private set; }

        public string AuthorisationCode { get; private set; }

        public int Amount { get; private set; }

        public bool Status { get; private set; }

        public PaymentResponse(string xml)
        {
            var textReader = new XmlTextReader(new StringReader(xml))
                {
                    XmlResolver = null, 
                    WhitespaceHandling = WhitespaceHandling.None
                };

            // get the root node
            textReader.Read();

            if ((textReader.NodeType != XmlNodeType.Element) || (textReader.Name != "ewayResponse"))
            {
                return;
            }

            while (textReader.Read())
            {
                if ((textReader.NodeType != XmlNodeType.Element) || (textReader.IsEmptyElement))
                {
                    continue;
                }

                var currentField = textReader.Name;
                textReader.Read();
                if (textReader.NodeType != XmlNodeType.Text)
                {
                    continue;
                }

                var readerValue = textReader.Value;
                switch (currentField)
                {
                    case "ewayTrxnError":
                        Error = readerValue;
                        break;

                    case "ewayTrxnStatus":
                        if (readerValue.ToLower().IndexOf("true", StringComparison.Ordinal) != -1)
                            Status = true;
                        break;

                    case "ewayTrxnNumber":
                        TransactionNumber = readerValue;
                        break;

                    case "ewayTrxnOption1":
                        Option1 = readerValue;
                        break;

                    case "ewayTrxnOption2":
                        Option2 = readerValue;
                        break;

                    case "ewayTrxnOption3":
                        Option3 = readerValue;
                        break;

                    case "ewayReturnAmount":
                        int amount;
                        Int32.TryParse(readerValue, out amount);
                        Amount = amount;
                        break;

                    case "ewayAuthCode":
                        AuthorisationCode = readerValue;
                        break;

                    case "ewayTrxnReference":
                        InvoiceReference = readerValue;
                        break;

                    default:
                        // unknown field
                        throw new Exception("Unknown field in response.");
                }
            }
        }
    }
}