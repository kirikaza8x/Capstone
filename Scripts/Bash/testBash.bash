#!/bin/bash

# Usage: ./create_clean_arch.sh SmartCalo
# Creates Clean Architecture structure with src + tests and wires up references.

if [ -z "$1" ]; then
  echo "❌ Please provide a project name."
  echo "👉 Example: ./create_clean_arch.sh SmartCalo"
  exit 1
fi

SERVICE_NAME=$1
ROOT_DIR="${SERVICE_NAME}.Microservice"
SRC_DIR="$ROOT_DIR/src"
TEST_DIR="$ROOT_DIR/tests"

# ✅ Detect dotnet path dynamically using where.exe
DOTNET_PATH=$(where.exe dotnet | head -n 1)
DOTNET_DIR=$(dirname "$DOTNET_PATH" | sed 's#\\#/#g' | sed 's#C:#/c#')

export PATH=$PATH:"$DOTNET_DIR"

# Quick check
if ! command -v dotnet &> /dev/null
then
    echo "❌ dotnet CLI not found. Please verify installation."
    exit 1
fi

# Create src folders
mkdir -p "$SRC_DIR/Infrastructure" "$SRC_DIR/Api" "$SRC_DIR/Application" "$SRC_DIR/Domain"

# Create test folders
mkdir -p "$TEST_DIR/Infrastructure" "$TEST_DIR/Api" "$TEST_DIR/Application" "$TEST_DIR/Domain"

# ✅ Create projects in src
dotnet new classlib -n "${SERVICE_NAME}.Infrastructure" -o "$SRC_DIR/Infrastructure"
dotnet new classlib -n "${SERVICE_NAME}.Application"   -o "$SRC_DIR/Application"
dotnet new classlib -n "${SERVICE_NAME}.Domain"        -o "$SRC_DIR/Domain"
dotnet new webapi   -n "${SERVICE_NAME}.Api"           -o "$SRC_DIR/Api"

# ✅ Create test projects (xUnit)
dotnet new xunit -n "${SERVICE_NAME}.Infrastructure.Tests" -o "$TEST_DIR/Infrastructure"
dotnet new xunit -n "${SERVICE_NAME}.Application.Tests"    -o "$TEST_DIR/Application"
dotnet new xunit -n "${SERVICE_NAME}.Domain.Tests"         -o "$TEST_DIR/Domain"
dotnet new xunit -n "${SERVICE_NAME}.Api.Tests"            -o "$TEST_DIR/Api"

# ✅ Add references between src projects
dotnet add "$SRC_DIR/Infrastructure/${SERVICE_NAME}.Infrastructure.csproj" reference "$SRC_DIR/Application/${SERVICE_NAME}.Application.csproj"
dotnet add "$SRC_DIR/Application/${SERVICE_NAME}.Application.csproj" reference "$SRC_DIR/Domain/${SERVICE_NAME}.Domain.csproj"
dotnet add "$SRC_DIR/Api/${SERVICE_NAME}.Api.csproj" reference "$SRC_DIR/Infrastructure/${SERVICE_NAME}.Infrastructure.csproj"

# ✅ Add references from test projects to src projects
dotnet add "$TEST_DIR/Infrastructure/${SERVICE_NAME}.Infrastructure.Tests.csproj" reference "$SRC_DIR/Infrastructure/${SERVICE_NAME}.Infrastructure.csproj"
dotnet add "$TEST_DIR/Application/${SERVICE_NAME}.Application.Tests.csproj" reference "$SRC_DIR/Application/${SERVICE_NAME}.Application.csproj"
dotnet add "$TEST_DIR/Domain/${SERVICE_NAME}.Domain.Tests.csproj" reference "$SRC_DIR/Domain/${SERVICE_NAME}.Domain.csproj"
dotnet add "$TEST_DIR/Api/${SERVICE_NAME}.Api.Tests.csproj" reference "$SRC_DIR/Api/${SERVICE_NAME}.Api.csproj"

# ✅ Create solution and add all projects
dotnet new sln -n "${SERVICE_NAME}"
dotnet sln "${SERVICE_NAME}.sln" add $(find "$SRC_DIR" -name "*.csproj")
dotnet sln "${SERVICE_NAME}.sln" add $(find "$TEST_DIR" -name "*.csproj")

echo "✅ Clean Architecture solution with tests created successfully!"
tree "$ROOT_DIR" 2>/dev/null || ls -R "$ROOT_DIR"
