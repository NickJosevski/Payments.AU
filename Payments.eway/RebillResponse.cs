using System;
using System.Xml;

namespace Payments.eway
{
    public class RebillResponse
    {
        public string Result { get; set; }

        public string ErrorSeverity { get; set; }

        public string ErrorDetails { get; set; }

        public RebillResponse(string xmlString)
        {
            var xmlDoc = new XmlDocument();

            xmlDoc.LoadXml(xmlString);

            if (!xmlDoc.HasChildNodes)
            {
                return;
            }

            Result = SelectResponseDetailNodeText(xmlDoc, "Result");
            ErrorDetails = SelectResponseDetailNodeText(xmlDoc, "ErrorDetails");
            ErrorSeverity = SelectResponseDetailNodeText(xmlDoc, "ErrorSeverity");
        }

        private static string SelectResponseDetailNodeText(XmlNode xml, string nodeName)
        {
            var path = string.Format("/ResponseDetails/{0}", nodeName);
            var node = xml.SelectSingleNode(path);

            if(node == null)
                throw new InvalidOperationException(string.Format("SelectResponseDetailNodeText failed to extract xml path: {0}", path));

            return node.InnerText;
        }
    }
}