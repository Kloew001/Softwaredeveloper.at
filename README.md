# softwaredeveloper.at – Infrastructure & SampleApp

Dieses Repository enthält wiederverwendbare `.NET 8` Infrastruktur-Bibliotheken sowie eine Sample-Anwendung (Backend + Angular Client), die die Nutzung der Bibliotheken demonstriert.

## Inhalt

### Infrastruktur-Bibliotheken

- `Infrastructure.Core`
  - Allgemeine Infrastruktur-Bausteine (EF Core Helpers, Paging/Default-Sortierung, Background Jobs, Sections wie Identity/Monitor/Multilingual, Dokument-Utilities, E-Mail, Caching)
- `Infrastructure.Core.Web`
  - ASP.NET Core Web-Bausteine (Middleware, JWT, Swagger/JSON Setup, Monitoring-/Multilingual-Controller)
- `Infrastructure.Core.PostgreSQL`
  - PostgreSQL Provider-Erweiterungen (EF Core Npgsql, Distributed Cache/Lock)
- `Infrastructure.Core.SqlServer`
  - SQL Server Provider-Erweiterungen (EF Core SQL Server, Distributed Cache)
- `Infrastructure.Core.Tests`
  - Gemeinsame Test-Helfer/Basisklassen

### SampleApp

- `SampleApp/SampleApp.Server`
  - ASP.NET Core Host (API) inkl. SPA Proxy für den Angular Dev-Server
- `SampleApp/SampleApp.Application`
  - Application/Domain Layer der SampleApp
- `SampleApp/SampleApp.Application.Tests`
  - Unit Tests
- `SampleApp.Application.Tests.Integrations`
  - Integrations-Tests (Setup abhängig von Umgebung)
- `SampleApp/sampleapp.client`
  - Angular Client

## Architektur (grober Überblick)

- Die Infrastruktur-Projekte liefern "Building Blocks".
- Die SampleApp zeigt typische Integration:
  - DI Auto-Registrierung über Self-Register Pattern
  - EF Core Zugriff über provider-spezifische Implementierung (PostgreSQL/SQL Server)
  - Web-API Querschnitt über `Infrastructure.Core.Web` (Middleware, JWT, Swagger)

## Voraussetzungen

- .NET SDK 8
- Node.js (für den Angular Client)
- Optional: PostgreSQL oder SQL Server (abhängig vom verwendeten Provider)

## Quickstart: SampleApp ausführen

### 1) Backend starten

Im Verzeichnis `SampleApp/SampleApp.Server`:

```bash
dotnet run
```

### 2) Frontend starten (Angular)

Im Verzeichnis `SampleApp/sampleapp.client`:

```bash
npm install
npm start
```

Der Backend-Host ist mit SPA Proxy konfiguriert und erwartet den Angular Dev-Server typischerweise unter `https://localhost:4200`.

## Konfiguration

- `SampleApp/SampleApp.Server/appsettings.json`
- `SampleApp/SampleApp.Server/appsettings.Development.json`

Dort werden u.a. ConnectionStrings und ggf. JWT/Logging-Settings gepflegt.

## Tests

```bash
dotnet test
```

## Lizenz / Repository

- Repository: https://github.com/Kloew001/Softwaredeveloper.at