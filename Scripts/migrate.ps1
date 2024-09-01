param(
    [string]$project = "Lib",
    [string]$context = "LocalDbCtx",
    [string]$migrationName = "InitialMigration"
)
dotnet ef migrations add $migrationName --project $project --context $context