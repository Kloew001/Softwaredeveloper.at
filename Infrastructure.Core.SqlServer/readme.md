# Infrastructure.Core.SqlServer

`Infrastructure.Core.SqlServer` ergänzt `Infrastructure.Core` um SQL-Server-spezifische Implementierungen (`.NET 8`).
Es richtet sich an Anwendungen, die Entity Framework Core mit SQL Server verwenden und optional Distributed Cache über SQL Server betreiben.

## Was ist enthalten?

### Entity Framework Core (SQL Server)

- `DbContextHandler`
  - SQL-Server-spezifische Implementierung der Core-DbContext-Abstraktion
- Provider-Abhängigkeit:
  - `Microsoft.EntityFrameworkCore.SqlServer`

### Distributed Cache (SQL Server)

- `DistributedCache`
  - Wrapper/Integration für `Microsoft.Extensions.Caching.SqlServer`

## Verwendung (typisch)

1. Referenz hinzufügen: `Infrastructure.Core.SqlServer`
2. Connection String konfigurieren
3. DbContext/Handler registrieren (DI)
4. Optional: Cache aktivieren und SQL Cache Table konfigurieren

## Voraussetzungen

- .NET SDK 8
- Microsoft SQL Server

## Abhängigkeiten

- Referenziert `Infrastructure.Core`

## Repository

- https://github.com/Kloew001/Softwaredeveloper.at