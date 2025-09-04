using Azure;
using Azure.Data.Tables;
using HealthMonitoringService.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HealthMonitoringService.Services
{
    public class DistributedLockService
    {
        private readonly TableClient _lockTableClient;
        private readonly ILogger<DistributedLockService> _logger;
        private readonly string _instanceId;

        public DistributedLockService(IConfiguration configuration, ILogger<DistributedLockService> logger)
        {
            _logger = logger;
            _instanceId = Environment.MachineName + "_" + Environment.ProcessId + "_" + Guid.NewGuid().ToString("N")[..8];
            
            var connectionString = configuration.GetConnectionString("DefaultConnection") ?? 
                                  "UseDevelopmentStorage=true";

            _lockTableClient = new TableClient(connectionString, "DistributedLocks");
            _lockTableClient.CreateIfNotExists();
        }

        public async Task<bool> TryAcquireLockAsync(string lockName, TimeSpan lockDuration)
        {
            try
            {
                var lockEntity = new DistributedLock
                {
                    RowKey = lockName,
                    InstanceId = _instanceId,
                    AcquiredAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.Add(lockDuration),
                    IsActive = true
                };

                // Try to create lock (will fail if already exists)
                try
                {
                    await _lockTableClient.AddEntityAsync(lockEntity);
                    _logger.LogDebug("Acquired lock '{LockName}' for instance {InstanceId}", lockName, _instanceId);
                    return true;
                }
                catch (RequestFailedException ex) when (ex.Status == 409) // Conflict - lock already exists
                {
                    // Check if existing lock is expired
                    try
                    {
                        var existingLock = await _lockTableClient.GetEntityAsync<DistributedLock>("LOCK", lockName);
                        
                        if (existingLock.Value.ExpiresAt < DateTime.UtcNow)
                        {
                            // Lock expired, try to take it over
                            lockEntity.ETag = existingLock.Value.ETag;
                            await _lockTableClient.UpdateEntityAsync(lockEntity, ETag.All);
                            _logger.LogDebug("Acquired expired lock '{LockName}' for instance {InstanceId}", lockName, _instanceId);
                            return true;
                        }
                        else
                        {
                            _logger.LogDebug("Lock '{LockName}' is held by instance {HolderInstanceId}, expires at {ExpiresAt}", 
                                lockName, existingLock.Value.InstanceId, existingLock.Value.ExpiresAt);
                            return false;
                        }
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error acquiring lock '{LockName}'", lockName);
                return false;
            }
        }

        public async Task ReleaseLockAsync(string lockName)
        {
            try
            {
                var existingLock = await _lockTableClient.GetEntityAsync<DistributedLock>("LOCK", lockName);
                
                if (existingLock.Value.InstanceId == _instanceId)
                {
                    await _lockTableClient.DeleteEntityAsync("LOCK", lockName);
                    _logger.LogDebug("Released lock '{LockName}' for instance {InstanceId}", lockName, _instanceId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error releasing lock '{LockName}'", lockName);
            }
        }

        public string GetInstanceId() => _instanceId;
    }
}
