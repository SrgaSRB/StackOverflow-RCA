using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HealthStatusServiceWeb.DTO
{
    public class ServiceStatusGraphDTO
    {
        public string ServiceName { get; set; } = "";
        public int TotalChecks { get; set; }
        public int SuccessfulChecks { get; set; }
        public int FailedChecks { get; set; }
        public DateTime LastCheckDateTime { get; set; }
    }
}