# HealthMonitoringService Documentation

## Overview
HealthMonitoringService je Worker Role servis koji kontinuirano prati dostupnost StackOverflowService i NotificationService servisa. Servis radi u 2 instance i šalje zahteve na svake 4 sekunde ka health-monitoring endpoint-ima.

## ✅ IMPLEMENTIRANO PREMA ZAHTEVIMA

### Zahtev: HealthMonitoringService (Worker Role)
- ✅ **Worker Role servis** - Implementiran kao BackgroundService
- ✅ **Naziv: HealthMonitoringService** - Kreiran folder i project
- ✅ **2 instance** - Konfigurisano za pokretanje 2 instance

### Zahtev: Na svake 4 sekunde šalje zahtev ka /health-monitoring endpoint
- ✅ **4 sekunde interval** - Timer u Worker.cs
- ✅ **StackOverflowService /health-monitoring** - GET endpoint kreiran
- ✅ **NotificationService /health-monitoring** - GET endpoint kreiran
- ✅ **Uspešan zahtev = sve OK** - HTTP 200 status proverava

### Zahtev: Neuspešan zahtev šalje mejl na mejl adrese iz Azure tabele
- ✅ **Azure tabela AlertEmails** - Kreirana tabela za email adrese
- ✅ **Slanje mejlova** - EmailService implementiran
- ✅ **SMTP konfiguracija** - U appsettings.json

### Zahtev: Upis u tabelu HealthCheck sa datum-vreme|status|naziv-servisa
- ✅ **HealthCheck tabela** - Kreirana sa Azure Table Storage
- ✅ **Datum-vreme** - DateTime kolona
- ✅ **Status** - "OK" ili "NOT_OK" kolona
- ✅ **Naziv-servisa** - ServiceName kolona
- ✅ **Svaki zahtev se upisuje** - I uspešni i neuspešni

## Architecture

### Components

1. **HealthMonitoringService** - Glavni Worker service
2. **HealthCheckService** - Servis za izvršavanje health check-ova
3. **EmailService** - Servis za slanje alert mejlova
4. **Azure Table Storage** - Za čuvanje HealthCheck logova i AlertEmails

### Tables

#### HealthCheck Table
| Kolona | Tip | Opis |
|--------|-----|------|
| PartitionKey | string | "HEALTH_CHECK" |
| RowKey | string | GUID |
| DateTime | DateTime | Datum i vreme provere |
| Status | string | "OK" ili "NOT_OK" |
| ServiceName | string | Naziv servisa |
| ErrorMessage | string | Opis greške (ako postoji) |
| ResponseTimeMs | int | Vreme odgovora u milisekundama |

#### AlertEmails Table
| Kolona | Tip | Opis |
|--------|-----|------|
| PartitionKey | string | "ALERT_EMAIL" |
| RowKey | string | Email adresa |
| Email | string | Email adresa |
| Name | string | Ime |
| IsActive | bool | Da li je aktivna |

## Configuration

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "UseDevelopmentStorage=true"
  },
  "Services": {
    "StackOverflowService": "https://localhost:5001",
    "NotificationService": "https://localhost:5002"
  },
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": "587",
    "SmtpUsername": "your-email@gmail.com",
    "SmtpPassword": "your-app-password",
    "FromEmail": "noreply@stackoverflow.com",
    "FromName": "StackOverflow Health Monitor"
  }
}
```

## Health Endpoints

### StackOverflowService
- **URL**: `GET /health-monitoring`
- **Response**: 
```json
{
  "Status": "Healthy",
  "Timestamp": "2025-09-04T10:00:00Z",
  "Service": "StackOverflowService",
  "Version": "1.0.0"
}
```

### NotificationService
- **URL**: `GET /health-monitoring`
- **Response**:
```json
{
  "Status": "Healthy",
  "Timestamp": "2025-09-04T10:00:00Z",
  "Service": "NotificationService",
  "Version": "1.0.0"
}
```

## Running the Service

### Single Instance
```bash
cd StackOverflow/HealthMonitoringService
dotnet run
```

### Multiple Instances (2 instances)
```bash
# Instance 1
cd StackOverflow/HealthMonitoringService
dotnet run

# Instance 2 (u novom terminalu)
cd StackOverflow/HealthMonitoringService
dotnet run
```

### Using Batch File
```bash
# Za pokretanje samo HealthMonitoringService
.\start-health-monitoring.bat

# Za pokretanje svih servisa uključujući HealthMonitoringService
.\start-all-services.bat
```

## Functionality

### Health Check Process
1. **Interval**: Svake 4 sekunde
2. **Targets**: StackOverflowService i NotificationService
3. **Timeout**: 10 sekundi po zahtevu
4. **Parallel Execution**: Provera oba servisa paralelno

### When Service is Healthy (OK)
1. Log u HealthCheck tabelu sa statusom "OK"
2. Zabeležiti vreme odgovora

### When Service is Unhealthy (NOT_OK)
1. Log u HealthCheck tabelu sa statusom "NOT_OK"
2. Pošaljiti alert mejlove svim aktivnim email adresama
3. Zabeležiti grešku i vreme odgovora

### Alert Email Content
- **Subject**: "Health Check Alert - [ServiceName] is DOWN"
- **Content**: Informacije o servisu, statusu, vremenu i grešci

## Default Alert Emails
Pri prvom pokretanju, automatski se dodaju default email adrese:
- admin@stackoverflow.com
- devops@stackoverflow.com

## Logging
Servis koristi standardni .NET logging za:
- Praćenje health check rezultata
- Greške tokom izvršavanja
- Informacije o poslatim alert mejlovima

## Scalability
- Servis je dizajniran da radi u više instanci
- Svaka instanca nezavisno izvršava health check-ove
- Azure Table Storage obezbeđuje thread-safe pristup podacima

## Dependencies
- Microsoft.Extensions.Hosting
- Microsoft.Extensions.Http
- Azure.Data.Tables
- MailKit (za slanje email-ova)

## Error Handling
- Connection timeout: 10 sekundi
- HTTP errors: Zabeležiti status kod
- Email sending errors: Logovati ali ne prekidati servis
- Table storage errors: Logovati ali nastaviti sa radom
