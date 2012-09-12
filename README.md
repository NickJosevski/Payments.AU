Payments.AU
=============

Some basic .NET based payment gateway logic for some providers in **Australia**. 

This started as an eWAY project, but I quickly realised it did not have a major capability I required. The plan is to slowly refine these so they are more solid and usable by others, but for now a few specific SecurePay capabilities in this library are quite solid. 

I would self classify this to be a **v0.6 release for SecurePay**, and a **v0.2 release for eWAY**. There are integration tests that communicate via test gateway accounts to demonstrate functionality.

## Supported Gateways

### [SecurePay.com.au]( http://www.securepay.com.au/)

### [eWAY.com.au]( http://www.eway.com.au/ )
 

## Capabilities

The intention of these gateway wrappers is to facilitate:  

 - Creating a customer
 - Storing customer card details with the provider.
 - Supplying a **token** that represents a customer back to the provider at various times to 'bill' the customer again.
 - Fetching past payment data for a given customer via a **token**.

## This Library - Known Design Flaws

 - API endpoint configuration is not great yet.
 - Still using XDocument and XElement for requests (responses at least form class objects)

<hr />


## Limitations / Gotchas
Both providers are missing important capability that's either not available or not documented (so might as well not be available). But SecurePay had greater support for at least basic **token based subsequent billing**.  

 1. If you use SecurePay to do Scheduled/Recurring (time period based) then the charge amount cannot be changed via the API.
  - This was a major limitation
  - Overcomed by just using the token, and taking responsibility for scheduling future transaction charges.
 2. Various technical ones about what data is required when.
 3. Multiple API endpoints for various actions
  - If you want a mix of single charges and recurring payments currently require 2 instances of gateway;  
     * one for: *securepay.com.au/xmlapi/payment*
     * another for *securepay.com.au/xmlapi/periodic*
 4. Purely scheduled and automatically run transaction functionality is not complete.

<hr />

## SecurePay Usage

Via ISecurePayGateway create a new SecurePayGateway supplying it with:

  - ICommunicate endpoint 
    - Create a new SecurePayWebCommunication - this is so unit testing can take place without a real web connection
  - MerchantId 
    - Provided by SecurePay specific to your account
  - MerchantPassword 
    - Provided by SecurePay specific to your account
  - API Uri 
    - Several to choose from 
    - NOTE: This is a known design flaw in this library, if you want a mix of single charges and recurring payments currently require 2 instances of gateway.

#### ISecurePayGateway  

	public interface ISecurePayGateway
	{
	    SecurePayMessage SingleCharge(SecurePayCardInfo card, SecurePayPayment payment, string referenceId);
	
	    SecurePayMessage CreateCustomerWithCharge(string clientId, SecurePayCardInfo card, SecurePayPayment payment);
	
	    SecurePayMessage ChargeExistingCustomer(string clientId, SecurePayPayment payment);
	}

## Examples
#### Repeatable Customer Entry / Token based future billing

    string ApiPeriodic = "https://test.securepay.com.au/xmlapi/periodic";
    var comm = new SecurePayWebCommunication();
    
    ISecurePayGateway gateway = new SecurePayGateway(comm, "ABC0001", "abc123", ApiPeriodic)
    
    
    var card = new SecurePayCardInfo { Number = "4444333322221111", Expiry = "10/15" }
    
    var payment = new SecurePayPayment { Amount = 1151, Currency = "AUD" };
    
    // Must supply payment when creating customer, even though they will not be billed yet
    
    response = gateway.CreateCustomerWithCharge(clientId, card, payment)
    

    // Now you have an entry of the customer with SecurePay, to bill them anytime:
    
    var response = gateway.ChargeExistingCustomer(clientId, payment);


### SecurePay Documentation Notes

The Fingerprint is a protected record of the amount to be paid. It must be generated and then included as an input field to SecureFrame. It prevents a customer modifying the transaction details when submitting their card information.

Fingerprint security is used at two points within the end to end payment process.

 - You will need to generate the request Fingerprint and include the value in the payment request submitted to SecurePay (see below on how to generate the fingerprint)
 - SecurePay will return a result Fingerprint using the Callback or Return url’s.


The Fingerprint is a SHA1 hash of the above mandatory fields, plus the SecurePay Transaction Password in this order with a pipe separator “|”:

 - “merchant_id”
 - Transaction Password (supplied by SecurePay Support)
 - “txn_type”
 - “primary_ref”
 - “amount”
 - “fp_timestamp”

<hr />

## eWAY Usage

WARNING: After getting most of this linked to eWAY it was discovered there isn't a clear way to re-use the token to simply bill the customer again via the RapidAPI.

1. Uses RapidAPI from eWAY  
   - From eWay:
   - *Rapid API is a payment product that allows merchants to post credit card data from their customer’s browser 
directly to eWAY without it passing through the merchant’s server.*
1. b   
1. c

### Note:
 > *Refer to up to date eWAY documentation in case this information changes.*

### Steps

1. Give eWAY customer details: Name & Address
 - CreateAccessCode
2. Clients browser submits credit card details to eWAY
  - Form posts to Payment Service
3. Verify transaction from eWAY
  - GetAccessCodeResult

Create a Form, have it post directly to eWAY


### SOAP Endpoint

> https://au.ewaypayments.com/hotpotato/soap.asmx

### Payment Service

> https://au.ewaypayments.com/hotpotato/payment




### Detailed Flow 
#### From the eWAY documentation

**Step 1**: Request an Access Code  

 - The merchant will make a server-side SOAP call to the *CreateAccessCode* method of the Rapid API web service
 - If   
   - a. The request involves an existing Token customer, their details will be returned in the response including the masked credit card number.
   - b. Token Payments are not in use for this transaction, the returned customer data will be an echo of the data in the request.
 - Send an query, with
   - Auth Details: 
     - eWAYCustomerID
     - Username
     - Password
   - Save token flag
   - Customer details

**Step 2**: Customer submits card details direct to eWAY

Once the merchant receives the access code response, the customer should be redirected to a secure page that contains an HTML form. eWAY will only accept data from forms that use the POST method. Any data posted from a form that has the method attribute set to GET will be rejected.

Form Example:
	
	<form method="POST" action="https://au.ewaypayments.com/hotpotato/payment">
	  <input type="hidden" name="EWAY_ACCESSCODE" value="nvt0mwZXN9aU43rsIRPl..." />
	  <input type="text" name="EWAY_CARDNAME" />
	  <input type="text" name="EWAY_CARDNUMBER" />
	  <input type="text" name="EWAY_CARDMONTH" />
	  <input type="text" name="EWAY_CARDYEAR" />
	  <input type="text" name="EWAY_CARDCVN" />
	  <input type="submit" value="ProcessPayment" text="Process Payment" />
	</form>

Once the customer has entered their card data and submitted the form directly to eWAY, the transaction will be sent to the bank network for authorisation. eWAY will then store the outcome of the transaction, and redirect the customer’s browser to the URL specified in the Redirect URL field in Step 1.


**Step 3**: Request the transaction results

Once the customer has been redirected to the next page, the merchant will need to request the results from eWAY by calling the GetAccessCodeResult method of the Rapid API web service.

Supplying:
 - eWAYCustomerID
 - UserName
 - Password
 - AccessCode


