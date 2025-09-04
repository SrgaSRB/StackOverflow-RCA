# AdminToolsConsoleApp

Konzolna aplikacija za upravljanje email adresama za upozorenja StackOverflow Health Monitoring sistema.

## Funkcionalnosti

- **Prikaz svih email adresa** - Lista svih registrovanih email adresa sa statusom (aktivan/neaktivan)
- **Dodavanje nove email adrese** - Dodavanje nove email adrese za primanje upozorenja
- **Brisanje email adrese** - Uklanjanje postojeće email adrese iz sistema
- **Aktiviranje/Deaktiviranje** - Menjanje statusa email adrese bez brisanja

## Pokretanje aplikacije

### Preduslovi
- .NET 8.0 SDK
- Azurite (Azure Storage Emulator) mora biti pokrenuto
- HealthMonitoringService mora biti konfigurisan da koristi istu bazu

### Koraci za pokretanje

1. **Pokretanje Azurite-a**
   ```bash
   azurite --silent --location c:\azurite --debug c:\azurite\debug.log
   ```

2. **Pokretanje aplikacije**
   ```bash
   cd StackOverflow\AdminToolsConsoleApp
   dotnet run
   ```

## Konfiguracija

Aplikacija koristi `appsettings.json` fajl za konfiguraciju:

```json
{
  "ConnectionStrings": {
    "AzureStorage": "UseDevelopmentStorage=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  }
}
```

## Struktura podataka

Aplikacija radi sa Azure Table Storage tabelom `AlertEmails` koja ima sledeću strukturu:

- **PartitionKey**: "ALERT_EMAIL" (konstantna vrednost)
- **RowKey**: Email adresa (jedinstveni identifikator)
- **Email**: Email adresa za primanje upozorenja
- **Name**: Ime vlasnika email adrese
- **IsActive**: Boolean vrednost koja označava da li je email aktivan

## Primer korišćenja

1. **Pokretanje aplikacije**
   - Aplikacija će prikazati glavni meni sa opcijama

2. **Dodavanje novog email-a**
   - Izaberite opciju 2
   - Unesite validnu email adresu
   - Unesite ime vlasnika
   - Potvrdite dodavanje

3. **Pregled email adresa**
   - Izaberite opciju 1
   - Videćete tabelu sa svim email adresama

4. **Deaktiviranje email adrese**
   - Izaberite opciju 4
   - Izaberite email iz liste
   - Potvrdite promenu statusa

## Validacija

- Email adrese se validiraju korišćenjem `System.Net.Mail.MailAddress` klase
- Duplikati email adresa nisu dozvoljeni
- Ime vlasnika ne sme biti prazno

## Logovanje

Aplikacija koristi Microsoft.Extensions.Logging za logovanje:
- Informacije o uspešnim operacijama
- Upozorenja o neispravnim podacima
- Greške tokom izvršavanja operacija

## Napomene

- Aplikacija radi direktno sa istom Azure Table Storage bazom koju koristi HealthMonitoringService
- Promene se odmah reflektuju u HealthMonitoringService sistemu
- Aplikacija ne zahteva restartovanje HealthMonitoringService-a
- Preporučuje se da se pre brisanja email adresa proveri da li su aktivni procesi koji koriste te adrese
