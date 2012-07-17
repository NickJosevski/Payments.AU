using System;
using System.Xml;

namespace Payments.eway
{
    /// <summary>
    /// RebillPayment Object
    /// </summary>
    public class RebillPayment
    {
        public string eWAYCustomerID { get; set; }
        public string ewayURL { get; set; }

        public string CustomerRef { get; set; }
        public string CustomerTitle { get; set; }
        public string CustomerFirstName { get; set; }
        public string CustomerLastName { get; set; }
        public string CustomerCompany { get; set; }
        public string CustomerJobDesc { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerAddress { get; set; }
        public string CustomerSuburb { get; set; }
        public string CustomerState { get; set; }
        public string CustomerPostCode { get; set; }
        public string CustomerCountry { get; set; }
        public string CustomerPhone1 { get; set; }
        public string CustomerPhone2 { get; set; }
        public string CustomerFax { get; set; }
        public string CustomerUrl { get; set; }
        public string CustomerComments { get; set; }
        public string RebillInvRef { get; set; }
        public string RebillInvDesc { get; set; }
        public string RebillCCName { get; set; }
        public string RebillCCNumber { get; set; }
        public string RebillCCExpMonth { get; set; }
        public string RebillCCExpYear { get; set; }
        public string RebillInitAmt { get; set; }
        public string RebillInitDate { get; set; }
        public string RebillRecurAmt { get; set; }
        public string RebillStartDate { get; set; }
        public string RebillInterval { get; set; }
        public string RebillIntervalType { get; set; }
        public string RebillEndDate { get; set; }

        public string AsXml()
        {
            var xmlRebill = new XmlDocument();
            XmlNode nodeRoot = null;
            XmlNode nodeNewRebill = null;
            XmlNode nodeCustomer = null;
            XmlNode nodeCustomerDetails = null;
            XmlNode nodeRebillEvent = null;
            XmlNode nodeRebillDetails = null;

            nodeRoot = xmlRebill.CreateNode(XmlNodeType.Element, "RebillUpload", "");
            nodeNewRebill = xmlRebill.CreateNode(XmlNodeType.Element, "NewRebill", "");

            nodeCustomer = xmlRebill.CreateNode(XmlNodeType.Element, "eWayCustomerID", "");
            nodeCustomer.InnerText = eWAYCustomerID;
            nodeNewRebill.AppendChild(nodeCustomer);

            // Customer 
            nodeCustomer = xmlRebill.CreateNode(XmlNodeType.Element, "Customer", "");

            nodeCustomerDetails = xmlRebill.CreateNode(XmlNodeType.Element, "CustomerRef", "");
            nodeCustomerDetails.InnerText = CustomerRef;
            nodeCustomer.AppendChild(nodeCustomerDetails);

            nodeCustomerDetails = xmlRebill.CreateNode(XmlNodeType.Element, "CustomerTitle", "");
            nodeCustomerDetails.InnerText = CustomerTitle;
            nodeCustomer.AppendChild(nodeCustomerDetails);

            nodeCustomerDetails = xmlRebill.CreateNode(XmlNodeType.Element, "CustomerFirstName", "");
            nodeCustomerDetails.InnerText = CustomerFirstName;
            nodeCustomer.AppendChild(nodeCustomerDetails);

            nodeCustomerDetails = xmlRebill.CreateNode(XmlNodeType.Element, "CustomerLastName", "");
            nodeCustomerDetails.InnerText = CustomerLastName;
            nodeCustomer.AppendChild(nodeCustomerDetails);

            nodeCustomerDetails = xmlRebill.CreateNode(XmlNodeType.Element, "CustomerCompany", "");
            nodeCustomerDetails.InnerText = CustomerCompany;
            nodeCustomer.AppendChild(nodeCustomerDetails);

            nodeCustomerDetails = xmlRebill.CreateNode(XmlNodeType.Element, "CustomerJobDesc", "");
            nodeCustomerDetails.InnerText = CustomerJobDesc;
            nodeCustomer.AppendChild(nodeCustomerDetails);

            nodeCustomerDetails = xmlRebill.CreateNode(XmlNodeType.Element, "CustomerEmail", "");
            nodeCustomerDetails.InnerText = CustomerEmail;
            nodeCustomer.AppendChild(nodeCustomerDetails);

            nodeCustomerDetails = xmlRebill.CreateNode(XmlNodeType.Element, "CustomerAddress", "");
            nodeCustomerDetails.InnerText = CustomerAddress;
            nodeCustomer.AppendChild(nodeCustomerDetails);

            nodeCustomerDetails = xmlRebill.CreateNode(XmlNodeType.Element, "CustomerSuburb", "");
            nodeCustomerDetails.InnerText = CustomerSuburb;
            nodeCustomer.AppendChild(nodeCustomerDetails);

            nodeCustomerDetails = xmlRebill.CreateNode(XmlNodeType.Element, "CustomerState", "");
            nodeCustomerDetails.InnerText = CustomerState;
            nodeCustomer.AppendChild(nodeCustomerDetails);

            nodeCustomerDetails = xmlRebill.CreateNode(XmlNodeType.Element, "CustomerPostCode", "");
            nodeCustomerDetails.InnerText = CustomerPostCode;
            nodeCustomer.AppendChild(nodeCustomerDetails);

            nodeCustomerDetails = xmlRebill.CreateNode(XmlNodeType.Element, "CustomerCountry", "");
            nodeCustomerDetails.InnerText = CustomerCountry;
            nodeCustomer.AppendChild(nodeCustomerDetails);

            nodeCustomerDetails = xmlRebill.CreateNode(XmlNodeType.Element, "CustomerPhone1", "");
            nodeCustomerDetails.InnerText = CustomerPhone1;
            nodeCustomer.AppendChild(nodeCustomerDetails);

            nodeCustomerDetails = xmlRebill.CreateNode(XmlNodeType.Element, "CustomerPhone2", "");
            nodeCustomerDetails.InnerText = CustomerPhone2;
            nodeCustomer.AppendChild(nodeCustomerDetails);

            nodeCustomerDetails = xmlRebill.CreateNode(XmlNodeType.Element, "CustomerFax", "");
            nodeCustomerDetails.InnerText = CustomerFax;
            nodeCustomer.AppendChild(nodeCustomerDetails);

            nodeCustomerDetails = xmlRebill.CreateNode(XmlNodeType.Element, "CustomerURL", "");
            nodeCustomerDetails.InnerText = CustomerUrl;
            nodeCustomer.AppendChild(nodeCustomerDetails);

            nodeCustomerDetails = xmlRebill.CreateNode(XmlNodeType.Element, "CustomerComments", "");
            nodeCustomerDetails.InnerText = CustomerComments;
            nodeCustomer.AppendChild(nodeCustomerDetails);

            nodeNewRebill.AppendChild(nodeCustomer);

            // ReBill Events
            nodeRebillEvent = xmlRebill.CreateNode(XmlNodeType.Element, "RebillEvent", "");

            nodeRebillDetails = xmlRebill.CreateNode(XmlNodeType.Element, "RebillInvRef", "");
            nodeRebillDetails.InnerText = RebillInvRef;
            nodeRebillEvent.AppendChild(nodeRebillDetails);

            nodeRebillDetails = xmlRebill.CreateNode(XmlNodeType.Element, "RebillInvDesc", "");
            nodeRebillDetails.InnerText = RebillInvDesc;
            nodeRebillEvent.AppendChild(nodeRebillDetails);

            nodeRebillDetails = xmlRebill.CreateNode(XmlNodeType.Element, "RebillCCName", "");
            nodeRebillDetails.InnerText = RebillCCName;
            nodeRebillEvent.AppendChild(nodeRebillDetails);

            nodeRebillDetails = xmlRebill.CreateNode(XmlNodeType.Element, "RebillCCNumber", "");
            nodeRebillDetails.InnerText = RebillCCNumber;
            nodeRebillEvent.AppendChild(nodeRebillDetails);

            nodeRebillDetails = xmlRebill.CreateNode(XmlNodeType.Element, "RebillCCExpMonth", "");
            nodeRebillDetails.InnerText = RebillCCExpMonth;
            nodeRebillEvent.AppendChild(nodeRebillDetails);

            nodeRebillDetails = xmlRebill.CreateNode(XmlNodeType.Element, "RebillCCExpYear", "");
            nodeRebillDetails.InnerText = RebillCCExpYear;
            nodeRebillEvent.AppendChild(nodeRebillDetails);

            nodeRebillDetails = xmlRebill.CreateNode(XmlNodeType.Element, "RebillInitAmt", "");
            nodeRebillDetails.InnerText = RebillInitAmt;
            nodeRebillEvent.AppendChild(nodeRebillDetails);

            nodeRebillDetails = xmlRebill.CreateNode(XmlNodeType.Element, "RebillInitDate", "");
            nodeRebillDetails.InnerText = RebillInitDate;
            nodeRebillEvent.AppendChild(nodeRebillDetails);

            nodeRebillDetails = xmlRebill.CreateNode(XmlNodeType.Element, "RebillRecurAmt", "");
            nodeRebillDetails.InnerText = RebillRecurAmt;
            nodeRebillEvent.AppendChild(nodeRebillDetails);

            nodeRebillDetails = xmlRebill.CreateNode(XmlNodeType.Element, "RebillStartDate", "");
            nodeRebillDetails.InnerText = RebillStartDate;
            nodeRebillEvent.AppendChild(nodeRebillDetails);

            nodeRebillDetails = xmlRebill.CreateNode(XmlNodeType.Element, "RebillInterval", "");
            nodeRebillDetails.InnerText = RebillInterval;
            nodeRebillEvent.AppendChild(nodeRebillDetails);

            nodeRebillDetails = xmlRebill.CreateNode(XmlNodeType.Element, "RebillIntervalType", "");
            nodeRebillDetails.InnerText = RebillIntervalType;
            nodeRebillEvent.AppendChild(nodeRebillDetails);

            nodeRebillDetails = xmlRebill.CreateNode(XmlNodeType.Element, "RebillEndDate", "");
            nodeRebillDetails.InnerText = RebillEndDate;
            nodeRebillEvent.AppendChild(nodeRebillDetails);

            nodeNewRebill.AppendChild(nodeRebillEvent);

            nodeRoot.AppendChild(nodeNewRebill);

            xmlRebill.AppendChild(nodeRoot);

            return xmlRebill.InnerXml;
        }
    }
}