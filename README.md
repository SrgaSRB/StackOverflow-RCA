# StackOverflow RCA (Root Cause Analysis) System

A comprehensive microservices-based system for monitoring, analyzing, and managing StackOverflow-like application health and notifications.

## 🏗️ Architecture Overview

This system consists of multiple interconnected services:

### Core Services
- **StackOverflow API** (`StackOverflowService`) - Main application API
- **Notification Service** - Handles email notifications (3 instances for scalability)
- **Health Monitoring Service** - Monitors system health (2 instances with distributed locking)
- **Health Status Service** - Web dashboard for health visualization
- **React Frontend** - User interface

### Supporting Infrastructure
- **Azure Storage Emulator (Azurite)** - Local storage for tables, blobs, and queues
- **Distributed Locking** - Prevents duplicate health checks across instances

## 🚀 Quick Start

### Prerequisites
- .NET 8.0 SDK
- Node.js (for React frontend and Azurite)
- PowerShell or Command Prompt

### Starting All Services

1. **Start Azure Storage Emulator**:
   ```cmd
   azurite
   ```

2. **Start All Services**:
   ```cmd
   .\start-all-services.bat
   ```

This will start:
- StackOverflow API: http://localhost:5167
- NotificationService Instance 1: http://localhost:5168
- NotificationService Instance 2: http://localhost:5169  
- NotificationService Instance 3: http://localhost:5170
- HealthMonitoringService: 2 instances monitoring every 4 seconds
- HealthStatusService Dashboard: http://localhost:5123
- React Frontend: http://localhost:3000

## 📊 Health Monitoring Dashboard

Access the real-time health dashboard at: **http://localhost:5123**

Features:
- Real-time service availability metrics
- 3-hour historical data visualization
- Hourly status summaries
- Recent health check logs
- Automatic refresh every 30 seconds

## 📁 Project Structure

```
├── docs/                          # Documentation files
├── StackOverflow/                 # .NET Core services
│   ├── StackOverflowService/      # Main API service
│   ├── NotificationService/       # Email notification service
│   ├── HealthMonitoringService/   # Health check worker service
│   └── HealthStatusService/       # Web dashboard for health status
├── my-app/                        # React frontend application
├── start-all-services.bat         # Service startup script
└── health-status-dashboard.html   # Static HTML dashboard (backup)
```

## 🔧 Configuration

### Health Monitoring
- **Check Interval**: Every 4 seconds
- **Timeout**: 10 seconds per health check
- **Data Retention**: 3 hours visible in dashboard
- **Alert Emails**: Configurable via Azure Table Storage

### Storage
All services use Azure Storage Emulator (Azurite) with connection string:
```
UseDevelopmentStorage=true
```

### Service URLs
Services are monitored via `/health-monitoring` endpoints:
- StackOverflow Service: http://localhost:5169/health-monitoring
- Notification Service: http://localhost:5168/health-monitoring

## 📚 Documentation

Detailed documentation is available in the `docs/` folder:

- [Health Monitoring System](docs/HEALTH_MONITORING_README.md)
- [Health Status Service](docs/HEALTH_STATUS_SERVICE_README.md)
- [Notification System](docs/NOTIFICATION_SYSTEM_README.md)
- [Distributed Locking Solution](docs/DISTRIBUTED_LOCK_SOLUTION.md)
- [Testing Health Monitoring](docs/TESTING_HEALTH_MONITORING.md)
- [Testing Notification System](docs/TESTING_NOTIFICATION_SYSTEM.md)
- [Voting API Endpoints](docs/VOTING_API_ENDPOINTS.md)

## 🧪 Testing

Use the provided test files:
- `test-api.html` - API endpoint testing
- `test-notification-health.ps1` - PowerShell health check script

## 🛠️ Development

### Adding New Services to Monitor
1. Update `appsettings.json` in HealthMonitoringService
2. Add service URL to the Services section
3. Ensure the new service has a `/health-monitoring` endpoint

### Customizing Alert Emails
Add email addresses to the `AlertEmails` table in Azure Storage.

## 📝 Notes

- **Distributed Locking**: Only one HealthMonitoringService instance performs checks at a time
- **Real-time Data**: Dashboard shows actual data from Azure Storage, not simulated data
- **Scalability**: Multiple service instances can run simultaneously
- **Reliability**: Health checks include timeout handling and error recovery

## 🏥 Health Check Flow

1. HealthMonitoringService instances compete for distributed lock
2. Winner performs health checks on all configured services
3. Results are stored in Azure Table Storage
4. HealthStatusService reads data and displays in web dashboard
5. Failed checks trigger email alerts to configured recipients

---

*Built with .NET 8, React, Azure Storage, and lots of ☕*
