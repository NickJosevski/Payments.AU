using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payments.eway
{
    public interface IEpayService
    {
        bool Subscribe(string customerId, RebillPaymentMessage recurringPaymentDetails);
    }

    public class EPayService : IEpayService
    {
        private GatewayConnector _gateway;

        public EPayService(string gatewayUri = "", int timeOut = 0)
        {
            _gateway = new GatewayConnector(gatewayUri, timeOut);
        }

        public bool Subscribe(string customerId, RebillPaymentMessage recurringPaymentDetails)
        {
            recurringPaymentDetails.CustomerRef = customerId;

            return _gateway.ProcessRequest(recurringPaymentDetails);
        }
    }
}
