param(
    [string]$ProjectName,
    [string]$ModuleName
)

# Prompt if params are missing
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
    exit 1
}

# Paths
$ModuleRoot   = Join-Path $BackendDir "Modules\$ModuleName"
$SrcDir       = $ModuleRoot
$ApiDir       = Join-Path $SrcDir "$ModuleName.Api"

# Solution path
$SolutionPath = Join-Path $BackendDir "$ProjectName.sln"
if (-not (Test-Path $SolutionPath)) {
    $existingSln = Get-ChildItem -Path $BackendDir -Filter *.sln -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($existingSln) { $SolutionPath = $existingSln.FullName }
}

# Auto-detect Host project
$HostProjCandidateA = Join-Path $BackendDir "Api\Api.csproj"
$HostProjCandidateB = Join-Path $BackendDir "Api.csproj"
$HostProj = $null
if (Test-Path $HostProjCandidateA) { $HostProj = $HostProjCandidateA }
elseif (Test-Path $HostProjCandidateB) { $HostProj = $HostProjCandidateB }

# Safeguard
$existingProjects = Get-ChildItem -Path $SrcDir -Recurse -Filter *.csproj -ErrorAction SilentlyContinue
if ($existingProjects) {
    Write-Host "`n[X] Existing project files detected under $SrcDir. Aborting." -ForegroundColor Red
    exit 1
}

# === Create folders ===
New-Item -ItemType Directory -Force -Path `
    "$SrcDir/$ModuleName.Api",
    "$SrcDir/$ModuleName.Application",
    "$SrcDir/$ModuleName.Domain",
    "$SrcDir/$ModuleName.Infrastructure",
    "$SrcDir/$ModuleName.IntegrationEvents",
    "$SrcDir/$ModuleName.PublicApi" | Out-Null

# === Create projects ===
dotnet new classlib -n "$ModuleName.Api"            -o "$SrcDir/$ModuleName.Api"
dotnet new classlib -n "$ModuleName.Application"    -o "$SrcDir/$ModuleName.Application"
dotnet new classlib -n "$ModuleName.Domain"         -o "$SrcDir/$ModuleName.Domain"
dotnet new classlib -n "$ModuleName.Infrastructure" -o "$SrcDir/$ModuleName.Infrastructure"
dotnet new classlib -n "$ModuleName.IntegrationEvents" -o "$SrcDir/$ModuleName.IntegrationEvents"
dotnet new webapi   -n "$ModuleName.PublicApi"        -o "$SrcDir/$ModuleName.PublicApi"

# === Add references ===
dotnet add "$SrcDir/$ModuleName.Infrastructure/$ModuleName.Infrastructure.csproj" reference "$SrcDir/$ModuleName.Application/$ModuleName.Application.csproj"
dotnet add "$SrcDir/$ModuleName.Application/$ModuleName.Application.csproj" reference "$SrcDir/$ModuleName.Domain/$ModuleName.Domain.csproj"
dotnet add "$SrcDir/$ModuleName.Api/$ModuleName.Api.csproj" reference "$SrcDir/$ModuleName.Infrastructure/$ModuleName.Infrastructure.csproj"

# IntegrationEvents -> Domain
dotnet add "$SrcDir/$ModuleName.IntegrationEvents/$ModuleName.IntegrationEvents.csproj" reference "$SrcDir/$ModuleName.Domain/$ModuleName.Domain.csproj"

# PublicApi -> Application + IntegrationEvents
dotnet add "$SrcDir/$ModuleName.PublicApi/$ModuleName.PublicApi.csproj" reference "$SrcDir/$ModuleName.Application/$ModuleName.Application.csproj"
dotnet add "$SrcDir/$ModuleName.PublicApi/$ModuleName.PublicApi.csproj" reference "$SrcDir/$ModuleName.IntegrationEvents/$ModuleName.IntegrationEvents.csproj"

# === Solution ===
if (-not (Test-Path $SolutionPath)) {
    dotnet new sln -n "$ProjectName" -o $BackendDir
    $SolutionPath = Join-Path $BackendDir "$ProjectName.sln"
}
dotnet sln "$SolutionPath" add "$SrcDir/$ModuleName.Api/$ModuleName.Api.csproj"
dotnet sln "$SolutionPath" add "$SrcDir/$ModuleName.Application/$ModuleName.Application.csproj"
dotnet sln "$SolutionPath" add "$SrcDir/$ModuleName.Domain/$ModuleName.Domain.csproj"
dotnet sln "$SolutionPath" add "$SrcDir/$ModuleName.Infrastructure/$ModuleName.Infrastructure.csproj"
dotnet sln "$SolutionPath" add "$SrcDir/$ModuleName.IntegrationEvents/$ModuleName.IntegrationEvents.csproj"
dotnet sln "$SolutionPath" add "$SrcDir/$ModuleName.PublicApi/$ModuleName.PublicApi.csproj"

if ($HostProj) { 
    dotnet sln "$SolutionPath" add "$HostProj" --solution-folder "Backend"
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

@"
namespace Modules.$ModuleName.IntegrationEvents
{
    public static class IntegrationEventsAssemblyReference { }
}
"@ | Set-Content "$SrcDir/$ModuleName.IntegrationEvents/AssemblyReference.cs"

@"
namespace Modules.$ModuleName.PublicApi
{
    public static class PublicApiAssemblyReference { }
}
"@ | Set-Content "$SrcDir/$ModuleName.PublicApi/AssemblyReference.cs"

Write-Host "`n[OK] Modular Monolith scaffold created for module '$ModuleName' with IntegrationEvents + PublicApi." -ForegroundColor Green
Write-Host "[Hint] Build via 'dotnet build $SolutionPath'." -ForegroundColor Yellow
if ($HostProj) {
    Write-Host "[Hint] Run the API via 'dotnet run --project $HostProj'." -ForegroundColor Yellow
} else {
    Write-Host "[Hint] No Host project detected. You may need to set up a host to run the API." -ForegroundColor Yellow
}