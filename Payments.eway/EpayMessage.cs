using System.Net;
using System.Xml;

namespace Payments.eway
{
    public abstract class EpayMessage
    {
        /// <summary>
        /// Compose the ePay message as XML
        /// </summary>
        /// <returns></returns>
        public abstract string AsXml();

        /// <summary>
        /// Builds a simple XML node.
        /// </summary>
        /// <param name="nodeName">The name of the node being created.</param>
        /// <param name="nodeValue">The value of the node being created.</param>
        /// <returns>An XML node as a string.</returns>
        internal static string CreateNode(string nodeName, string nodeValue)
        {
            // This is temporary, the sample code from ePay was simple concatenation, it now at least encodes
            return string.Format("<{0}>{1}</{0}>", WebUtility.HtmlEncode(nodeName), WebUtility.HtmlEncode(nodeValue));
        }

        /// <summary>
        /// Builds an XMLNode and appends it
        /// </summary>
        internal static void CreateAndAppendChild(XmlDocument doc, XmlNode node, string nodeName, string nodeValue)
        {
            var nodeToAdd = doc.CreateNode(XmlNodeType.Element, nodeName, "");
            nodeToAdd.InnerText = nodeValue;
            node.AppendChild(nodeToAdd);
        }
    }
}