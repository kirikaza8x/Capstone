Write-Host "`n[+] Running Unit Tests..." -ForegroundColor Cyan

$solutionPath = Join-Path $PSScriptRoot "..\..\src\Backend\Capstone.sln"

dotnet test $solutionPath `
    --configuration Release `
    --logger "trx;LogFileName=test_results.trx"

Write-Host "`n[OK] Unit tests completed." -ForegroundColor Green
