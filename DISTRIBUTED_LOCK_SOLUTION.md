# Distributed Lock Solution za HealthMonitoringService

## 🔧 Problem rešen:
- ✅ **Duplicate logs** - samo jedna instanca u isto vreme zapisuje u HealthCheck tabelu
- ✅ **Duplicate emails** - samo jedna instanca šalje alert mejlove
- ✅ **Koordinacija instanci** - instance se koordiniraju preko distributed lock-a

## 🏗️ Kako radi:

### 1. Distributed Lock Pattern
- Koristi Azure Table Storage tabelu `DistributedLocks`
- Svaka instanca pokušava da akvirira lock pre health check-a
- Lock traje 6 sekundi (više od 4s ciklusa)
- Samo instanca koja ima lock izvršava health check

### 2. Lock Lifecycle
```
Instance 1: Pokušava lock -> ✅ Dobija -> Izvršava health check -> Oslobađa lock
Instance 2: Pokušava lock -> ❌ Ne dobija -> Čeka sledeći ciklus
```

### 3. Failure Handling
- Lock automatski **expire** posle 6 sekundi
- Ako instanca sa lock-om "umre", druga instanca može da preuzme expired lock
- Redundancy se zadržava - ako jedna instanca padne, druga nastavlja

## 📊 Rezultat:

### Pre (2 instance bez koordinacije):
```
Health Check ciklus svakih 4 sekunde:
- Instance 1: ✅ Zapisuje u HealthCheck tabelu
- Instance 2: ✅ Zapisuje u HealthCheck tabelu (DUPLIKAT)
- Instance 1: ✅ Šalje 2 alert email-a  
- Instance 2: ✅ Šalje 2 alert email-a (4 email-a ukupno!)
```

### Posle (2 instance sa distributed lock):
```
Health Check ciklus svakih 4 sekunde:
- Instance 1: ✅ Dobija lock → Zapisuje u tabelu → Šalje email-ove
- Instance 2: ❌ Ne dobija lock → Čeka

Sledeći ciklus:
- Instance 2: ✅ Dobija lock → Zapisuje u tabelu → Šalje email-ove  
- Instance 1: ❌ Ne dobija lock → Čeka
```

## 🔍 Novi logovi:
```
info: HealthMonitoringService.Worker[0]
      Health Monitoring Service started. Instance: LAPTOP_12345_8_abcd1234. Checking services every 4 seconds.

dbug: HealthMonitoringService.Worker[0]
      Instance LAPTOP_12345_8_abcd1234 acquired lock - performing health checks

info: HealthMonitoringService.Services.HealthCheckService[0]
      Health check passed for StackOverflowService in 25ms

dbug: HealthMonitoringService.Worker[0]
      Instance LAPTOP_12345_8_abcd1234 completed health checks for all services

dbug: HealthMonitoringService.Worker[0]
      Instance LAPTOP_67890_9_efgh5678 - another instance is performing health checks
```

## 🚀 Prednosti:

1. **Eliminisani duplikati** - samo jedan health check po ciklusu
2. **Održana redundancy** - ako jedna instanca padne, druga preuzima
3. **Load balancing** - instance se smenjuju u izvršavanju
4. **Fault tolerance** - expired lock-ovi se automatski preuzimaju
5. **Visibility** - svaka instanca ima unique ID za tracking

## 🛠️ Implementacija:

- `DistributedLockService` - upravlja lock-ovima
- `DistributedLock` model - čuva lock podatke u Azure Table
- Modified `Worker` - koristi lock pre health check-a
- Instance ID: `MachineName_ProcessId_GUID` za uniqueness

**Sada imate 2 instance HealthMonitoringService-a koje rade koordinisano bez duplikata!** 🎉
