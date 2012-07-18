using System;
using System.Globalization;
using System.Xml;

namespace Payments.eway
{
    /// <summary>
    /// ReBill Payment (recurring payments)
    /// </summary>
    public class RebillPaymentMessage : EpayMessage
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

        public override string AsXml()
        {
            var xmlRebill = new XmlDocument();

            var nodeRoot = xmlRebill.CreateNode(XmlNodeType.Element, "RebillUpload", "");
            var nodeNewRebill = xmlRebill.CreateNode(XmlNodeType.Element, "NewRebill", "");

            var nodeCustomer = xmlRebill.CreateNode(XmlNodeType.Element, "eWayCustomerID", "");
            nodeCustomer.InnerText = eWAYCustomerID;
            nodeNewRebill.AppendChild(nodeCustomer);

            // Customer 
            nodeCustomer = xmlRebill.CreateNode(XmlNodeType.Element, "Customer", "");

            CreateAndAppendChild(xmlRebill, nodeCustomer, "CustomerRef", CustomerRef);
            CreateAndAppendChild(xmlRebill, nodeCustomer, "CustomerTitle", CustomerTitle);
            CreateAndAppendChild(xmlRebill, nodeCustomer, "CustomerFirstName", CustomerFirstName);

            CreateAndAppendChild(xmlRebill, nodeCustomer, "CustomerLastName", CustomerLastName);
            CreateAndAppendChild(xmlRebill, nodeCustomer, "CustomerCompany", CustomerCompany);
            CreateAndAppendChild(xmlRebill, nodeCustomer, "CustomerJobDesc", CustomerJobDesc);
            CreateAndAppendChild(xmlRebill, nodeCustomer, "CustomerEmail", CustomerEmail);
            CreateAndAppendChild(xmlRebill, nodeCustomer, "CustomerAddress", CustomerAddress);
            CreateAndAppendChild(xmlRebill, nodeCustomer, "CustomerSuburb", CustomerSuburb);
            CreateAndAppendChild(xmlRebill, nodeCustomer, "CustomerState", CustomerState);
            CreateAndAppendChild(xmlRebill, nodeCustomer, "CustomerPostCode", CustomerPostCode);
            CreateAndAppendChild(xmlRebill, nodeCustomer, "CustomerCountry", CustomerCountry);

            CreateAndAppendChild(xmlRebill, nodeCustomer, "CustomerPhone1", CustomerPhone1);
            CreateAndAppendChild(xmlRebill, nodeCustomer, "CustomerPhone2", CustomerPhone2);
            CreateAndAppendChild(xmlRebill, nodeCustomer, "CustomerFax", CustomerFax);
            CreateAndAppendChild(xmlRebill, nodeCustomer, "CustomerURL", CustomerUrl);
            CreateAndAppendChild(xmlRebill, nodeCustomer, "CustomerComments", CustomerComments);

            nodeNewRebill.AppendChild(nodeCustomer);

            // ReBill Events
            var nodeRebillEvent = xmlRebill.CreateNode(XmlNodeType.Element, "RebillEvent", "");

            CreateAndAppendChild(xmlRebill, nodeRebillEvent, "RebillInvRef", RebillInvRef);
            CreateAndAppendChild(xmlRebill, nodeRebillEvent, "RebillInvDesc", RebillInvDesc);
            CreateAndAppendChild(xmlRebill, nodeRebillEvent, "RebillCCName", RebillCCName);
            CreateAndAppendChild(xmlRebill, nodeRebillEvent, "RebillCCNumber", RebillCCNumber);
            CreateAndAppendChild(xmlRebill, nodeRebillEvent, "RebillCCExpMonth", RebillCCExpMonth);
            CreateAndAppendChild(xmlRebill, nodeRebillEvent, "RebillCCExpYear", RebillCCExpYear);
            CreateAndAppendChild(xmlRebill, nodeRebillEvent, "RebillInitAmt", RebillInitAmt);
            CreateAndAppendChild(xmlRebill, nodeRebillEvent, "RebillInitDate", RebillInitDate);
            CreateAndAppendChild(xmlRebill, nodeRebillEvent, "RebillRecurAmt", RebillRecurAmt);
            CreateAndAppendChild(xmlRebill, nodeRebillEvent, "RebillStartDate", RebillStartDate);
            CreateAndAppendChild(xmlRebill, nodeRebillEvent, "RebillInterval", RebillInterval);
            CreateAndAppendChild(xmlRebill, nodeRebillEvent, "RebillIntervalType", RebillIntervalType);
            CreateAndAppendChild(xmlRebill, nodeRebillEvent, "RebillEndDate", RebillEndDate);

            nodeNewRebill.AppendChild(nodeRebillEvent);

            nodeRoot.AppendChild(nodeNewRebill);

            xmlRebill.AppendChild(nodeRoot);

            return xmlRebill.InnerXml;
        }
    }

    /// <summary>
    /// An ePay refund request
    /// </summary>
    public class RefundRequestMessage : EpayMessage
    {
        private string _txCardExpiryMonth = "01";
        private string _txCardExpiryYear = "00";
        private string _txTransactionNumber = string.Empty;
        private string _txOption1 = string.Empty;
        private string _txOption2 = string.Empty;
        private string _txOption3 = string.Empty;
        private string _txRefundPassword = string.Empty;

        public RefundRequestMessage()
        {
            InvoiceAmount = 0;
            EwayCustomerID = string.Empty;
        }

        public string EwayCustomerID { get; set; }

        public int InvoiceAmount { get; set; }

        public string CardExpiryMonth
        {
            get
            {
                return _txCardExpiryMonth;
            }

            set
            {
                _txCardExpiryMonth = value;
            }
        }

        public string CardExpiryYear
        {
            get
            {
                return _txCardExpiryYear;
            }

            set
            {
                _txCardExpiryYear = value;
            }
        }

        public string TransactionNumber
        {
            get
            {
                return _txTransactionNumber;
            }

            set
            {
                _txTransactionNumber = value;
            }
        }
        public string EwayOption1
        {
            get { return _txOption1; }
            set { _txOption1 = value; }
        }
        public string EwayOption2
        {
            get { return _txOption2; }
            set { _txOption2 = value; }
        }
        public string EwayOption3
        {
            get { return _txOption3; }
            set { _txOption3 = value; }
        }
        public string RefundPassword
        {
            get { return _txRefundPassword; }
            set { _txRefundPassword = value; }
        }

        public override string AsXml()
        {
            // We don't really need the overhead of creating an XML DOM object
            // to really just concatenate a string together.

            var xmlOutput = "<ewaygateway>";
            xmlOutput += CreateNode("ewayCustomerID", EwayCustomerID);
            xmlOutput += CreateNode("ewayOriginalTrxnNumber", _txTransactionNumber);
            xmlOutput += CreateNode("ewayTotalAmount", InvoiceAmount.ToString(CultureInfo.InvariantCulture));
            xmlOutput += CreateNode("ewayCardExpiryMonth", _txCardExpiryMonth);
            xmlOutput += CreateNode("ewayCardExpiryYear", _txCardExpiryYear);
            xmlOutput += CreateNode("ewayOption1", _txOption1);
            xmlOutput += CreateNode("ewayOption2", _txOption2);
            xmlOutput += CreateNode("ewayOption3", _txOption3);
            xmlOutput += CreateNode("ewayRefundPassword", _txRefundPassword);
            xmlOutput += "</ewaygateway>";

            return xmlOutput;
        }
    }
}