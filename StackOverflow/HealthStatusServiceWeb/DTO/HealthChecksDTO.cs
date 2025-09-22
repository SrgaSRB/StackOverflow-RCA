using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HealthStatusServiceWeb.DTO
{
    public class HealthChecksDTO
    {
        public List<SerbiceStatusTableDTO> ServiceStatusTable { get; set; } = new List<SerbiceStatusTableDTO>();
        public List<ServiceStatusGraphDTO> ServiceStatusGraph { get; set; } = new List<ServiceStatusGraphDTO>();
    }
}