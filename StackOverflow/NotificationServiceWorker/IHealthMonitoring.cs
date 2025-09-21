using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace NotificationServiceWorker
{
    [ServiceContract]
    public interface IHealthMonitoring
    {
        [OperationContract]
        [WebGet(UriTemplate = "/health-monitoring")]
        string HealthCheck();
    }
}
