# === CAPSTONE SCRIPT LAUNCHER ===

# Map each option to its script path (all inside Scripts/Shells/)
$scriptMap = @{
    1 = "create_clean_arch.ps1"
    2 = "init_postgres.ps1"
    3 = "setup_docker_env.ps1"
    4 = "generate_ci_cd.ps1"
    5 = "add_base_di.ps1"
    6 = "ef_migrate.ps1"
    7 = "ef_scaffold.ps1"
}

function Show-Menu {
    Write-Host ""
    Write-Host "=== CAPSTONE SCRIPT LAUNCHER ===" -ForegroundColor Cyan
    Write-Host "Choose an option:" -ForegroundColor Yellow
    Write-Host "1. Create Clean Architecture Microservice" -ForegroundColor Green
    Write-Host "2. Initialize PostgreSQL Database" -ForegroundColor Green
    Write-Host "3. Setup Docker Environment" -ForegroundColor Green
    Write-Host "4. Generate CI/CD Pipeline" -ForegroundColor Green
    Write-Host "5. Add Base DI + NuGet Packages" -ForegroundColor Green
    Write-Host "6. EF Core Migration + Database Update" -ForegroundColor Green
    Write-Host "7. EF Core Scaffold DbContext + Entities" -ForegroundColor Green
    Write-Host "0. Exit" -ForegroundColor Red
    Write-Host ""
}

function Show-Guide {
    param([int]$Option)
    switch ($Option) {
        1 {
            Write-Host "`nGuide for Option 1:" -ForegroundColor Cyan
            Write-Host "Creates a microservice folder structure with Clean Architecture layers." -ForegroundColor White
        }
        2 {
            Write-Host "`nGuide for Option 2:" -ForegroundColor Cyan
            Write-Host "Initializes PostgreSQL database with default schema and connection string." -ForegroundColor White
        }
        3 {
            Write-Host "`nGuide for Option 3:" -ForegroundColor Cyan
            Write-Host "Sets up Docker environment with Dockerfile and docker-compose.yml." -ForegroundColor White
        }
        4 {
            Write-Host "`nGuide for Option 4:" -ForegroundColor Cyan
            Write-Host "Generates CI/CD pipeline configuration (GitHub Actions, Azure DevOps, etc.)." -ForegroundColor White
        }
        5 {
            Write-Host "`nGuide for Option 5:" -ForegroundColor Cyan
            Write-Host "Adds NuGet packages and scaffolds Dependency Injection setup for Domain, Application, Infrastructure, and Api layers." -ForegroundColor White
        }
        6 {
            Write-Host "`nGuide for Option 6:" -ForegroundColor Cyan
            Write-Host "Runs EF Core migration: adds a migration, updates the database, and can scaffold DbContext." -ForegroundColor White
        }
        7 {
            Write-Host "`nGuide for Option 7:" -ForegroundColor Cyan
            Write-Host "Scaffolds DbContext and entity classes from an existing database using EF Core." -ForegroundColor White
        }
        default {
            Write-Host "`nNo guide available for this option yet." -ForegroundColor DarkYellow
        }
    }
}

function Run-Option {
    param([int]$Option)

    if ($Option -eq 0) {
        Write-Host "`nExiting. See you next time!" -ForegroundColor Magenta
        exit
    }

    if (-not $scriptMap.ContainsKey($Option)) {
        Write-Host "`n[X] Invalid option. Please try again." -ForegroundColor Red
        return
    }

    Show-Guide -Option $Option

    $projectName = Read-Host "`nEnter Project Name (e.g., Capstone)"
    $serviceName = Read-Host "Enter Service Name (e.g., SmartCalo)"
    $solutionName = Read-Host "Enter Solution Name (default: $projectName.sln)"

    if ([string]::IsNullOrWhiteSpace($solutionName)) {
        $solutionName = "$projectName.sln"
    }

    $scriptName = $scriptMap[$Option]
    $scriptPath = Join-Path $PSScriptRoot $scriptName

    if (-not (Test-Path $scriptPath)) {
        Write-Host "`n[X] Script not found at: $scriptPath" -ForegroundColor Red
        return
    }

    switch ($Option) {
        6 {
            $migrationName = Read-Host "Enter Migration Name (default: InitialCreate)"
            if ([string]::IsNullOrWhiteSpace($migrationName)) {
                $migrationName = "InitialCreate"
            }
            & $scriptPath $projectName $serviceName $migrationName
        }
        7 {
            $connectionString = Read-Host "Enter Connection String"
            $dbContextName = Read-Host "Enter DbContext Name (default: ProgramsDbContext)"
            if ([string]::IsNullOrWhiteSpace($dbContextName)) {
                $dbContextName = "ProgramsDbContext"
            }
            & $scriptPath $projectName $serviceName $connectionString $dbContextName
        }
        default {
            & $scriptPath $projectName $serviceName $solutionName
        }
    }
}

do {
    Show-Menu
    $choice = Read-Host "Enter your choice"
    Run-Option -Option $choice
} while ($true)
