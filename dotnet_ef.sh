#!/bin/bash
# dotnet_ef.sh
# Helper to run dotnet ef commands with specified projects.
# Assumes script is run from the solution root directory.

# --- How to use ---
# ./dotnet_ef.sh <dotnet ef arguments>
# Example: ./dotnet_ef.sh migrations add InitialCreate
# Example: ./dotnet_ef.sh database update
# Example: ./dotnet_ef.sh migrations script MyMigrationName

# --- Configuration: Paths relative to solution root ---
STARTUP_PROJECT="VisualAmeco.API/VisualAmeco.API.csproj"
MIGRATIONS_PROJECT="VisualAmeco.Data/VisualAmeco.Data.csproj" # Project containing DbContext & Migrations

# --- Script Logic ---
# Check if any arguments were provided
if [ $# -eq 0 ]; then
  echo "Usage: ./dotnet_ef.sh <dotnet ef arguments>"
  echo "Example: ./dotnet_ef.sh migrations add InitialCreate"
  exit 1
fi

echo "Running: dotnet ef $@"
echo "(Using Startup Project: $STARTUP_PROJECT | Migrations Project: $MIGRATIONS_PROJECT)"
echo "-----------------------------------------"

# Execute dotnet ef, passing all script arguments ("$@") through
dotnet ef --startup-project "$STARTUP_PROJECT" --project "$MIGRATIONS_PROJECT" "$@"

# Check the exit code
EXIT_CODE=$?
echo "-----------------------------------------"
if [ $EXIT_CODE -ne 0 ]; then
  echo "Error: dotnet ef command failed with exit code $EXIT_CODE"
  exit $EXIT_CODE
else
  echo "dotnet ef command completed successfully."
fi

exit 0
