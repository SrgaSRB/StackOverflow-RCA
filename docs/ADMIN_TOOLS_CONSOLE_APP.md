# AdminToolsConsoleApp - Implementacija

## Pregled

`AdminToolsConsoleApp` je konzolna aplikacija koja omogućava upravljanje email adresama za upozorenja u StackOverflow Health Monitoring sistemu. Aplikacija komunicira direktno sa Azure Table Storage bazom koju koristi `HealthMonitoringService`.

## Implementirane funkcionalnosti

### 1. Prikaz svih email adresa
- Lista svih registrovanih email adresa
- Prikazuje email, ime vlasnika i status (aktivan/neaktivan)
- Sortira email adrese alfabetski

### 2. Dodavanje nove email adrese
- Validacija email formata
- Provera da li email već postoji
- Dodavanje novog email-a sa imenom vlasnika
- Automatsko postavljanje na aktivan status

### 3. Brisanje email adrese
- Prikaz svih dostupnih email adresa
- Numerisano biranje email-a za brisanje
- Potvrda pre brisanja radi bezbednosti

### 4. Aktiviranje/Deaktiviranje email adrese
- Menjanje statusa bez brisanja email-a
- Korisno za privremeno isključivanje upozorenja
- Očuva sve druge podatke

## Tehnička implementacija

### Arhitektura
```
Program.cs (Main + UI Logic)
├── Services/
│   └── AlertEmailService.cs (Business Logic)
└── Models/
    └── AlertEmail.cs (Data Model)
```

### Dependency Injection
- `IConfiguration` - za pristup konfiguraciji
- `ILogger` - za logovanje operacija
- `AlertEmailService` - za business logiku

### Azure Table Storage integracija
- Koristi `Azure.Data.Tables` NuGet paket
- Deli istu tabelu `AlertEmails` sa `HealthMonitoringService`
- Automatski kreira tabelu ako ne postoji

### Sigurnosni aspekti
- Validacija email formata pomoću `System.Net.Mail.MailAddress`
- Provera duplikata pre dodavanja
- Potvrda pre brisanja
- Try-catch blokovi za sve kritične operacije

## Korisničko iskustvo

### Interfejs
- Tekstualni meni sa jasnim opcijama
- Numerisano biranje opcija
- Potvrda za destruktivne operacije
- Jasne poruke o uspešnosti/neuspešnosti operacija

### Navigacija
- Glavni meni sa 5 opcija
- Automatski povratak na glavni meni nakon operacije
- Mogućnost izlaska iz bilo kog koraka

### Validacija unosa
- Provera da li su polja prazna
- Validacija email formata
- Numerička validacija za biranje opcija

## Povezanost sa sistemom

### Realtime sinhronizacija
- Promene se odmah reflektuju u `HealthMonitoringService`
- Nema potrebe za restartovanjem servisa
- Deli istu konfiguraciju za Azure Storage

### Konsistentnost podataka
- Koristi isti data model kao `HealthMonitoringService`
- Isti PartitionKey i RowKey strategija
- Automatsko kreiranje potrebnih tabela

## Način korišćenja

1. **Setup**
   ```bash
   # Pokretanje Azurite
   azurite --silent --location c:\azurite --debug c:\azurite\debug.log
   
   # Pokretanje aplikacije
   dotnet run --project AdminToolsConsoleApp
   ```

2. **Tipičan workflow**
   - Pokretanje aplikacije
   - Opcija 1: Pregled trenutnih email adresa
   - Opcija 2: Dodavanje nove email adrese
   - Opcija 1: Potvrda da je email dodat
   - Opcija 5: Izlaz

3. **Upravljanje postojećim email adresama**
   - Opcija 4: Deaktiviranje email adrese (umesto brisanja)
   - Opcija 3: Brisanje email adrese (definitvno uklanjanje)

## Prednosti implementacije

1. **Jednostavnost korišćenja** - Intuitivan tekstualni interfejs
2. **Sigurnost** - Potvrda pre kritičnih operacija
3. **Realtime** - Odmah se reflektuje u sistemu
4. **Consistent** - Koristi iste modele i konfiguraciju
5. **Robust** - Kompletno error handling i logging
6. **Maintainable** - Jasna separation of concerns
