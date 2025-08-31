# Dead simple NuGet publisher
param(
    [Parameter(Mandatory=$true)]
    [string]$Package,
    
    [Parameter(Mandatory=$true)]
    [string]$Version
)

$ErrorActionPreference = "Stop"

# Build and pack
Write-Host "Publishing $Package v$Version..." -ForegroundColor Green
$proj = "src\$Package\$Package.csproj"

# Update version
(Get-Content $proj -Raw) -replace '<Version>.*?</Version>', "<Version>$Version</Version>" | Set-Content $proj -NoNewline

# Build, pack, and push
dotnet build $proj -c Release
dotnet pack $proj -c Release --no-build -o .\nupkgs

# Auto-publish if API key is set
if ($env:NUGET_API_KEY) {
    Write-Host "Publishing to NuGet.org..." -ForegroundColor Cyan
    dotnet nuget push "nupkgs\$Package.$Version.nupkg" --api-key $env:NUGET_API_KEY --source https://api.nuget.org/v3/index.json
    Write-Host "✅ Published!" -ForegroundColor Green
} else {
    Write-Host "`n✅ Package created! To publish, run:" -ForegroundColor Green
    Write-Host "dotnet nuget push nupkgs\$Package.$Version.nupkg --api-key YOUR_KEY --source https://api.nuget.org/v3/index.json" -ForegroundColor Yellow
}