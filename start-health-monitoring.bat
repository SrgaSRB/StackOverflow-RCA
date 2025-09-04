@echo off
echo Starting HealthMonitoringService with 2 instances...

cd /d "StackOverflow\HealthMonitoringService"

echo Starting Instance 1...
start "HealthMonitor-Instance1" dotnet run --environment Development

echo Starting Instance 2...
start "HealthMonitor-Instance2" dotnet run --environment Development

echo Both HealthMonitoringService instances started.
echo Instance 1 and Instance 2 are running in separate windows.

pause
