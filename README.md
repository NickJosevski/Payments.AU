Payments.AU
=============

Some basic .NET based payment gateway logic for some providers in Australia.

## Gateways

### SecurePay 

[SecurePay.com.au]( http://www.securepay.com.au/)

### eWAY 

[eWAY.com.au]( http://www.eway.com.au/ )
 



## Capabilities

~~ section in progress


## SecurePay Usage

~~ section in progress


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


