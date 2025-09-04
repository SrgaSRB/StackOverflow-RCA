@echo off
echo Starting StackOverflow Services...
echo.

REM Start StackOverflow API Service
echo [1/2] Starting StackOverflow API Service...
start "StackOverflow API" cmd /k "cd /d "c:\Users\Ana\Documents\Faks\4. god\Cloud\StackOverflow-RCA\StackOverflow\StackOverflowService" && echo Starting StackOverflow API... && dotnet run"

REM Wait a moment for the API to start
timeout /t 3 /nobreak >nul

REM Start NotificationService  
echo [2/2] Starting NotificationService...
start "NotificationService" cmd /k "cd /d "c:\Users\Ana\Documents\Faks\4. god\Cloud\StackOverflow-RCA\StackOverflow\NotificationService" && echo Starting NotificationService... && dotnet run"

echo.
echo ✅ Both services are starting!
echo.
echo Services:
echo - StackOverflow API: http://localhost:5167
echo - NotificationService: Background worker
echo.
echo Press any key to exit this launcher...
pause >nul
