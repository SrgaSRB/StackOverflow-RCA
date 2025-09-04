# Testing HealthMonitoringService

## Preduslovi
1. Azurite treba da bude pokrenut
2. StackOverflowService treba da bude pokrenut na portu 5167
3. NotificationService treba da bude pokrenut na portu 5168

## Korak 1: Pokretanje servisa

### Automatsko pokretanje svih servisa
```bash
.\start-all-services.bat
```

### Manuelno pokretanje
```bash
# Terminal 1: StackOverflowService
cd StackOverflow\StackOverflowService
dotnet run

# Terminal 2: NotificationService
cd StackOverflow\NotificationService
dotnet run

# Terminal 3: HealthMonitoringService Instance 1
cd StackOverflow\HealthMonitoringService
dotnet run

# Terminal 4: HealthMonitoringService Instance 2
cd StackOverflow\HealthMonitoringService
dotnet run
```

## Korak 2: Testiranje Health Endpoints

### StackOverflowService Health Check
```bash
curl http://localhost:5167/health-monitoring
```

Očekivani odgovor:
```json
{
  "Status": "Healthy",
  "Timestamp": "2025-09-04T10:00:00Z",
  "Service": "StackOverflowService",
  "Version": "1.0.0"
}
```

### NotificationService Health Check
```bash
curl http://localhost:5168/health-monitoring
```

Očekivani odgovor:
```json
{
  "Status": "Healthy",
  "Timestamp": "2025-09-04T10:00:00Z",
  "Service": "NotificationService",
  "Version": "1.0.0"
}
```

## Korak 3: Provera HealthMonitoringService logova

U terminalima gde rade HealthMonitoringService instance, trebalo bi da vidite:

```
info: HealthMonitoringService.Worker[0]
      Health Monitoring Service started. Checking services every 4 seconds.
info: HealthMonitoringService.Services.HealthCheckService[0]
      Health check passed for StackOverflowService in 25ms
info: HealthMonitoringService.Services.HealthCheckService[0]
      Health check passed for NotificationService in 18ms
info: HealthMonitoringService.Services.HealthCheckService[0]
      Health check saved: StackOverflowService - OK
info: HealthMonitoringService.Services.HealthCheckService[0]
      Health check saved: NotificationService - OK
```

## Korak 4: Testiranje fail scenarija

### Isključite StackOverflowService
1. Zaustavite StackOverflowService (Ctrl+C u terminalu)
2. Sačekajte 4 sekunde
3. Proverite logove HealthMonitoringService-a

Trebalo bi da vidite:
```
warn: HealthMonitoringService.Services.HealthCheckService[0]
      Health check failed for StackOverflowService: HttpRequestException
info: HealthMonitoringService.Services.EmailService[0]
      Alert email sent to admin@stackoverflow.com for service StackOverflowService
info: HealthMonitoringService.Services.EmailService[0]
      Alert email sent to devops@stackoverflow.com for service StackOverflowService
info: HealthMonitoringService.Services.HealthCheckService[0]
      Sent 2 alert emails for StackOverflowService
info: HealthMonitoringService.Services.HealthCheckService[0]
      Health check saved: StackOverflowService - NOT_OK
```

## Korak 5: Provera Azure Table Storage

Možete koristiti Azure Storage Explorer ili Azurite Explorer da proverite tabele:

### HealthCheck Table
- Sadrži sve health check zapise
- Status: "OK" ili "NOT_OK"
- ServiceName: "StackOverflowService" ili "NotificationService"
- ResponseTimeMs: vreme odgovora u milisekundama

### AlertEmails Table
- Sadrži email adrese za slanje alert-ova
- Default: admin@stackoverflow.com, devops@stackoverflow.com

## Korak 6: Dodavanje novih alert email adresa

Možete dodati nove email adrese direktno u AlertEmails tabelu preko Azure Storage Explorer-a:

- PartitionKey: "ALERT_EMAIL"
- RowKey: email adresa
- Email: email adresa
- Name: ime osobe
- IsActive: true

## Troubleshooting

### Problem: Health check timeout
- Proverite da li su servisi pokrenuti na ispravnim portovima
- Proverite firewall postavke

### Problem: Email se ne šalje
- Ažurirajte SMTP postavke u appsettings.json
- Proverite da li imate valid email credentials

### Problem: Azure Table Storage greške
- Proverite da li je Azurite pokrenut
- Proverite connection string u appsettings.json

### Problem: Duplicate logovi
- Ovo je normalno jer rade 2 instance servisa
- Svaka instanca nezavisno vrši health check-ove
