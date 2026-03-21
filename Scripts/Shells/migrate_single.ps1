param(
    [string]$ModuleName,
    [string]$MigrationName
)

$ScriptDir  = Split-Path -Parent $MyInvocation.MyCommand.Path
$BackendDir = $null

foreach ($candidate in @("..\..\src\Backend", "..\..\..\src\Backend")) {
    $path = Join-Path $ScriptDir $candidate
    if (Test-Path $path) {
        $BackendDir = (Resolve-Path $path).Path
        break
    }
}

if (-not $BackendDir) {
    Write-Host "[X] Could not locate 'src\Backend' from $ScriptDir." -ForegroundColor Red
    exit 1
}

$HostApiProj = Join-Path $BackendDir "Api\Api\Api.csproj"
if (-not (Test-Path $HostApiProj)) {
    Write-Host "[X] Host API project not found at $HostApiProj." -ForegroundColor Red
    exit 1
}

$AllInfraProjects = Get-ChildItem -Path "$BackendDir\Modules" -Filter "*.Infrastructure.csproj" -Recurse

# ── Module selection ──────────────────────────────────────────────────────────
if (-not $ModuleName) {
    Write-Host "`nAvailable modules:" -ForegroundColor Cyan
    $modules = $AllInfraProjects | ForEach-Object { $_.Directory.Parent.Name } | Sort-Object -Unique
    $i = 1
    foreach ($m in $modules) {
        Write-Host "  [$i] $m"
        $i++
    }

    $pick = Read-Host "`nSelect module number (or type name directly)"
    $ModuleName = if ($pick -match '^\d+$') { $modules[$([int]$pick - 1)] } else { $pick }
}

$InfraProj = $AllInfraProjects | Where-Object { $_.Directory.Parent.Name -eq $ModuleName } | Select-Object -First 1

if (-not $InfraProj) {
    Write-Host "[X] No Infrastructure project found for module '$ModuleName'." -ForegroundColor Red
    exit 1
}

# ── DbContext auto-detection ──────────────────────────────────────────────────
$ContextFiles = Get-ChildItem -Path $InfraProj.Directory.FullName -Filter "*Context.cs" -Recurse |
                Where-Object { $_.Name -notmatch "Factory" }

if (-not $ContextFiles) {
    Write-Host "[X] No DbContext found in module '$ModuleName'." -ForegroundColor Red
    exit 1
}

$DbContextName = $null

if ($ContextFiles.Count -eq 1) {
    $DbContextName = $ContextFiles[0].BaseName
    Write-Host "`n[Auto-detected] DbContext: $DbContextName" -ForegroundColor DarkGray
} else {
    Write-Host "`nMultiple DbContexts found:" -ForegroundColor Cyan
    $i = 1
    foreach ($f in $ContextFiles) {
        Write-Host "  [$i] $($f.BaseName)"
        $i++
    }
    $pick          = Read-Host "Select DbContext number"
    $DbContextName = $ContextFiles[$([int]$pick - 1)].BaseName
}

# ── Migration name ────────────────────────────────────────────────────────────
$Timestamp = Get-Date -Format "yyyyMMddHHmmss"

if (-not $MigrationName) {
    Write-Host "`nMigration name:" -ForegroundColor Cyan
    Write-Host "  [1] Auto timestamp  -> migrate_auto_$Timestamp"
    Write-Host "  [2] Custom name"
    $choice = Read-Host "Choice"

    if ($choice -eq "2") {
        $custom        = Read-Host "Enter name"
        $MigrationName = "${custom}_$Timestamp"
    } else {
        $MigrationName = "migrate_auto_$Timestamp"
    }
} else {
    $MigrationName = "${MigrationName}_$Timestamp"
}

# ── Confirm ───────────────────────────────────────────────────────────────────
Write-Host "`n============================" -ForegroundColor Cyan
Write-Host "  Module    : $ModuleName"
Write-Host "  DbContext : $DbContextName"
Write-Host "  Migration : $MigrationName"
Write-Host "  Output    : Persistence/Migrations"
Write-Host "============================`n" -ForegroundColor Cyan

$confirm = Read-Host "Proceed? (y/n)"
if ($confirm -ne 'y') {
    Write-Host "Aborted." -ForegroundColor Yellow
    exit 0
}

# ── Add migration ─────────────────────────────────────────────────────────────
Write-Host "`n[>] Adding migration..." -ForegroundColor Cyan

dotnet ef migrations add $MigrationName `
    --project "$($InfraProj.FullName)" `
    --startup-project "$HostApiProj" `
    --context "$DbContextName" `
    --output-dir "Persistence/Migrations"

if ($LASTEXITCODE -ne 0) {
    Write-Host "`n[X] Migration failed." -ForegroundColor Red
    exit 1
}

# ── Update database ───────────────────────────────────────────────────────────
Write-Host "`n[>] Updating database..." -ForegroundColor Cyan

dotnet ef database update `
    --project "$($InfraProj.FullName)" `
    --startup-project "$HostApiProj" `
    --context "$DbContextName"

if ($LASTEXITCODE -ne 0) {
    Write-Host "`n[X] Database update failed." -ForegroundColor Red
    exit 1
}

Write-Host "`n[OK] '$MigrationName' applied for $ModuleName / $DbContextName." -ForegroundColor Green
exit 0