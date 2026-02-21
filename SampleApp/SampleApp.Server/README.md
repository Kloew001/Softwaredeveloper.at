```markdown
# SampleApp.Server

ASP.NET Core Host-Anwendung (`.NET 8`) für die SampleApp. Stellt API-Endpunkte bereit und hostet (via SPA Proxy) den Angular-Client aus `SampleApp/sampleapp.client`.

## Tech-Stack

- ASP.NET Core (Web)
- Swagger (Swashbuckle)
- SPA Proxy für Angular Dev-Server

## Start (lokal)

### Backend

- Start über Visual Studio / `dotnet run` im Projekt `SampleApp.Server`.

### Frontend (Angular)

Der `SpaProxyLaunchCommand` ist auf `npm start` konfiguriert. Alternativ manuell:

- `cd SampleApp/sampleapp.client`
- `npm install`
- `npm start`

## Konfiguration

- `appsettings.json`
- `appsettings.Development.json`

## Projektabhängigkeiten

- `Infrastructure.Core.Web`
- `Infrastructure.Core.PostgreSQL`
- `SampleApp.Application`

## Repository

- https://github.com/Kloew001/Softwaredeveloper.at
```