$project = "BeyondTrust.SecretSafeProvider/BeyondTrust.SecretSafeProvider.csproj"
$dist    = Join-Path $PSScriptRoot "dist"

dotnet publish $project -r win-x64 -o $dist
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Get-ChildItem -Path $dist -Exclude "*.exe" | Remove-Item -Force
