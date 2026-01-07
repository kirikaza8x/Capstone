param(
    [string]$ProjectName,
    [string]$ModuleName
)

# Prompt if params are missing (launcher stays param-agnostic)
if (-not $ProjectName) { $ProjectName = Read-Host "Enter ProjectName (e.g., Capstone)" }
if (-not $ModuleName) { $ModuleName = Read-Host "Enter ModuleName (e.g., Example)" }

# Resolve BackendDir relative to Scripts/Shells/run.ps1
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$BackendDirCandidateA = Join-Path $ScriptDir "..\..\..\src\Backend"
$BackendDirCandidateB = Join-Path $ScriptDir "..\..\src\Backend"

if (Test-Path $BackendDirCandidateA) {
    $BackendDir = (Resolve-Path $BackendDirCandidateA).Path
} elseif (Test-Path $BackendDirCandidateB) {
    $BackendDir = (Resolve-Path $BackendDirCandidateB).Path
} else {
    Write-Host "`n[X] Could not locate 'src\Backend' from $ScriptDir." -ForegroundColor Red
    Write-Host "Tried:" -ForegroundColor Yellow
    Write-Host " - $BackendDirCandidateA" -ForegroundColor Yellow
    Write-Host " - $BackendDirCandidateB" -ForegroundColor Yellow
    exit 1
}

# Paths
$ModuleRoot   = Join-Path $BackendDir "Modules\$ModuleName"
$SrcDir       = $ModuleRoot
$ApiDir       = Join-Path $SrcDir "$ModuleName.Api"

# Solution path: prefer <ProjectName>.sln under Backend; else reuse any existing .sln; else create
$SolutionPath = Join-Path $BackendDir "$ProjectName.sln"
if (-not (Test-Path $SolutionPath)) {
    $existingSln = Get-ChildItem -Path $BackendDir -Filter *.sln -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($existingSln) {
        $SolutionPath = $existingSln.FullName
    }
}

# Auto-detect Host project (Api)
$HostProjCandidateA = Join-Path $BackendDir "Api\Api.csproj"      # e.g., src\Backend\Api\Api.csproj
$HostProjCandidateB = Join-Path $BackendDir "Api.csproj"          # e.g., src\Backend\Api.csproj
$HostProj = $null

if (Test-Path $HostProjCandidateA) {
    $HostProj = $HostProjCandidateA
} elseif (Test-Path $HostProjCandidateB) {
    $HostProj = $HostProjCandidateB
} else {
    Write-Host "`n[!] Host project not found. Expected either:" -ForegroundColor Yellow
    Write-Host " - $HostProjCandidateA" -ForegroundColor Yellow
    Write-Host " - $HostProjCandidateB" -ForegroundColor Yellow
    Write-Host "Continuing without adding host to solution; please create the host project and rerun." -ForegroundColor Yellow
}

# Print resolved paths (sanity check)
Write-Host "`n=== Resolved paths ===" -ForegroundColor Cyan
Write-Host "BackendDir:     $BackendDir" -ForegroundColor Green
Write-Host "SolutionPath:   $SolutionPath" -ForegroundColor Green
Write-Host "ModuleRoot:     $ModuleRoot" -ForegroundColor Green
Write-Host "ApiDir:         $ApiDir" -ForegroundColor Green
if ($HostProj) { Write-Host "HostProj:       $HostProj" -ForegroundColor Green } else { Write-Host "HostProj:       (not found)" -ForegroundColor Yellow }

# === SAFEGUARDS ===
$existingProjects = Get-ChildItem -Path $SrcDir -Recurse -Filter *.csproj -ErrorAction SilentlyContinue
if ($existingProjects) {
    Write-Host "`n[X] Existing project files detected under $SrcDir. Aborting to avoid overwriting." -ForegroundColor Red
    exit 1
}

# === Create folders ===
New-Item -ItemType Directory -Force -Path "$SrcDir/$ModuleName.Api","$SrcDir/$ModuleName.Application","$SrcDir/$ModuleName.Domain","$SrcDir/$ModuleName.Infrastructure" | Out-Null

# === Create projects ===
dotnet new classlib -n "$ModuleName.Api"            -o "$SrcDir/$ModuleName.Api"
dotnet new classlib -n "$ModuleName.Application"    -o "$SrcDir/$ModuleName.Application"
dotnet new classlib -n "$ModuleName.Domain"         -o "$SrcDir/$ModuleName.Domain"
dotnet new classlib -n "$ModuleName.Infrastructure" -o "$SrcDir/$ModuleName.Infrastructure"

# === Add references (Api -> Infrastructure -> Application -> Domain) ===
dotnet add "$SrcDir/$ModuleName.Infrastructure/$ModuleName.Infrastructure.csproj" reference "$SrcDir/$ModuleName.Application/$ModuleName.Application.csproj"
dotnet add "$SrcDir/$ModuleName.Application/$ModuleName.Application.csproj" reference "$SrcDir/$ModuleName.Domain/$ModuleName.Domain.csproj"
dotnet add "$SrcDir/$ModuleName.Api/$ModuleName.Api.csproj" reference "$SrcDir/$ModuleName.Infrastructure/$ModuleName.Infrastructure.csproj"

# === Solution ===
if (-not (Test-Path $SolutionPath)) {
    Write-Host "`n[!] Solution not found; creating $ProjectName.sln under $BackendDir" -ForegroundColor Yellow
    dotnet new sln -n "$ProjectName" -o $BackendDir
    $SolutionPath = Join-Path $BackendDir "$ProjectName.sln"
}
dotnet sln "$SolutionPath" add "$SrcDir/$ModuleName.Api/$ModuleName.Api.csproj"
dotnet sln "$SolutionPath" add "$SrcDir/$ModuleName.Application/$ModuleName.Application.csproj"
dotnet sln "$SolutionPath" add "$SrcDir/$ModuleName.Domain/$ModuleName.Domain.csproj"
dotnet sln "$SolutionPath" add "$SrcDir/$ModuleName.Infrastructure/$ModuleName.Infrastructure.csproj"

if ($HostProj) {
    dotnet sln "$SolutionPath" add "$HostProj"
} else {
    Write-Host "[!] Skipped adding host to solution (not found)." -ForegroundColor Yellow
}

# === Marker AssemblyReference classes ===
@"
namespace Modules.$ModuleName.Api
{
    public static class ApiAssemblyReference { }
}
"@ | Set-Content "$ApiDir/AssemblyReference.cs"

@"
namespace Modules.$ModuleName.Application
{
    public static class ApplicationAssemblyReference { }
}
"@ | Set-Content "$SrcDir/$ModuleName.Application/AssemblyReference.cs"

@"
namespace Modules.$ModuleName.Domain
{
    public static class DomainAssemblyReference { }
}
"@ | Set-Content "$SrcDir/$ModuleName.Domain/AssemblyReference.cs"

@"
namespace Modules.$ModuleName.Infrastructure
{
    public static class InfrastructureAssemblyReference { }
}
"@ | Set-Content "$SrcDir/$ModuleName.Infrastructure/AssemblyReference.cs"

Write-Host "`n[OK] Modular Monolith scaffold created for module '$ModuleName'." -ForegroundColor Green
Write-Host "[Hint] Verify the resolved paths above. If your host project path is different, create it or adjust detection." -ForegroundColor Yellow
Write-Host "[Hint] You can now build the solution via 'dotnet build $SolutionPath'." -ForegroundColor Yellow
