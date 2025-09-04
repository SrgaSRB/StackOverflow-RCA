@echo off
echo 🚀 Starting Complete StackOverflow System...
echo.

echo [1/7] Starting StackOverflow API Service...
start "StackOverflow API" cmd /k "cd StackOverflow\StackOverflowService & echo Starting StackOverflow API... & dotnet run"
timeout /t 3 /nobreak >nul

echo [2/7] Starting NotificationService Instance 1...
start "NotificationService-1" cmd /k "cd StackOverflow\NotificationService & echo Starting NotificationService Instance 1... & dotnet run --launch-profile NotificationService-Instance1"
timeout /t 2 /nobreak >nul

echo [3/7] Starting NotificationService Instance 2...
start "NotificationService-2" cmd /k "cd StackOverflow\NotificationService & echo Starting NotificationService Instance 2... & dotnet run --launch-profile NotificationService-Instance2"
timeout /t 2 /nobreak >nul

echo [4/7] Starting NotificationService Instance 3...
start "NotificationService-3" cmd /k "cd StackOverflow\NotificationService & echo Starting NotificationService Instance 3... & dotnet run --launch-profile NotificationService-Instance3"
timeout /t 2 /nobreak >nul

echo [5/7] Starting HealthMonitoringService Instance 1...
start "HealthMonitor-Instance1" cmd /k "cd StackOverflow\HealthMonitoringService & echo Starting HealthMonitoring Instance 1... & dotnet run --launch-profile HealthMonitoringService-Instance1"
timeout /t 2 /nobreak >nul

echo [6/7] Starting HealthMonitoringService Instance 2...
start "HealthMonitor-Instance2" cmd /k "cd StackOverflow\HealthMonitoringService & echo Starting HealthMonitoring Instance 2... & dotnet run --launch-profile HealthMonitoringService-Instance2"
timeout /t 2 /nobreak >nul

echo [7/7] Starting HealthStatusService Dashboard (HTML Version)...
start "HealthStatus Dashboard" cmd /k "echo Starting Health Status Dashboard... & start health-status-dashboard.html & echo Dashboard opened in browser!"
timeout /t 2 /nobreak >nul

echo [8/7] Starting React Frontend...
start "React App" cmd /k "cd my-app & echo Starting React App... & npm start"

echo.
echo ✅ All services are starting!
echo.
echo Services:
echo - StackOverflow API: http://localhost:5167
echo - NotificationService Instance 1: http://localhost:5168
echo - NotificationService Instance 2: http://localhost:5169  
echo - NotificationService Instance 3: http://localhost:5170
echo - HealthMonitoringService: 2 instances monitoring health every 4 seconds
echo - HealthStatusService Dashboard: HTML Version (opened in browser)
echo - React Frontend: http://localhost:3000
echo.
echo 📊 Health Status Dashboard opened in your default browser
echo (HTML version with simulated data - full .NET version available when disk space allows)
echo.
echo Press any key to exit this launcher...
pause >nul
