using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HealthStatusServiceWeb.DTO
{
    public class SerbiceStatusTableDTO
    {
        public DateTime CheckDateTime { get; set; }
        public string Status { get; set; } = ""; // "OK" or "NOT_OK"
        public string ServiceName { get; set; } = "";
        public string ErrorMessage { get; set; } = "";
        public int ResponseTimeMs { get; set; }
    }
}