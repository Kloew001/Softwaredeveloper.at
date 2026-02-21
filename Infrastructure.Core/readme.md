<div align="center">
<h1>http://softwaredeveloper.at</h1>
</div>

# Infrastructure.Core

`Infrastructure.Core` ist die zentrale Basis-Bibliothek (`.NET 8`) für wiederverwendbare, "infrastruktur-nahe" Bausteine.
Sie bündelt Querschnittsthemen (z.B. Datenzugriff, Hosted Services, Settings, Logging, Utilities), die in mehreren Anwendungen/Services identisch gebraucht werden.

## Wofür ist die Bibliothek gedacht?

- Reduzieren von Boilerplate-Code in neuen Projekten (Standard-Patterns stehen bereits als Services/Extensions bereit)
- Vereinheitlichen von Infrastruktur-Entscheidungen (z.B. EF Core Zugriffsmuster, Options/Settings, Logging)
- Bereitstellen von typischen "Building Blocks" aus echten Projekten (Identity/Monitor/Document-Utilities usw.)

## Enthaltene Module (Auswahl)

### Entity Framework Core / Datenzugriff

- Abstraktion über `IDbContextHandler` / `BaseDbContextHandler` und konkrete `DbContextHandler`-Implementierungen in den Provider-Projekten
- CRUD-/Query-Services
  - `EntityService` (typische Create/Update/Delete-Operationen)
  - `EntityQueryService` (Query-/Read-Operationen)
- Basis-`DbContext` für gemeinsame Konventionen (`SoftwaredeveloperDotAtDbContextCore`)

### Background / Hosted Services

- `BaseHostedService` als Grundlage für wiederkehrende Hintergrundaufgaben
- `BackgroundTrigger` zum Triggern/Orchestrieren von Background-Work (z.B. aus Requests heraus)

### Konfiguration / Settings

- `ApplicationSettingsCore` als zentrale Struktur für "Core"-Settings
- Unterstützung für Options-Binding (Microsoft.Extensions.Options) und Konfigurations-Patterns

### Access Conditions / Berechtigungsregeln

- `IAccessCondition`, `AccessConditionService` / `AccessService` zur Modellierung und Auswertung von Zugriffsbedingungen (z.B. objekt-/kontextabhängige Regeln)

### Sektionen (Feature-Bausteine)

Je nach Anwendung können einzelne Sections verwendet werden, u.a.:

- Identity (User/Role Basics, Seeds)
- Monitor (Monitoring-Services)
- Multilingual (Mehrsprachigkeit, inkl. Excel-Import/Export)
- DocumentManagement Utilities
  - Excel (`ExcelUtility`, ClosedXML)
  - PDF (`PdfTextExtractor`, iText)
  - Word (`WordTextExtractor`)
- E-Mail (`EmailMessage`, `IEMailSender`)
- Caching-Utilities (z.B. `MemoryCacheExtensions`)

## Installation / Einbindung

### Als ProjectReference

- In der konsumierenden `.csproj`:
  - `ProjectReference` auf `Infrastructure.Core/Infrastructure.Core.csproj`

### Als NuGet-Paket

Das Projekt ist für NuGet-Packing vorbereitet (`PackageReadmeFile` -> `readme.md`).

## Verwendung (typische Schritte)

1. Infrastruktur-Projekt referenzieren (ProjectReference/NuGet)
2. Benötigte Sections/Services via DI registrieren (je nach Modul)
3. EF Core: passenden Provider ergänzen (`Infrastructure.Core.PostgreSQL` oder `Infrastructure.Core.SqlServer`)

## Voraussetzungen

- .NET SDK 8

## Repository

- https://github.com/Kloew001/Softwaredeveloper.at