# Changelog

All notable changes to this project will be documented in this file.

The format is based on Keep a Changelog and this project follows Semantic Versioning.

## [Unreleased]

### Added
- Security policy in `SECURITY.md`
- Dependency update automation in `.github/dependabot.yml`
- Code scanning via `.github/workflows/codeql.yml`
- CI improvements in `.github/workflows/CI.yml` (build, tests, vulnerability audit)
- Support policy in `SUPPORT.md`
- Release process guide in `RELEASING.md`
- NuGet publish workflow hardening in `.github/workflows/publish-nuget.yml` (tag validation, tests, SBOM, GitHub release assets)

### Changed
- NuGet release trigger now enforces `vMAJOR.MINOR.PATCH.REVISION` format via workflow validation
