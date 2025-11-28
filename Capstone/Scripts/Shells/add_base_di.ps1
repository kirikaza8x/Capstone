param(
    [string]$ProjectName,
    [string]$ServiceName
)

if (-not $ProjectName -or -not $ServiceName) {
    Write-Host "`n[X] Missing parameters." -ForegroundColor Red
    Write-Host "Usage: ./add_base_di.ps1 <ProjectName> <ServiceName>" -ForegroundColor Yellow
    exit 1
}

# Paths
$ScriptDir     = Split-Path -Parent $MyInvocation.MyCommand.Path
$BackendDir    = Join-Path $ScriptDir "..\..\Backend\$ProjectName"
$MicroRoot     = Join-Path $BackendDir "$ServiceName.Microservice"
$SrcDir        = Join-Path $MicroRoot "src"
$SolutionPath  = Join-Path $BackendDir "$ProjectName.sln"

if (-not (Test-Path $SrcDir)) {
    Write-Host "`n[X] Service '$ServiceName' not found under project '$ProjectName'." -ForegroundColor Red
    exit 1
}

Write-Host "[>] Adding NuGet packages and DI setup for service '$ServiceName'..." -ForegroundColor Cyan

# ---------------- Domain ----------------
dotnet add "$SrcDir/Domain/Domain.csproj" package Ardalis.GuardClauses --version 5.0.0
dotnet add "$SrcDir/Domain/Domain.csproj" package Ardalis.Specification --version 9.3.1
dotnet add "$SrcDir/Domain/Domain.csproj" package ValueOf --version 2.0.31
dotnet add "$SrcDir/Domain/Domain.csproj" package Microsoft.Extensions.DependencyInjection.Abstractions --version 9.0.0


# ---------------- Application ----------------
dotnet add "$SrcDir/Application/Application.csproj" package AutoMapper --version 12.0.1
dotnet add "$SrcDir/Application/Application.csproj" package AutoMapper.Extensions.Microsoft.DependencyInjection --version 12.0.1
dotnet add "$SrcDir/Application/Application.csproj" package FluentValidation --version 12.0.0
dotnet add "$SrcDir/Application/Application.csproj" package FluentValidation.DependencyInjectionExtensions --version 12.0.0
dotnet add "$SrcDir/Application/Application.csproj" package MediatR --version 13.1.0
dotnet add "$SrcDir/Application/Application.csproj" package Scrutor --version 6.1.0

# ---------------- Infrastructure ----------------
dotnet add "$SrcDir/Infrastructure/Infrastructure.csproj" package Ardalis.Specification --version 9.3.1
dotnet add "$SrcDir/Infrastructure/Infrastructure.csproj" package MassTransit --version 8.5.4
dotnet add "$SrcDir/Infrastructure/Infrastructure.csproj" package MassTransit.AspNetCore --version 7.3.1
dotnet add "$SrcDir/Infrastructure/Infrastructure.csproj" package MassTransit.EntityFrameworkCore --version 8.5.4
dotnet add "$SrcDir/Infrastructure/Infrastructure.csproj" package Microsoft.EntityFrameworkCore.Tools --version 9.0.9
dotnet add "$SrcDir/Infrastructure/Infrastructure.csproj" package Microsoft.Extensions.Configuration --version 9.0.9
dotnet add "$SrcDir/Infrastructure/Infrastructure.csproj" package Microsoft.Extensions.Configuration.Json --version 9.0.9
dotnet add "$SrcDir/Infrastructure/Infrastructure.csproj" package Microsoft.Extensions.Configuration.UserSecrets --version 9.0.9
dotnet add "$SrcDir/Infrastructure/Infrastructure.csproj" package Npgsql.EntityFrameworkCore.PostgreSQL --version 9.0.4
dotnet add "$SrcDir/Infrastructure/Infrastructure.csproj" package Scrutor --version 6.1.0
dotnet add "$SrcDir/Infrastructure/Infrastructure.csproj" package Serilog --version 4.3.0
dotnet add "$SrcDir/Infrastructure/Infrastructure.csproj" package Serilog.Extensions.Logging --version 9.0.2
dotnet add "$SrcDir/Infrastructure/Infrastructure.csproj" package StackExchange.Redis --version 2.9.25

# ---------------- Api ----------------
dotnet add "$SrcDir/Api/Api.csproj" package MediatR --version 13.1.0
dotnet add "$SrcDir/Api/Api.csproj" package Microsoft.AspNetCore.Authentication.JwtBearer --version 9.0.9
dotnet add "$SrcDir/Api/Api.csproj" package Microsoft.AspNetCore.Mvc.Versioning --version 5.1.0
dotnet add "$SrcDir/Api/Api.csproj" package Microsoft.AspNetCore.OpenApi --version 9.0.9
dotnet add "$SrcDir/Api/Api.csproj" package Microsoft.EntityFrameworkCore.Design --version 9.0.9
dotnet add "$SrcDir/Api/Api.csproj" package Serilog.AspNetCore --version 9.0.0
dotnet add "$SrcDir/Api/Api.csproj" package Swashbuckle.AspNetCore --version 9.0.6

# ---------------- Scaffold DI extension classes ----------------
$diDomain = @"
using Microsoft.Extensions.DependencyInjection;

namespace Domain
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDomain(this IServiceCollection services)
        {
            return services;
        }
    }
}
"@

$diApplication = @"
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;

namespace Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(DependencyInjection).Assembly);
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
            services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
            return services;
        }
    }
}
"@

$diInfrastructure = @"
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            return services;
        }
    }
}
"@

$diApi = @"
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;

namespace Api
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApi(this IServiceCollection services)
        {
            // Use ASP.NET Core built-in ProblemDetails + OpenAPI
            services.AddProblemDetails();

            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1,0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
            });

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer();

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
            });

            return services;
        }
    }
}
"@

Set-Content "$SrcDir/Domain/DependencyInjection.cs" $diDomain
Set-Content "$SrcDir/Application/DependencyInjection.cs" $diApplication
Set-Content "$SrcDir/Infrastructure/DependencyInjection.cs" $diInfrastructure
Set-Content "$SrcDir/Api/DependencyInjection.cs" $diApi

# ---------------- Add projects to solution ----------------
if (-not (Test-Path $SolutionPath)) {
    Write-Host "[>] Creating solution $ProjectName.sln..." -ForegroundColor Cyan
    dotnet new sln -n "$ProjectName" -o $BackendDir
}

Write-Host "[>] Adding updated projects to solution $ProjectName.sln..." -ForegroundColor Cyan
dotnet sln "$SolutionPath" add "$SrcDir/Domain/Domain.csproj"
dotnet sln "$SolutionPath" add "$SrcDir/Application/Application.csproj"
dotnet sln "$SolutionPath" add "$SrcDir/Infrastructure/Infrastructure.csproj"
dotnet sln "$SolutionPath" add "$SrcDir/Api/Api.csproj"

# ---------------- Final summary ----------------
Write-Host "`n[OK] Base DI setup added with NuGet packages for service '$ServiceName'." -ForegroundColor Green
Write-Host "    [DIR]     $MicroRoot" -ForegroundColor Yellow
Write-Host "    [SLN]     $SolutionPath" -ForegroundColor Yellow
Write-Host "`n[>] You can now start wiring services into each layer." -ForegroundColor Magenta
