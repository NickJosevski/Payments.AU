using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Payments.Web.Controllers
{
    public class PaymentCompleteController : Controller
    {
        //
        // GET: /PaymentComplete/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult PaymentComplete(object response)
        {
            var x = response;

            return View();
        }
    }
}
