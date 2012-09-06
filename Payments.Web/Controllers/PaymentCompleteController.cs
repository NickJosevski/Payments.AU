using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Payments.eway;

namespace Payments.Web.Controllers
{
    public class PaymentCompleteController : Controller
    {
        private readonly EwayPaymentGateway _eway;

        public PaymentCompleteController()
        {
            // TODO: make this constructor injected
            _eway = new EwayPaymentGateway();
        }

        public ActionResult Good(string accessCode)
        {
            var result = _eway.GetAccessCodeResult(accessCode);
            var msg = "All Good";

            if(result.ResponseCode != "00")
                msg = "Not Good";

            return View(new PaymentResposnse { Message = msg, Code = result.ResponseMessage, Token = result.TokenCustomerID });
        }

        public void SecurePayGood(object response)
        {
            var t = response;

            throw new NotImplementedException("SecurePayGood got a response");
        }
    }

    public class PaymentResposnse
    {
        public string Code { get; set; }

        public string Message { get; set; }

        public string Token { get; set; }
    }

    public class PaymentSetup
    {
        public string AccessCode { get; set; }
    }
}
