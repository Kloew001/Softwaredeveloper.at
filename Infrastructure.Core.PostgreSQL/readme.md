# Infrastructure.Core.PostgreSQL

`Infrastructure.Core.PostgreSQL` ergänzt `Infrastructure.Core` um PostgreSQL-spezifische Implementierungen (`.NET 8`).
Es richtet sich primär an Anwendungen, die Entity Framework Core mit dem Npgsql-Provider verwenden und zusätzlich Distributed Cache/Locking über PostgreSQL benötigen.

## Was ist enthalten?

### Entity Framework Core (PostgreSQL)

- `DbContextHandler`
  - PostgreSQL-spezifische Implementierung der Core-DbContext-Abstraktion
- `DbContextHandlerExtensions`
  - Convenience-Extensions für Registrierung/Konfiguration (DI)
- Provider-Abhängigkeit:
  - `Npgsql.EntityFrameworkCore.PostgreSQL`

### Distributed Cache (PostgreSQL)

- `DistributedCache`
  - Integration/Wrapper für einen auf PostgreSQL basierten Distributed Cache
  - Basierend auf `Community.Microsoft.Extensions.Caching.PostgreSql`

### Distributed Lock (PostgreSQL)

- `DistributedLock`
  - Hilft beim Implementieren von "single runner" Jobs / kritischen Sektionen über mehrere Instanzen

## Verwendung (typisch)

1. Referenz hinzufügen: `Infrastructure.Core.PostgreSQL`
2. Connection String konfigurieren
3. DbContext/Handler registrieren (Extensions/DI)
4. Optional: Cache-/Locking-Services aktivieren

## Voraussetzungen

- .NET SDK 8
- PostgreSQL Datenbank

## Abhängigkeiten

- Referenziert `Infrastructure.Core`

## Repository

- https://github.com/Kloew001/Softwaredeveloper.at