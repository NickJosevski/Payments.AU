
window.PaymentGateWay = window.PaymentGateWay || {};

window.PaymentGateWay.isValidForEway = function (details) {

    // length of access code should be 157 characters, if it's longer that will be ok, if it grows in the future
    return details.ewayAccessCode.length >= 157 && window.PaymentGateWay.isValid(details);
};

window.PaymentGateWay.isValid = function (details) {

    return window.PaymentGateWay.validateCardNumber(details.cardNumber)
        && window.PaymentGateWay.validateCVC(details.cardCvc)
        && window.PaymentGateWay.validateExpiry(details.cardMonth, details.cardYear);
};

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
        // 2 digit year
        year = 2000 + parseInt(year, 10);
    }
    expiry = new Date(year, month);
    currentTime = new Date;
    expiry.setMonth(expiry.getMonth() - 1);
    expiry.setMonth(expiry.getMonth() + 1, 1);
    console.log('expiry = ' + expiry);
    console.log('currentTime = ' + currentTime);
    return expiry > currentTime;
};

window.PaymentGateWay.validateCVC = function(num) {
    num = window.PaymentGateWay.trim(num);
    return /^\d+$/.test(num) && num.length >= 3 && num.length <= 4;
};

window.PaymentGateWay.trim = function (str) {
    return (str + '').replace(/^\s+|\s+$/g, '');
};