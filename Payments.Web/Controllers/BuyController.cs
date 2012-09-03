using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Payments.Web.Controllers
{
    public class BuyController : Controller
    {
        //
        // GET: /Buy/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult BasicForm()
        {
            return View();
        }
    }

    public class BasicForm
    {
        public string EWAY_ACCESSCODE { get; set; }

        public string EWAY_CARDNAME { get; set; }

        public string EWAY_CARDNUMBER { get; set; }

        public string EWAY_CARDMONTH { get; set; }

        public string EWAY_CARDYEAR { get; set; }

        public string EWAY_CARDCVN { get; set; }
    }
}
