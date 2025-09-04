@echo off
echo 🚀 Starting Complete StackOverflow System...
echo.

echo [1/6] Starting StackOverflow API Service...
start "StackOverflow API" cmd /k "cd StackOverflow\StackOverflowService && echo Starting StackOverflow API... && dotnet run"
timeout /t 3 /nobreak >nul

echo [2/6] Starting NotificationService Instance 1...
start "NotificationService-1" cmd /k "cd StackOverflow\NotificationService && echo Starting NotificationService Instance 1... && dotnet run"
timeout /t 2 /nobreak >nul

echo [3/6] Starting NotificationService Instance 2...
start "NotificationService-2" cmd /k "cd StackOverflow\NotificationService && echo Starting NotificationService Instance 2... && dotnet run"
timeout /t 2 /nobreak >nul

echo [4/6] Starting NotificationService Instance 3...
start "NotificationService-3" cmd /k "cd StackOverflow\NotificationService && echo Starting NotificationService Instance 3... && dotnet run"
timeout /t 2 /nobreak >nul

echo [5/6] Starting HealthMonitoringService Instance 1...
start "HealthMonitor-Instance1" cmd /k "cd StackOverflow\HealthMonitoringService && echo Starting HealthMonitoring Instance 1... && dotnet run"
timeout /t 2 /nobreak >nul

echo [6/6] Starting HealthMonitoringService Instance 2...
start "HealthMonitor-Instance2" cmd /k "cd StackOverflow\HealthMonitoringService && echo Starting HealthMonitoring Instance 2... && dotnet run"
timeout /t 2 /nobreak >nul

echo [7/6] Starting React Frontend...
start "React App" cmd /k "cd my-app && echo Starting React App... && npm start"

echo.
echo ✅ All services are starting!
echo.
echo Services:
echo - StackOverflow API: http://localhost:5167
echo - NotificationService: 3 instances (background worker + health endpoint on port 5168)
echo - HealthMonitoringService: 2 instances monitoring health every 4 seconds
echo - React Frontend: http://localhost:3000
echo.
echo Press any key to exit this launcher...
pause >nul
