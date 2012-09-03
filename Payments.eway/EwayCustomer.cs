using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payments.eway
{
    public class EwayCustomer
    {
        public long? ThisokenCustomerIDField { get; set; }

        public bool TokenCustomerIDFieldSpecified { get; set; }

        public bool SaveTokenField { get; set; }

        public string ReferenceField { get; set; }

        public string titleField { get; set; }

        public string firstNameField { get; set; }

        public string lastNameField { get; set; }

        public string companyNameField { get; set; }

        public string jobDescriptionField { get; set; }

        public string street1Field { get; set; }

        public string cityField { get; set; }

        public string stateField { get; set; }

        public string postalCodeField { get; set; }

        public string countryField { get; set; }

        public string emailField { get; set; }

        public string phoneField { get; set; }

        public string mobileField { get; set; }

        public string commentsField { get; set; }

        public string faxField { get; set; }

        public string urlField { get; set; }

        public string cardNumberField { get; set; }

        public string cardNameField { get; set; }

        public string cardExpiryMonthField { get; set; }

        public string cardExpiryYearField { get; set; }
    }
}
