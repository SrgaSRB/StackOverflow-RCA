using Azure.Data.Tables;
using AdminToolsConsoleApp.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AdminToolsConsoleApp.Services
{
    public class AlertEmailService
    {
        private readonly TableClient _alertEmailTableClient;
        private readonly ILogger<AlertEmailService> _logger;

        public AlertEmailService(IConfiguration configuration, ILogger<AlertEmailService> logger)
        {
            _logger = logger;
            var connectionString = configuration.GetConnectionString("AzureStorage") ?? 
                                 "UseDevelopmentStorage=true";

            _alertEmailTableClient = new TableClient(connectionString, "AlertEmails");
            
            try
            {
                _alertEmailTableClient.CreateIfNotExists();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to Azure Storage. Make sure Azurite is running.");
                throw new InvalidOperationException(
                    "Cannot connect to Azure Storage. Please ensure Azurite is running.\n" +
                    "Start Azurite with: azurite --silent --location c:\\azurite --debug c:\\azurite\\debug.log", ex);
            }
        }

        public async Task<List<AlertEmail>> GetAllAlertEmailsAsync()
        {
            var emails = new List<AlertEmail>();
            try
            {
                await foreach (var email in _alertEmailTableClient.QueryAsync<AlertEmail>())
                {
                    emails.Add(email);
                }
                return emails.OrderBy(e => e.Email).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving alert emails");
                throw;
            }
        }

        public async Task<bool> AddAlertEmailAsync(string email, string name)
        {
            try
            {
                // Validate email format
                if (!IsValidEmail(email))
                {
                    _logger.LogWarning("Invalid email format: {Email}", email);
                    return false;
                }

                // Check if email already exists
                var existingEmail = await GetAlertEmailAsync(email);
                if (existingEmail != null)
                {
                    _logger.LogWarning("Email already exists: {Email}", email);
                    return false;
                }

                var alertEmail = new AlertEmail
                {
                    RowKey = email,
                    Email = email,
                    Name = name,
                    IsActive = true
                };

                await _alertEmailTableClient.AddEntityAsync(alertEmail);
                _logger.LogInformation("Added alert email: {Email}", email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding alert email: {Email}", email);
                return false;
            }
        }

        public async Task<bool> DeleteAlertEmailAsync(string email)
        {
            try
            {
                var existingEmail = await GetAlertEmailAsync(email);
                if (existingEmail == null)
                {
                    _logger.LogWarning("Email not found: {Email}", email);
                    return false;
                }

                await _alertEmailTableClient.DeleteEntityAsync("ALERT_EMAIL", email);
                _logger.LogInformation("Deleted alert email: {Email}", email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting alert email: {Email}", email);
                return false;
            }
        }

        public async Task<bool> ToggleAlertEmailStatusAsync(string email)
        {
            try
            {
                var existingEmail = await GetAlertEmailAsync(email);
                if (existingEmail == null)
                {
                    _logger.LogWarning("Email not found: {Email}", email);
                    return false;
                }

                existingEmail.IsActive = !existingEmail.IsActive;
                await _alertEmailTableClient.UpdateEntityAsync(existingEmail, existingEmail.ETag);
                
                _logger.LogInformation("Updated alert email status: {Email} -> {Status}", 
                    email, existingEmail.IsActive ? "Active" : "Inactive");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating alert email status: {Email}", email);
                return false;
            }
        }

        private async Task<AlertEmail?> GetAlertEmailAsync(string email)
        {
            try
            {
                var response = await _alertEmailTableClient.GetEntityAsync<AlertEmail>("ALERT_EMAIL", email);
                return response.Value;
            }
            catch (Azure.RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
