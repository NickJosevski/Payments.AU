using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Payments.eway;

namespace Payments.Web.Controllers
{
    public class BuyController : Controller
    {
        private readonly EwayPaymentGateway _eway;

        private EwayCustomerDetails _customer;

        private EwayPayment _payment;

        public BuyController()
        {
            // TODO: make this constructor injected
            _eway = new EwayPaymentGateway();

            // TODO: this won't exist here, this is up to you to manage in your system.
            _customer = new EwayCustomerDetails
            {
                Title = "Mr.",
                FirstName = "Just",
                LastName = "SetupTheCustomer",
                Country = "au",
            };

            _payment = new EwayPayment
            {
                InvoiceDescription = "Customer Created",
                InvoiceNumber = Guid.NewGuid().ToString(),
                InvoiceReference = Guid.NewGuid().ToString(),
                TotalAmount = DateTime.Now.Minute * 100
            };
        }

        //
        // GET: /Buy/
        public ActionResult Index()
        {
            var result = _eway.CreateCustomerWithPaymentRequirement("http://localhost:51868/PaymentComplete/Good", true, _customer, _payment);

            if(String.IsNullOrWhiteSpace(result.AccessCode)) return View("Error");

            return View(new PaymentSetup
            {
                AccessCode = result.AccessCode
            });
        }

        // GET: /BasicForm/
        public ActionResult BasicForm()
        {
            var result = _eway.CreateCustomerWithPaymentRequirement("http://localhost:51868/PaymentComplete/Good", true, _customer, _payment);

            if (String.IsNullOrWhiteSpace(result.AccessCode)) return View("Error");

            return View(new BasicForm
                {
                    EWAY_ACCESSCODE = result.AccessCode,
                    EWAY_CARDCVN = "123",
                    EWAY_CARDNAME = "Test User",
                    EWAY_CARDNUMBER = "4444333322221111",
                    EWAY_CARDMONTH = "12",
                    EWAY_CARDYEAR = "12",
                });
        }
    }

    /// <summary>
    /// Only to seed test data, this data will never come from the application.
    /// </summary>
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
