# HealthStatusService - Web Role za prikaz zdravlja servisa

## Pregled

HealthStatusService je MVC aplikacija koja pruža vizuelni prikaz dostupnosti StackOverflowService-a kroz Web Role.

## Implementirane funkcionalnosti

### 1. Čitanje podataka iz HealthCheck tabele
- Aplikacija čita podatke iz Azure Table Storage
- Prikazuje podatke za poslednja 3 sata
- Connection string konfigurisan za lokalni Azurite development storage

### 2. Vizuelni prikaz
- **Dashboard sa karticama** - prikazuje ključne metrike dostupnosti
- **Chart.js grafikon** - linijski grafikon dostupnosti po satima
- **Tabela sa satnim podacima** - detaljni pregled po satima
- **Tabela sa najnovijim proverama** - poslednjih 50 health check-ova

### 3. Procentualna dostupnost
- Izračunava ukupan procenat dostupnosti za period od 3 sata
- Prikazuje procenat nedostupnosti
- Grupiše podatke po satima za detaljnu analizu

## Struktura aplikacije

```
HealthStatusService/
├── Controllers/
│   └── HomeController.cs           # Glavni kontroler za dashboard
├── Models/
│   ├── HealthCheck.cs              # Model za health check podatke
│   └── HealthStatusViewModel.cs    # ViewModel za prikaz
├── Services/
│   └── HealthCheckService.cs       # Servis za čitanje iz Azure Table Storage
├── Views/
│   ├── Home/
│   │   └── Index.cshtml            # Glavni dashboard
│   └── Shared/
│       ├── _Layout.cshtml          # Layout template
│       ├── _ViewStart.cshtml       # View start configuration
│       └── _ViewImports.cshtml     # View imports
├── wwwroot/
│   ├── css/
│   │   └── site.css                # Custom CSS stilovi
│   └── js/
│       └── site.js                 # JavaScript funkcionalnost
├── Properties/
│   └── launchSettings.json         # Launch konfiguracija
├── appsettings.json                # Glavni config
├── appsettings.Development.json    # Development config
├── Program.cs                      # Application entry point
└── HealthStatusService.csproj      # Project file
```

## Pokretanje

1. Pokretanje preko batch fajla:
   ```
   start-health-status-service.bat
   ```

2. Ili direktno preko dotnet CLI:
   ```
   cd StackOverflow/HealthStatusService
   dotnet run --urls="http://localhost:5123"
   ```

3. Otvorite browser na: `http://localhost:5123`

## Features Dashboard-a

### Summary kartice
- **Availability** - ukupan procenat dostupnosti (zelena)
- **Unavailability** - ukupan procenat nedostupnosti (crvena)
- **Successful Checks** - broj uspešnih provera (plava)
- **Failed Checks** - broj neuspešnih provera (žuta)

### Grafički prikaz
- Linijski grafikon koji prikazuje dostupnost po satima
- Koristi Chart.js biblioteku
- Y-osa prikazuje procenat dostupnosti (0-100%)
- X-osa prikazuje sate

### Tabele
1. **Hourly Status Summary** - satni pregled sa:
   - Vremenom
   - Brojem provera
   - Uspešnim proverama
   - Neuspešnim proverama
   - Procentom dostupnosti
   - Status (OK/DEGRADED)

2. **Recent Health Checks** - poslednje provere sa:
   - Timestamp
   - Naziv servisa
   - Status
   - Vreme odgovora
   - Poruka o grešci

### Auto-refresh
- Stranica se automatski osvežava svakih 30 sekundi
- Omogućava real-time monitoring

## Konfiguracija

### Connection String
```json
{
  "ConnectionStrings": {
    "AzureStorage": "UseDevelopmentStorage=true"
  }
}
```

### Dependencies
- Azure.Data.Tables - za pristup Azure Table Storage
- Microsoft.AspNetCore.Mvc - za MVC funkcionalnost
- Bootstrap 5 - za responsive UI
- Chart.js - za grafikone

## Error Handling

Ako nema podataka u tabeli ili Azure Table Storage nije dostupan, aplikacija generiše sample podatke za demonstraciju funkcionalnosti.

## URL Endpoints

- `/` - Glavni dashboard
- `/Home/GetChartData` - JSON API za chart podatke
