# Infrastructure.Core.Tests

`Infrastructure.Core.Tests` ist eine Test-Hilfsbibliothek (\`.NET 8\`) für alles, was beim Testen von Komponenten aus \`Infrastructure.Core\` und darauf aufbauenden Projekten regelmäßig wiederkehrt.
Der Fokus liegt darauf, Test-Setup-Code (Host/DI/Configuration) nicht in jedem Testprojekt neu zu schreiben.

## Was ist enthalten?

- \`BaseTest\`
  - Gemeinsame Test-Basis (z.B. für DI Container, Configuration, Setup/Teardown)
- Abhängigkeiten für typische Unit-Tests:
  - NUnit
  - Moq
  - Microsoft.Extensions.Hosting/DependencyInjection/Configuration

## Verwendung

- Dieses Projekt wird üblicherweise aus anderen Testprojekten per \`ProjectReference\` eingebunden.
- Tests erben dann bspw. von \`BaseTest\` und verwenden die bereitgestellten Konventionen/Utilities.

## Ausführen

- \`dotnet test\`
- Visual Studio Test Explorer

## Voraussetzungen

- .NET SDK 8

## Repository

- https://github.com/Kloew001/Softwaredeveloper.at