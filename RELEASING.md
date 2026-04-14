# Releasing Guide

## Versioning

This repository follows Semantic Versioning with four numeric parts:

`MAJOR.MINOR.PATCH.REVISION`

Release tags must use:

`vMAJOR.MINOR.PATCH.REVISION`

Example:

`v1.4.2.0`

## Release source of truth

Releases are created only via GitHub Actions from version tags.

## Release checklist

1. Update `CHANGELOG.md` under `Unreleased`
2. Create and push a tag in the required format
3. CI release workflow builds, tests, packs and publishes packages
4. CI generates an SBOM and attaches it to the GitHub release
5. (Optional but recommended) NuGet package signing is applied when signing secrets are configured

## Required repository secrets

- `NUGET_TOKEN`

## Optional signing secrets

- `NUGET_SIGN_CERT_BASE64` (base64 encoded `.pfx`)
- `NUGET_SIGN_CERT_PASSWORD`
