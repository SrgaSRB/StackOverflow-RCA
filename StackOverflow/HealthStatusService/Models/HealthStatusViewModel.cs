namespace HealthStatusService.Models
{
    public class HealthStatusViewModel
    {
        public List<HealthCheck> HealthChecks { get; set; } = new List<HealthCheck>();
        public double AvailabilityPercentage { get; set; }
        public double UnavailabilityPercentage { get; set; }
        public int TotalChecks { get; set; }
        public int SuccessfulChecks { get; set; }
        public int FailedChecks { get; set; }
        public DateTime FromTime { get; set; }
        public DateTime ToTime { get; set; }
        public List<HourlyStatusData> HourlyData { get; set; } = new List<HourlyStatusData>();
    }

    public class HourlyStatusData
    {
        public DateTime Hour { get; set; }
        public int TotalChecks { get; set; }
        public int SuccessfulChecks { get; set; }
        public int FailedChecks { get; set; }
        public double AvailabilityPercentage { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
