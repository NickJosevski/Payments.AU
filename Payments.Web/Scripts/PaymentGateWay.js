
window.PaymentGateWay = window.PaymentGateWay || {};

window.PaymentGateWay.ewayResponseWait = function(iframe, onSuccess, onTimeout) {

    var checks = 1;
    
    function checkForEwayRedirectBasedResult() {
        var response = iframe.contents().find("#eway-response");
        
        if (response.get(0)) {
            
            onSuccess(response);
            return;
        }
        if (checks > 150) {
            // waited 1.5 minutes (100 * 600ms) nothing's happened, let user know
            onTimeout();
            return;
        }
        checks++;
        setTimeout(checkForEwayRedirectBasedResult, 600);
    }

    checkForEwayRedirectBasedResult();
};

window.PaymentGateWay.ewayPaymentSubmit = function (submit, errorDisplay, iframe, onSuccess, onTimeout) {

    // disable the submit button to prevent repeated clicks
    $(submit).attr("disabled", "disabled");

    var errors = "";

    var valid = true;
    if (!window.PaymentGateWay.validateCardNumber($('.card-number').val())) {

        errors = "Looks like you have mistyped your credit card number <br />";
        valid = false;
    }

    if (!window.PaymentGateWay.validateExpiry($('.card-expiry-month').val(), $('.card-expiry-year').val())) {

        errors = errors + "Card expiry date is not correct <br />";
        valid = false;
    }

    if (!window.PaymentGateWay.validateCVC($('.card-cvc').val())) {

        errors = errors + "Card CVC is not in the correct format needs to be 3 or 4 digits <br />";
        valid = false;
    }

    if (valid) {
        window.PaymentGateWay.processEWayPayment(iframe, errorDisplay, {
            ewayAccessCode: $('.eway-access-code').val(),
            cardName: $('.card-name').val(),
            cardNumber: $('.card-number').val(),
            cardCvc: $('.card-cvc').val(),
            cardMonth: $('.card-expiry-month').val(),
            cardYear: $('.card-expiry-year').val()
        });
        
        window.PaymentGateWay.ewayResponseWait(iframe, onSuccess, onTimeout);
    }
    else {
        errorDisplay.html(errors);
        $(submit).removeAttr("disabled");
    }
};

window.PaymentGateWay.processEWayPayment = function (iframe, errorDisplay, paymentDetails) {
    // NOTE: we're not breaching the http://en.wikipedia.org/wiki/Same_origin_policy with this iFrame

    var nestedForm = iframe.contents().find('form');

    // Copy all the card details to the form to send
    nestedForm.find('.eway-access-code').val(paymentDetails.ewayAccessCode);
    nestedForm.find('.card-name').val(paymentDetails.cardName);
    nestedForm.find('.card-number').val(paymentDetails.cardNumber);
    nestedForm.find('.card-cvc').val(paymentDetails.cardCvc);
    nestedForm.find('.card-expiry-month').val(paymentDetails.cardMonth);
    nestedForm.find('.card-expiry-year').val(paymentDetails.cardYear);

    // paranoid check, we've copied fields over to nested form, lets rebuild the details, and validate them as a group
    var finalDetails =
        {
            ewayAccessCode: nestedForm.find('.eway-access-code').val(),
            cardName: nestedForm.find('.card-name').val(),
            cardNumber: nestedForm.find('.card-number').val(),
            cardCvc: nestedForm.find('.card-cvc').val(),
            cardMonth: nestedForm.find('.card-expiry-month').val(),
            cardYear: nestedForm.find('.card-expiry-year').val()
        };

    if (window.PaymentGateWay.isValidForEway(finalDetails)) {
        nestedForm.submit();
    } else {
        errorDisplay.html("Second round of validating Credit Card details failed. The problem should have already been reported. If you see this message please contact us.");
    }
};

window.PaymentGateWay.isValidForEway = function (details) {

    // length of access code should be 157 characters, if it's longer that will be ok, if it grows in the future
    return details.ewayAccessCode.length >= 157 && window.PaymentGateWay.isValid(details);
};

window.PaymentGateWay.isValid = function (details) {

    return window.PaymentGateWay.validateCardNumber(details.cardNumber)
        && window.PaymentGateWay.validateCVC(details.cardCvc)
        && window.PaymentGateWay.validateExpiry(details.cardMonth, details.cardYear);
};

// card, luhn, cvc and expiry logic thanks to Stripe.com - https://js.stripe.com/v1/stripe-debug.js

window.PaymentGateWay.validateCardNumber = function(num) {
    num = (num + '').replace(/\s+|-/g, '');
    return num.length >= 10 && num.length <= 16 && window.PaymentGateWay.luhnCheck(num);
};

window.PaymentGateWay.cardType = function(num) {
    return window.PaymentGateWay.cardTypes[num.slice(0, 2)] || 'Unknown';
};

window.PaymentGateWay.luhnCheck = function (num) {
    var digit, digits, odd, sum, _i, _len;
    odd = true;
    sum = 0;
    digits = (num + '').split('').reverse();
    for (_i = 0, _len = digits.length; _i < _len; _i++) {
        digit = digits[_i];
        digit = parseInt(digit, 10);
        if ((odd = !odd)) {
            digit *= 2;
        }
        if (digit > 9) {
            digit -= 9;
        }
        sum += digit;
    }
    return sum % 10 === 0;
};

window.PaymentGateWay.cardTypes = function () {
    var num, types, _i, _j;
    types = {};
    for (num = _i = 40; _i <= 49; num = ++_i) {
        types[num] = 'Visa';
    }
    for (num = _j = 50; _j <= 59; num = ++_j) {
        types[num] = 'MasterCard';
    }
    types[34] = types[37] = 'American Express';
    types[60] = types[62] = types[64] = types[65] = 'Discover';
    types[35] = 'JCB';
    types[30] = types[36] = types[38] = types[39] = 'Diners Club';
    return types;
};

window.PaymentGateWay.validateExpiry = function (month, year) {
    var currentTime, expiry;
    month = window.PaymentGateWay.trim(month);
    year = window.PaymentGateWay.trim(year);
    if (!/^\d+$/.test(month)) {
        return false;
    }
    if (!/^\d+$/.test(year)) {
        return false;
    }
    if (!(parseInt(month, 10) <= 12)) {
        return false;
    }
    if (year < 100) {
        // 2 digit year - note a y2.1k bug ;)
        year = 2000 + parseInt(year, 10);
    }
    expiry = new Date(year, month);
    currentTime = new Date;
    expiry.setMonth(expiry.getMonth() - 1);
    expiry.setMonth(expiry.getMonth() + 1, 1);
    return expiry > currentTime;
};

window.PaymentGateWay.validateCVC = function(num) {
    num = window.PaymentGateWay.trim(num);
    return /^\d+$/.test(num) && num.length >= 3 && num.length <= 4;
};

window.PaymentGateWay.trim = function (str) {
    return (str + '').replace(/^\s+|\s+$/g, '');
};