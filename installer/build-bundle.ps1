param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$root = Split-Path $PSScriptRoot -Parent
$project = "$root\src\RevitAdapter\RevitQualityChecker.RevitAdapter.csproj"
$bundleName = "RevitQualityChecker.bundle"
$outputDir = "$PSScriptRoot\output"
$bundleDir = "$outputDir\$bundleName"
$contentsDir = "$bundleDir\Contents"

Write-Host "Building RevitAdapter ($Configuration)..." -ForegroundColor Cyan
dotnet build $project -c $Configuration
if ($LASTEXITCODE -ne 0) { throw "Build failed." }

$buildOutput = "$root\src\RevitAdapter\bin\$Configuration"

if (Test-Path $outputDir) { Remove-Item $outputDir -Recurse -Force }
New-Item -ItemType Directory -Path $contentsDir -Force | Out-Null
New-Item -ItemType Directory -Path "$contentsDir\rules" -Force | Out-Null

Copy-Item "$PSScriptRoot\PackageContents.xml" "$bundleDir\"
Copy-Item "$PSScriptRoot\Contents\RevitQualityChecker.addin" "$contentsDir\"

$dlls = @(
    "RevitQualityChecker.RevitAdapter.dll",
    "RevitQualityChecker.Core.dll",
    "RevitQualityChecker.Reporting.dll",
    "Newtonsoft.Json.dll"
)
foreach ($dll in $dlls) {
    Copy-Item "$buildOutput\$dll" "$contentsDir\"
}

Copy-Item "$buildOutput\rules\default-rules.json" "$contentsDir\rules\"

$zipPath = "$outputDir\RevitQualityChecker.bundle.zip"
if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
Compress-Archive -Path $bundleDir -DestinationPath $zipPath

Write-Host ""
Write-Host "Bundle created:" -ForegroundColor Green
Write-Host "  Folder: $bundleDir"
Write-Host "  ZIP:    $zipPath"
Write-Host ""
Write-Host "To install manually, copy the '$bundleName' folder to:" -ForegroundColor Yellow
Write-Host '  %ProgramData%\Autodesk\ApplicationPlugins\'
