<div align="center">
<h1>http://softwaredeveloper.at</h1>
</div>

# Infrastructure.Core.Web

`Infrastructure.Core.Web` ist die ASP.NET Core Web-Erweiterungsbibliothek (`.NET 8`) für `Infrastructure.Core`.
Sie stellt wiederverwendbare Bausteine für Web-APIs bereit: Middleware, Auth (JWT), Basiskomponenten für Swagger, Logging/Serilog-Integration sowie Hilfs-Controller.

## Was macht die Bibliothek konkret?

### Middleware (Querschnitt)

- `GlobalExceptionHandler`
  - Einheitliches Error-Handling für ungefangene Exceptions
  - Liefert konsistente HTTP-Fehlerantworten
- `FullRequestLoggingMiddleware`
  - Request/Response Logging (inkl. optionaler Body-/Header-Mitschnitte, abhängig von Implementierung/Settings)
- `CurrentCultureMiddleware`
  - Setzt `CurrentCulture`/`CurrentUICulture` pro Request (z.B. anhand Header/Query/Settings)
- `SerilogAdditionalContextMiddleware`
  - Reicheres Logging durch zusätzliche Context-Properties (z.B. Correlation-/User-Infos)

### Authentifizierung / JWT

- `TokenAuthenticateService`
  - Service zum Erstellen/Validieren und Verarbeiten von JWT Tokens
- Abhängigkeiten für `Microsoft.AspNetCore.Authentication.JwtBearer` sind vorhanden

### Controller-Bausteine

- `MonitorController`
  - Endpunkte für Monitoring/Health/Status-Informationen (anwendungsabhängig)
- `MultilingualController`
  - Endpunkte für Mehrsprachigkeit (z.B. Export/Import/Lookup)

### Startup/Bootstrap Helpers

- `WebApplicationBuilderExtensions`
  - Erweiterungsmethoden für `WebApplicationBuilder` zur Standard-Registrierung typischer Web-Services
- `WebStartupCore`
  - Zentraler Einstiegspunkt für wiederkehrende Pipeline-/Service-Konfiguration

## Installation / Einbindung

- Als `ProjectReference` oder (gepackt) als NuGet referenzieren.

## Verwendung (typisch)

1. Referenz hinzufügen: `Infrastructure.Core.Web`
2. In `Program.cs` die bereitgestellten Builder-/Startup-Extensions verwenden
3. Middleware in die Pipeline einhängen (je nach gewünschter Funktionalität)
4. JWT/Swagger/Json-Optionen konfigurieren

## Abhängigkeiten

- Referenziert `Infrastructure.Core`

## Voraussetzungen

- .NET SDK 8

## Repository

- https://github.com/Kloew001/Softwaredeveloper.at