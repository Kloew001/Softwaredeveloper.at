```markdown
# SampleApp.Application

Applikations-/Domain-Layer (`.NET 8`) der SampleApp. Enthält Domain-Logik, Services und (falls verwendet) EF Core DbContext/Migrations.

## Inhalt

- Domain Startup/Registrierung (z.B. `DomainStartup`)
- Sections/Services (z.B. Person-Logik, DTO-Factories)
- EF Core (inkl. `Migrations`-Ordner)

## Verwendung

Wird von `SampleApp.Server` referenziert und stellt die fachliche Logik über DI bereit.

## Voraussetzungen

- .NET SDK 8

## Abhängigkeiten

- `Infrastructure.Core`
- `Infrastructure.Core.PostgreSQL`

## Repository

- https://github.com/Kloew001/Softwaredeveloper.at
```