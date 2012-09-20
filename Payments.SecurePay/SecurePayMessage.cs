using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Payments.SecurePay
{
    [Serializable]
    public class SecurePayMessage
    {
        [XmlElement]
        public SecurePayMessageInfo MessageInfo { get; set; }

        [XmlElement]
        public string RequestType { get; set; }

        [XmlElement]
        public SecurePayMerchantInfo MerchantInfo { get; set; }

        [XmlElement]
        public SecurePayStatus Status { get; set; }

        [XmlElement]
        public SecurePayPeriodic Periodic { get; set; }

        public bool WasSuccessful()
        {
            return Status.StatusDescription.Equals("Normal") && Status.StatusCode == 0;
        }

        public string ReceiptNumbers()
        {
            var receiptCodes = Periodic.PeriodicList.PeriodicItem.Where(p => !String.IsNullOrWhiteSpace(p.Receipt)).Select(p => p.Receipt);

            return string.Join(", ", receiptCodes);
        }
    }

    [Serializable]
    public class SecurePayMessageInfo
    {
        [XmlElement("messageID")]
        public string MessageId { get; set; }

        [XmlElement("messageTimestamp")]
        public string MessageTimestamp { get; set; }

        [XmlElement("timeoutValue")]
        public string TimeoutValue { get; set; }

        [XmlElement("apiVersion")]
        public string ApiVersion { get; set; }
    }

    [Serializable]
    public class SecurePayMerchantInfo
    {
        [XmlElement("merchantID")]
        public string MerchantId { get; set; }
    }

    [Serializable]
    public class SecurePayStatus
    {
        [XmlElement("statusCode")]
        public int StatusCode { get; set; }

        [XmlElement("statusDescription")]
        public string StatusDescription { get; set; }
    }

    [Serializable]
    public class SecurePayPeriodic
    {
        [XmlElement]
        public SecurePayPeriodicList PeriodicList { get; set; }
    }

    [Serializable]
    public class SecurePayPeriodicList
    {
        public SecurePayPeriodicList()
        { PeriodicItem = new List<SecurePayPeriodicItem>(); }

        [XmlElement]
        public List<SecurePayPeriodicItem> PeriodicItem { get; set; }

        [XmlAttribute("count")]
        public int Count { get; set; }
    }

    [Serializable]
    [XmlRoot("PeriodicItem")]
    public class SecurePayPeriodicItem
    {
        [XmlAttribute("ID")]
        public int Id { get; set; }

        [XmlElement("actionType")]
        public string ActionType { get; set; }

        [XmlElement("clientID")]
        public string ClientId { get; set; }

        [XmlElement("responseCode")]
        public int ResponseCode { get; set; }

        [XmlElement("responseText")]
        public string ResponseText { get; set; }

        [XmlElement("successful")]
        public string Successful { get; set; }

        [XmlElement("txnType")]
        public int TxnType { get; set; }

        [XmlElement("amount")]
        public int Amount { get; set; }

        [XmlElement("currency")]
        public string Currency { get; set; }

        [XmlElement("txnID")]
        public string TxnId { get; set; }

        [XmlElement("receipt")]
        public string Receipt { get; set; }

        [XmlElement("ponum")]
        public string Ponum { get; set; }

        [XmlElement("periodicType")]
        public string PeriodicType { get; set; }

        [XmlElement("paymentInterval")]
        public string PaymentInterval { get; set; }

        [XmlElement("numberOfPayments")]
        public string NumberOfPayments { get; set; }

        [XmlElement("settlementDate")]
        public string SettlementDate { get; set; }

        [XmlElement]
        public SecurePayCreditCardInfo CreditCardInfo { get; set; }
    }

    [Serializable]
    public class SecurePayCreditCardInfo
    {
        [XmlElement("pan")]
        public string Pan { get; set; }

        [XmlElement("expiryDate")]
        public string expiryDate { get; set; }

        [XmlElement("recurringFlag")]
        public string RecurringFlag { get; set; }

        [XmlElement("cardType")]
        public string CardType { get; set; }

        [XmlElement("cardDescription")]
        public string CardDescription { get; set; }
    }

    [Serializable]
    public class SecurePayException : Exception
    {
        public int StatusCode { get; set; }

        public string StatusDescription { get; set; }

        public SecurePayException(string msg)
            :base(msg)
        {
            StatusCode = -1;
            StatusDescription = msg;
        }

        public SecurePayException(string msg, int statusCode, string statusDescription)
            : base(msg)
        {
            StatusCode = statusCode;
            StatusDescription = statusDescription;
        }

        public SecurePayException(string msg, Exception inner)
            : base(msg, inner)
        {
        }
    }

    public enum ActionType
    {
        // Must be lowercase
        add,
        trigger,
        delete
    }

    public enum SecurePayStatusCodes
    {
        Normal = 0,
        Expired = 54,
        InvalidMerchantId = 504,
        InvalidUrl = 505,
        UnableToConnectToServer = 510,
        ServerConnectionAbortedDuringTransaction = 511,
        TransactionTimedOutByClient = 512,
        GeneralDatabaseError = 513,
        ErrorLoadingPropertiesFile = 514,
        FatalUnknownError = 515,
        RequestTypeUnavailable = 516,
        MessageFormatError = 517,
        ResponseNotReceived = 524,
        SystemMaintenanceInProgress = 545,
        InvalidPassword = 550,
        NotImplemented = 5575,
        TooManyRecordsForProcessing = 577,
        ProcessMethodHasNotBeenCalled = 580,
        MerchantDisabled = 595
    }

    public enum SecurePayCardType
    {
        Unknown = 0,
        JCB,
        AmericanExpress,
        DinersClub,
        Bankcard,
        MasterCard,
        Visa
    }
}