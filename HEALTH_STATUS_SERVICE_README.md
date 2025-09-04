# HEALTH_STATUS_SERVICE_README.md

## HealthStatusService - Web Role za monitoring dostupnosti

### Pregled
HealthStatusService je **Web Role aplikacija** implementirana kao ASP.NET Core MVC aplikacija koja pruža vizuelni prikaz dostupnosti StackOverflowService-a. Aplikacija čita podatke iz HealthCheck Azure Table Storage tabele i prikazuje ih kroz intuitivni dashboard.

### Funkcionalnosti

#### 1. Čitanje podataka iz HealthCheck tabele
- **Vremenski opseg**: Poslednja 3 sata
- **Izvor podataka**: Azure Table Storage (HealthChecks tabela)
- **Connection**: Koristi isti connection string kao ostali servisi
- **Filter**: `PartitionKey eq 'HEALTH_CHECK' and DateTime ge datetime'{3hoursAgo}'`

#### 2. Vizuelni prikaz dostupnosti/nedostupnosti

##### Dashboard komponente:
- **Summary kartice** sa ključnim metrikama
- **Linijski grafikon** dostupnosti po vremenu 
- **Tabela sa satnim podacima** za detaljnu analizu
- **Lista najnovijih provera** sa detaljima o greškama

##### Grafički elementi:
- **Chart.js grafikon** - dostupnost kroz vreme
- **Bootstrap kartice** - brzo pregled metrika
- **Responsive tabele** - kompatibilne sa različitim uređajima
- **Color-coded status** - vizuelno razlikovanje OK/ERROR stanja

#### 3. Procentualna dostupnost

##### Kalkulacije:
```csharp
AvailabilityPercentage = (SuccessfulChecks / TotalChecks) * 100
UnavailabilityPercentage = 100 - AvailabilityPercentage
```

##### Metrike po satima:
- Grupiranje provera po satima
- Izračunavanje dostupnosti za svaki sat
- Status određivanje (OK/DEGRADED)

### Implementacija

#### Tehnologije:
- **ASP.NET Core 8.0** - Web framework
- **Azure.Data.Tables** - Azure Table Storage pristup
- **Bootstrap 5** - UI framework
- **Chart.js** - Grafikoni
- **C# ViewModels** - Data binding

#### Arhitektura:
```
HealthStatusService/
├── Controllers/HomeController.cs      # MVC kontroler
├── Services/HealthCheckService.cs     # Business logic
├── Models/HealthStatusViewModel.cs    # Data models
└── Views/Home/Index.cshtml           # Dashboard UI
```

#### API Endpoints:
- `GET /` - Glavni dashboard
- `GET /Home/GetChartData` - JSON podatci za grafikon

### Pokretanje

#### Option 1: Batch file
```bash
start-health-status-service.bat
```

#### Option 2: Direct dotnet run
```bash
cd StackOverflow/HealthStatusService
dotnet run --urls="http://localhost:5123"
```

#### Option 3: Visual Studio
- Open StackOverflow.sln
- Set HealthStatusService as startup project
- Press F5

### Konfiguracija

#### appsettings.json:
```json
{
  "ConnectionStrings": {
    "AzureStorage": "UseDevelopmentStorage=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

#### Port configuration:
- **HTTP**: http://localhost:5123
- **HTTPS**: https://localhost:7123

### Features detaljno

#### 1. Real-time Dashboard
- **Auto-refresh**: Svakih 30 sekundi
- **Live metrike**: Trenutni status dostupnosti
- **Vremenska linija**: Poslednja 3 sata podataka

#### 2. Availability Metrics
- **Ukupna dostupnost**: Procenat uspešnih provera
- **Nedostupnost**: Procenat neuspešnih provera  
- **Uspešne provere**: Broj OK statussa
- **Neuspešne provere**: Broj ERROR statussa

#### 3. Temporal Analysis
- **Satni pregled**: Grupiranje po satima
- **Trend analiza**: Vizuelni prikaz promene kroz vreme
- **Detaljne provere**: Lista sa timestamp, status, response time

#### 4. Error Handling
- **Graceful degradation**: Sample data ako nema realnih podataka
- **Exception logging**: Console output za debugging
- **Connection resilience**: Retry logic za Azure Storage

### Integracija sa sistemom

#### Dependency na druge servise:
1. **HealthMonitoringService** - generiše podatke
2. **Azure Table Storage** - čuva health check podatke
3. **Azurite** - lokalni development storage

#### Data flow:
```
StackOverflowService → HealthMonitoringService → Azure Table Storage → HealthStatusService → Dashboard
```

### Monitoring možnosti

#### Dashboard prikazuje:
- **Service availability %** za poslednja 3 sata
- **Response time trends** kroz vreme
- **Error rate analysis** sa detaljima grešaka
- **Uptime/downtime periods** vizuelno označene

#### Alert indicators:
- **Zeleno**: Dostupnost > 95%
- **Žuto**: Dostupnost 90-95% (degraded)
- **Crveno**: Dostupnost < 90% (critical)

### Development & Deployment

#### Local development:
1. Start Azurite storage emulator
2. Run HealthMonitoringService (generiše podatke)
3. Run HealthStatusService (prikazuje dashboard)

#### Production considerations:
- Connection string za pravi Azure Storage
- Load balancing za high availability
- Caching za performance optimization
- Authentication/Authorization ako potrebno

### Testiranje

#### Manual testing:
1. Pokretanje svih servisa
2. Otvaranje dashboard-a
3. Verifikacija real-time osvežavanja
4. Provera accuracy metrika

#### Sample data testing:
- Ako nema realnih podataka, koriste se simulirani podaci
- Test različitih scenarija (high/low availability)
- UI responsiveness testing

Ovaj servis ispunjava sve zahteve za Web Role aplikaciju koja vizuelno prikazuje dostupnost StackOverflowService-a kroz čitanje HealthCheck tabele i proračunavanje procentualne dostupnosti za poslednja 3 sata.
