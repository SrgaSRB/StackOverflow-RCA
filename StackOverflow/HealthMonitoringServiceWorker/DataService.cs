using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Models;
using System.Diagnostics;

namespace HealthMonitoringServiceWorker
{
    public class DataService
    {

        private readonly CloudStorageAccount _storageAccount;
        private readonly CloudTableClient _tableClient;
        private readonly CloudTable _healthCheckTable;
        private readonly CloudTable _alertEmailTable;

        public DataService()
        {
            var connectionString = CloudConfigurationManager.GetSetting("StorageConnectionString");
            _storageAccount = CloudStorageAccount.Parse(connectionString);
            _tableClient = _storageAccount.CreateCloudTableClient();

            _healthCheckTable = _tableClient.GetTableReference("HealthChecks");
            _alertEmailTable = _tableClient.GetTableReference("AlertEmails");

            _healthCheckTable.CreateIfNotExists();
            _alertEmailTable.CreateIfNotExists();
        }

        public async Task SaveHealthCheckAsync(HealthCheck healthCheck)
        {
            try
            {
                var operation = TableOperation.Insert(healthCheck);
                await _healthCheckTable.ExecuteAsync(operation);
                Trace.TraceInformation($"Health check for {healthCheck.ServiceName} saved successfully.");
            }
            catch
            {
                Trace.TraceError($"Error saving health check for {healthCheck.ServiceName}");
            }
        }

        public async Task<List<AlertEmail>> GetActiveEmailsAsync()
        {
            try
            {
                var query = new TableQuery<AlertEmail>().
                    Where(TableQuery.GenerateFilterConditionForBool("IsActive", QueryComparisons.Equal, true));

                var emails = new List<AlertEmail>();
                TableContinuationToken token = null;

                do
                {
                    var segment = await _alertEmailTable.ExecuteQuerySegmentedAsync(query, token);
                    emails.AddRange(segment.Results);
                    token = segment.ContinuationToken;
                } while (token != null);

                Trace.TraceInformation($"Retrieved {emails.Count} active alert emails.");

                return emails;
            }
            catch
            {
                Trace.TraceError("Error retrieving active alert emails.");
                return new List<AlertEmail>();
            }
        }

        public async Task AddAlertEmailAsync(string email)
        {
            try
            {
                AlertEmail alertEmail = new AlertEmail(email);
                var operation = TableOperation.Insert(alertEmail);

                await _alertEmailTable.ExecuteAsync(operation);
                Trace.TraceInformation($"Alert email {email} added successfully.");
            }
            catch
            {
                Trace.TraceError($"Error adding alert email {email}.");
            }
        }

    }
}
