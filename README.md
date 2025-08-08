# Group Scholar Milestone Ledger

Group Scholar Milestone Ledger is a C# CLI that records scholar milestones, flags risk signals, and generates weekly digest summaries. It stores milestone activity in PostgreSQL to keep progress tracking durable and queryable.

## Features
- Record milestone updates with cohort/status context
- List milestones as a table or CSV export
- Weekly digest summaries with counts by type, cohort, and scholar status
- Attention queue for scholars needing follow-up (inactive or risk flagged)
- One-command schema initialization and seed data load

## Tech Stack
- C# / .NET 9
- Npgsql for PostgreSQL access
- xUnit for tests

## Setup

1. Ensure the .NET SDK is installed.
2. Export the production database credentials as environment variables:

```
export GS_MILESTONE_DB_HOST="db-acupinir.groupscholar.com"
export GS_MILESTONE_DB_PORT="23947"
export GS_MILESTONE_DB_NAME="postgres"
export GS_MILESTONE_DB_USER="ralph"
export GS_MILESTONE_DB_PASSWORD="<production-password>"
export GS_MILESTONE_DB_SSLMODE="Disable"
```

3. Initialize the schema and seed data:

```
dotnet run --project MilestoneLedger.App init
```

## Usage

```
# Record a milestone
dotnet run --project MilestoneLedger.App add \
  --name "Ava Mitchell" \
  --type "Scholarship Awarded" \
  --date 2026-02-08 \
  --cohort "Spring 2025" \
  --status active \
  --notes "Award letter received" \
  --risk false

# List milestones
dotnet run --project MilestoneLedger.App list --since 2026-01-01 --until 2026-02-08

# Export CSV
dotnet run --project MilestoneLedger.App list --format csv

# Weekly digest (last 4 weeks by default)
dotnet run --project MilestoneLedger.App digest --weeks 4

# Attention queue
dotnet run --project MilestoneLedger.App attention --inactive-days 30 --as-of 2026-02-08

# Attention queue scoped to a cohort and CSV export
dotnet run --project MilestoneLedger.App attention --cohort "Spring 2025" --format csv
```

## Notes
- The ledger uses the `gs_milestone_ledger` schema in the shared PostgreSQL database.
- Use production credentials only; do not hardcode or commit secrets.

## Tests

```
dotnet test
```
