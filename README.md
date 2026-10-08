# Revit Quality Checker

Automated BIM model quality checker plugin for Autodesk Revit. Validates models against configurable rules, provides a quality score, and exports detailed reports.

Available on the [Autodesk App Store](#) | [Privacy Policy](PRIVACY.md)

![Results Window](installer/revitcheck.png)

## Features

- **15 Built-in Rules** across four categories: Naming Conventions, Required Parameters, Model Health, and Documentation
- **Quality Score** — 0-100 score per rule and overall, so you can track model quality over time
- **Auto-Fix** — one-click fixes for supported issues (e.g. unjoined walls)
- **HTML Report** — styled report with charts and violation tables, ready to share with your team
- **BCF Export** — BCF 2.1 format for import into Navisworks, Solibri, BIMcollab, and other coordination tools
- **Configurable Rules** — enable/disable rules or load custom JSON rule files for company standards
- **Multi-Version Support** — Revit 2022, 2023, 2024, 2025, 2026, 2027, 2028
- **Dark Theme UI** — matches Revit's interface

## Screenshots

| Results | Settings | About |
|---------|----------|-------|
| ![Results](installer/revitcheck.png) | ![Settings](installer/revitsettings.png) | ![About](installer/revitabout.png) |

## Quality Rules

| Category | Rule | Severity |
|----------|------|----------|
| Naming | View names follow standard | Warning |
| Naming | Sheet names follow standard | Warning |
| Naming | Level names follow standard | Warning |
| Naming | Grid names follow standard | Warning |
| Parameters | Walls have required parameters | Error |
| Parameters | Doors have required parameters | Error |
| Parameters | Windows have required parameters | Error |
| Health | Model warnings below threshold | Warning |
| Health | Walls are properly joined | Error |
| Health | Rooms are placed and enclosed | Error |
| Health | No duplicate sheet numbers | Error |
| Health | Room tags are placed | Warning |
| Documentation | Sheets have title blocks | Error |
| Documentation | Views on sheets | Info |
| Naming | No duplicate view names | Warning |

## Installation

### From Autodesk App Store (Recommended)

Download from the [Autodesk App Store](#) and follow the installer instructions.

### Manual Installation

1. Download the latest release from [Releases](https://github.com/kaan36875/revit-quality-checker/releases)
2. Extract `RevitQualityChecker.bundle` to `%ProgramData%\Autodesk\ApplicationPlugins\`
3. Restart Revit

## Usage

1. Open a Revit model
2. Go to the **Quality Checker** tab on the ribbon
3. *(Optional)* Click **Settings** to enable/disable rules or load a custom rules file
4. Click **Check Model** to run the analysis
5. Review results — each rule shows pass/fail, violation count, and element details
6. Click **Fix** on supported rules to auto-fix issues
7. Click **Export HTML** or **Export BCF** to generate reports

## Building from Source

### Prerequisites

- Visual Studio 2022+ or `dotnet` CLI
- .NET 10 SDK
- Revit 2022+ installed (for Revit API references)

### Build

```bash
dotnet build src/RevitAdapter -c Release
```

The post-build target automatically deploys to `%AppData%\Autodesk\Revit\Addins\2027\`.

### Run Tests

```bash
dotnet test tests/Core.Tests
```

### Create Installer Bundle

```powershell
powershell -ExecutionPolicy Bypass -File installer\build-bundle.ps1
```

Output: `installer/output/RevitQualityChecker.bundle.zip`

## Project Structure

```
src/
  Core/               # Rules engine, models, configuration (netstandard2.0)
  Reporting/           # HTML and BCF report generators (netstandard2.0)
  RevitAdapter/        # Revit plugin, WPF UI, commands (net10.0-windows)
tests/
  Core.Tests/          # Unit tests (36 tests)
rules/
  default-rules.json   # Built-in rule definitions
  ruleset-schema.json  # JSON schema for custom rules
installer/
  build-bundle.ps1     # Bundle packaging script
  PackageContents.xml  # Autodesk AppStore manifest
pyrevit/               # pyRevit MVP prototype (Python)
```

## Custom Rules

Create a JSON file following the schema in `rules/ruleset-schema.json` and load it through **Settings > Custom Rules File**. This lets you define organization-specific naming patterns, required parameters, and thresholds.

## Uninstallation

1. Delete `%ProgramData%\Autodesk\ApplicationPlugins\RevitQualityChecker.bundle\`
2. *(Optional)* Delete settings: `%AppData%\RevitQualityChecker\`

## License

[MIT](LICENSE)

## Author

**Kaan Beyazkilic** — [kaanbeyazkilic.com](https://kaanbeyazkilic.com)
