# run.ps1 - 
# Configuration
$port       = 8000
$listenHost = "127.0.0.1"              
$module     = "app.main:app"

# Colors
$green  = "`e[32m"
$yellow = "`e[33m"
$red    = "`e[31m"
$reset  = "`e[0m"

Write-Host "${green}Starting FastAPI development server...${reset}"
Write-Host "Host: $listenHost    Port: $port"
Write-Host "Module: $module"
Write-Host "${yellow}Press Ctrl+C to stop${reset}"
Write-Host ""

# Optional: activate venv if it exists
if (Test-Path ".\venv\Scripts\Activate.ps1") {
    Write-Host "${yellow}Activating virtual environment...${reset}"
    & .\venv\Scripts\Activate.ps1
}

# Run uvicorn
try {
    python -m uvicorn $module --host $listenHost --port $port --reload
}
catch {
    Write-Host "${red}Error starting server:${reset}"
    Write-Host $_.Exception.Message
    Write-Host ""
    Write-Host "${yellow}Common fixes:${reset}"
    Write-Host "1. Make sure you're in the correct project folder"
    Write-Host "2. Check if uvicorn is installed: pip install uvicorn"
    Write-Host "3. Verify DATABASE_URL in .env is correct"
    Write-Host "4. Check if PostgreSQL/Redis is running"
    exit 1
}