@echo off
echo 🚀 Starting Complete StackOverflow System...
echo.

echo [1/3] Starting StackOverflow API Service...
start "StackOverflow API" cmd /k "cd StackOverflow\StackOverflowService && echo Starting StackOverflow API... && dotnet run"
timeout /t 3 /nobreak >nul

echo [2/3] Starting NotificationService...
start "NotificationService" cmd /k "cd StackOverflow\NotificationService && echo Starting NotificationService... && dotnet run"
timeout /t 2 /nobreak >nul

echo [3/3] Starting React Frontend...
start "React App" cmd /k "cd my-app && echo Starting React App... && npm start"

echo.
echo ✅ All services are starting!
echo.
echo Services:
echo - StackOverflow API: http://localhost:5167
echo - NotificationService: Background worker
echo - React Frontend: http://localhost:3000
echo.
echo Press any key to exit this launcher...
pause >nul
