# Copilot Instructions

## Package / Library Licensing Policy

Only use permissive, commercially-free licenses (MIT, Apache 2.0, BSD). Never introduce AGPL/GPL/LGPL,
copyleft, paid/commercial, or unclear-license dependencies (e.g. iText → use `PdfPig` instead).

Before adding, replacing, or changing the version of a package in `Directory.Packages.props`, verify the
license (incl. transitive dependencies) stays within the allowed set. If a version change alters the
license model to a non-compliant one, don't apply it — find a compliant alternative or ask the user.

Keep `Directory.Packages.props` entries grouped into `ItemGroup`s by `Label` (license model, e.g.
`"MIT License"`, `"Apache License 2.0"`), sorted alphabetically by package name within each group.
