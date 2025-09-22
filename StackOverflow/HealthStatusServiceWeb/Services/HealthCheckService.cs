using Common.Models;
using HealthStatusServiceWeb.DTO;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI.WebControls;

namespace HealthStatusServiceWeb.Services
{
    public class HealthCheckService
    {

        private readonly CloudTable _commentsTable;


        public HealthCheckService(string connectionString)
        {
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var tableClient = storageAccount.CreateCloudTableClient();

            _commentsTable = tableClient.GetTableReference("HealthChecks");
            _commentsTable.CreateIfNotExists();

        }

        public async Task<HealthChecksDTO> GetServicesStatusLast3Hours()
        {
            try
            {
                DateTime threeHoursAgo = DateTime.UtcNow.AddHours(-3);

                string partitionFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "HEALTH_CHECK");
                string dateFilter = TableQuery.GenerateFilterConditionForDate("CheckDateTime", QueryComparisons.GreaterThanOrEqual, threeHoursAgo);
                string combinedFilter = TableQuery.CombineFilters(partitionFilter, TableOperators.And, dateFilter);

                TableQuery<HealthCheck> query = new TableQuery<HealthCheck>().Where(combinedFilter);

                Trace.TraceInformation("Pre ExecuteQuerySegmentedAsync");
                var segment = await _commentsTable.ExecuteQuerySegmentedAsync(query, null);
                Trace.TraceInformation("Posle ExecuteQuerySegmentedAsync");

                List<HealthCheck> healthChecks = segment.Results;

                var groupedByService = healthChecks.GroupBy(hc => hc.ServiceName);

                var serviceStatusGraph = groupedByService.Select(s => new ServiceStatusGraphDTO
                {
                    ServiceName = s.Key,
                    TotalChecks = s.Count(),
                    SuccessfulChecks = s.Count(hc => hc.Status == "OK"),
                    FailedChecks = s.Count(hc => hc.Status != "OK"),
                    LastCheckDateTime = s.Max(hc => hc.CheckDateTime)
                });

                var serviceStatusTable = healthChecks.Select(hc => new SerbiceStatusTableDTO
                {
                    ServiceName = hc.ServiceName,
                    CheckDateTime = hc.CheckDateTime,
                    Status = hc.Status,
                    ErrorMessage = hc.ErrorMessage,
                    ResponseTimeMs = hc.ResponseTimeMs
                }).OrderByDescending(hc => hc.CheckDateTime).ToList();

                Trace.TraceInformation("Successfully retrieved health check data.");

                return new HealthChecksDTO
                {
                    ServiceStatusGraph = serviceStatusGraph.ToList(),
                    ServiceStatusTable = serviceStatusTable
                };

            }catch (Exception ex)
            {
                Trace.TraceError("Error retrieving health check data: " + ex.Message);
                throw new Exception("Error retrieving health check data: " + ex.Message);
            }
           
        }


    }
}