# Copilot Instructions

## Package / Library Licensing Policy

Only use third-party NuGet packages or libraries that are free for commercial use and licensed under
permissive, non-restrictive licenses such as:

- MIT
- Apache 2.0
- BSD (2-Clause / 3-Clause)

Do **not** introduce dependencies that:

- Require a paid/commercial license (e.g. iText, which is AGPL/commercial dual-licensed and not free for commercial closed-source use)
- Are licensed under AGPL, GPL, LGPL, or any other copyleft license that imposes restrictions on commercial or proprietary use
- Have unclear, proprietary, or "contact us for pricing" licensing terms

When adding or replacing a dependency, verify the license of the package (and its transitive dependencies)
before adding it to `Directory.Packages.props` / the project file. If an existing dependency violates this
policy, prefer replacing it with a suitable MIT/Apache-2.0 licensed alternative (e.g. `PdfPig` instead of `iText` for PDF text extraction).
