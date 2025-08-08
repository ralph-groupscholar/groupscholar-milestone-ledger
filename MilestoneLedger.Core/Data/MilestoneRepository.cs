using MilestoneLedger.Core.Models;
using Npgsql;

namespace MilestoneLedger.Core.Data;

public sealed class MilestoneRepository
{
    private const string Schema = "gs_milestone_ledger";
    private readonly string _connectionString;

    public MilestoneRepository(DbConfig config)
    {
        _connectionString = config.BuildConnectionString();
    }

    public async Task EnsureSchemaAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var sql = $@"
            CREATE SCHEMA IF NOT EXISTS {Schema};

            CREATE TABLE IF NOT EXISTS {Schema}.scholars (
                id SERIAL PRIMARY KEY,
                full_name TEXT NOT NULL UNIQUE,
                cohort TEXT,
                status TEXT,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );

            CREATE TABLE IF NOT EXISTS {Schema}.milestones (
                id SERIAL PRIMARY KEY,
                scholar_id INT NOT NULL REFERENCES {Schema}.scholars(id) ON DELETE CASCADE,
                milestone_type TEXT NOT NULL,
                milestone_date DATE NOT NULL,
                notes TEXT,
                risk_flag BOOLEAN NOT NULL DEFAULT FALSE,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );

            CREATE INDEX IF NOT EXISTS milestones_milestone_date_idx ON {Schema}.milestones (milestone_date);
            CREATE INDEX IF NOT EXISTS milestones_type_idx ON {Schema}.milestones (milestone_type);
            CREATE INDEX IF NOT EXISTS scholars_cohort_idx ON {Schema}.scholars (cohort);
        ";

        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        var countCommand = new NpgsqlCommand($"SELECT COUNT(*) FROM {Schema}.scholars;", connection, transaction);
        var existing = ReadLong(await countCommand.ExecuteScalarAsync(cancellationToken));
        if (existing > 0)
        {
            await transaction.CommitAsync(cancellationToken);
            return;
        }

        var scholars = new List<(string Name, string Cohort, string Status)>
        {
            ("Ava Mitchell", "Spring 2025", "active"),
            ("Darius Reed", "Spring 2025", "active"),
            ("Mei Santos", "Fall 2024", "active"),
            ("Priya Patel", "Fall 2024", "paused"),
            ("Jordan Walker", "Spring 2023", "alumni")
        };

        var scholarIds = new Dictionary<string, int>();
        foreach (var scholar in scholars)
        {
            var insertScholar = new NpgsqlCommand(
                $"INSERT INTO {Schema}.scholars (full_name, cohort, status) VALUES (@name, @cohort, @status) RETURNING id;",
                connection,
                transaction);
            insertScholar.Parameters.AddWithValue("name", scholar.Name);
            insertScholar.Parameters.AddWithValue("cohort", scholar.Cohort);
            insertScholar.Parameters.AddWithValue("status", scholar.Status);
            var id = ReadId(await insertScholar.ExecuteScalarAsync(cancellationToken));
            scholarIds[scholar.Name] = id;
        }

        var milestones = new List<MilestoneInput>
        {
            new("Ava Mitchell", "Spring 2025", "active", "Scholarship Awarded", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-20)), "Award letter received.", false),
            new("Ava Mitchell", "Spring 2025", "active", "Mentor Match", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)), "Matched with STEM mentor.", false),
            new("Darius Reed", "Spring 2025", "active", "FAFSA Filed", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-14)), "Submitted FAFSA with counselor support.", false),
            new("Mei Santos", "Fall 2024", "active", "Internship Offer", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-6)), "Accepted summer placement.", false),
            new("Priya Patel", "Fall 2024", "paused", "Re-engagement", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-4)), "Needs follow-up on pause plan.", true),
            new("Jordan Walker", "Spring 2023", "alumni", "First Job", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30)), "Started full-time role.", false)
        };

        foreach (var milestone in milestones)
        {
            await InsertMilestoneAsync(connection, transaction, scholarIds[milestone.FullName], milestone, cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task AddMilestoneAsync(MilestoneInput input, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        var scholarId = await EnsureScholarAsync(connection, transaction, input, cancellationToken);
        await InsertMilestoneAsync(connection, transaction, scholarId, input, cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MilestoneEntry>> ListMilestonesAsync(DateOnly? startDate, DateOnly? endDate, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var sql = $@"
            SELECT m.id,
                   s.full_name,
                   s.cohort,
                   s.status,
                   m.milestone_type,
                   m.milestone_date,
                   m.notes,
                   m.risk_flag,
                   m.created_at
            FROM {Schema}.milestones m
            JOIN {Schema}.scholars s ON s.id = m.scholar_id
            WHERE (@start IS NULL OR m.milestone_date >= @start)
              AND (@end IS NULL OR m.milestone_date <= @end)
            ORDER BY m.milestone_date DESC, s.full_name ASC;";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("start", (object?)startDate ?? DBNull.Value);
        command.Parameters.AddWithValue("end", (object?)endDate ?? DBNull.Value);

        var results = new List<MilestoneEntry>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new MilestoneEntry(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.IsDBNull(2) ? null : reader.GetString(2),
                reader.IsDBNull(3) ? null : reader.GetString(3),
                reader.GetString(4),
                reader.GetFieldValue<DateOnly>(5),
                reader.IsDBNull(6) ? null : reader.GetString(6),
                reader.GetBoolean(7),
                reader.GetFieldValue<DateTimeOffset>(8)
            ));
        }

        return results;
    }

    public async Task<DigestSummary> GetDigestAsync(DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var totalsSql = $@"
            SELECT COUNT(*), COALESCE(SUM(CASE WHEN m.risk_flag THEN 1 ELSE 0 END), 0)
            FROM {Schema}.milestones m
            WHERE m.milestone_date BETWEEN @start AND @end;";

        await using var totalsCommand = new NpgsqlCommand(totalsSql, connection);
        totalsCommand.Parameters.AddWithValue("start", startDate);
        totalsCommand.Parameters.AddWithValue("end", endDate);
        await using var totalsReader = await totalsCommand.ExecuteReaderAsync(cancellationToken);
        int totalMilestones = 0;
        int riskFlags = 0;
        if (await totalsReader.ReadAsync(cancellationToken))
        {
            totalMilestones = totalsReader.GetInt32(0);
            riskFlags = totalsReader.GetInt32(1);
        }

        await totalsReader.CloseAsync();

        var byType = await LoadCountsAsync(connection,
            $@"SELECT m.milestone_type, COUNT(*)
               FROM {Schema}.milestones m
               WHERE m.milestone_date BETWEEN @start AND @end
               GROUP BY m.milestone_type
               ORDER BY COUNT(*) DESC, m.milestone_type ASC;",
            startDate,
            endDate,
            cancellationToken);

        var byCohort = await LoadCountsAsync(connection,
            $@"SELECT COALESCE(s.cohort, 'Unassigned') AS cohort, COUNT(*)
               FROM {Schema}.milestones m
               JOIN {Schema}.scholars s ON s.id = m.scholar_id
               WHERE m.milestone_date BETWEEN @start AND @end
               GROUP BY COALESCE(s.cohort, 'Unassigned')
               ORDER BY COUNT(*) DESC, cohort ASC;",
            startDate,
            endDate,
            cancellationToken);

        var byStatus = await LoadCountsAsync(connection,
            $@"SELECT COALESCE(s.status, 'unknown') AS status, COUNT(*)
               FROM {Schema}.milestones m
               JOIN {Schema}.scholars s ON s.id = m.scholar_id
               WHERE m.milestone_date BETWEEN @start AND @end
               GROUP BY COALESCE(s.status, 'unknown')
               ORDER BY COUNT(*) DESC, status ASC;",
            startDate,
            endDate,
            cancellationToken);

        return new DigestSummary(startDate, endDate, totalMilestones, riskFlags, byType, byCohort, byStatus);
    }

    public async Task<IReadOnlyList<AttentionRecord>> GetAttentionRecordsAsync(
        DateOnly asOf,
        string? cohort,
        string? status,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var sql = $@"
            SELECT s.full_name,
                   s.cohort,
                   s.status,
                   MAX(m.milestone_date) AS last_milestone_date,
                   COALESCE(SUM(CASE WHEN m.risk_flag THEN 1 ELSE 0 END), 0) AS risk_count,
                   MAX(CASE WHEN m.risk_flag THEN m.milestone_date ELSE NULL END) AS latest_risk_date
            FROM {Schema}.scholars s
            LEFT JOIN {Schema}.milestones m
              ON m.scholar_id = s.id
             AND m.milestone_date <= @asOf
            WHERE (@cohort IS NULL OR s.cohort = @cohort)
              AND (@status IS NULL OR s.status = @status)
            GROUP BY s.id, s.full_name, s.cohort, s.status
            ORDER BY s.full_name ASC;";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("asOf", asOf);
        command.Parameters.AddWithValue("cohort", (object?)cohort ?? DBNull.Value);
        command.Parameters.AddWithValue("status", (object?)status ?? DBNull.Value);

        var results = new List<AttentionRecord>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new AttentionRecord(
                reader.GetString(0),
                reader.IsDBNull(1) ? null : reader.GetString(1),
                reader.IsDBNull(2) ? null : reader.GetString(2),
                reader.IsDBNull(3) ? null : reader.GetFieldValue<DateOnly>(3),
                reader.GetInt32(4),
                reader.IsDBNull(5) ? null : reader.GetFieldValue<DateOnly>(5)
            ));
        }

        return results;
    }

    private static async Task<int> EnsureScholarAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, MilestoneInput input, CancellationToken cancellationToken)
    {
        var select = new NpgsqlCommand($"SELECT id, cohort, status FROM {Schema}.scholars WHERE full_name = @name;", connection, transaction);
        select.Parameters.AddWithValue("name", input.FullName);
        await using var reader = await select.ExecuteReaderAsync(cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            var id = reader.GetInt32(0);
            var existingCohort = reader.IsDBNull(1) ? null : reader.GetString(1);
            var existingStatus = reader.IsDBNull(2) ? null : reader.GetString(2);
            await reader.CloseAsync();

            if ((input.Cohort is not null && input.Cohort != existingCohort) || (input.Status is not null && input.Status != existingStatus))
            {
                var update = new NpgsqlCommand(
                    $"UPDATE {Schema}.scholars SET cohort = COALESCE(@cohort, cohort), status = COALESCE(@status, status) WHERE id = @id;",
                    connection,
                    transaction);
                update.Parameters.AddWithValue("cohort", (object?)input.Cohort ?? DBNull.Value);
                update.Parameters.AddWithValue("status", (object?)input.Status ?? DBNull.Value);
                update.Parameters.AddWithValue("id", id);
                await update.ExecuteNonQueryAsync(cancellationToken);
            }

            return id;
        }

        await reader.CloseAsync();

        var insert = new NpgsqlCommand(
            $"INSERT INTO {Schema}.scholars (full_name, cohort, status) VALUES (@name, @cohort, @status) RETURNING id;",
            connection,
            transaction);
        insert.Parameters.AddWithValue("name", input.FullName);
        insert.Parameters.AddWithValue("cohort", (object?)input.Cohort ?? DBNull.Value);
        insert.Parameters.AddWithValue("status", (object?)input.Status ?? DBNull.Value);
        return ReadId(await insert.ExecuteScalarAsync(cancellationToken));
    }

    private static async Task InsertMilestoneAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, int scholarId, MilestoneInput input, CancellationToken cancellationToken)
    {
        var insert = new NpgsqlCommand(
            $"INSERT INTO {Schema}.milestones (scholar_id, milestone_type, milestone_date, notes, risk_flag) VALUES (@scholarId, @type, @date, @notes, @risk);",
            connection,
            transaction);
        insert.Parameters.AddWithValue("scholarId", scholarId);
        insert.Parameters.AddWithValue("type", input.MilestoneType);
        insert.Parameters.AddWithValue("date", input.MilestoneDate);
        insert.Parameters.AddWithValue("notes", (object?)input.Notes ?? DBNull.Value);
        insert.Parameters.AddWithValue("risk", input.RiskFlag);
        await insert.ExecuteNonQueryAsync(cancellationToken);
    }

    private static int ReadId(object? value)
    {
        if (value is null || value is DBNull)
        {
            throw new InvalidOperationException("Expected database to return an id.");
        }

        return Convert.ToInt32(value);
    }

    private static long ReadLong(object? value)
    {
        if (value is null || value is DBNull)
        {
            throw new InvalidOperationException("Expected database to return a value.");
        }

        return Convert.ToInt64(value);
    }

    private static async Task<IReadOnlyDictionary<string, int>> LoadCountsAsync(
        NpgsqlConnection connection,
        string sql,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken)
    {
        var result = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("start", startDate);
        command.Parameters.AddWithValue("end", endDate);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result[reader.GetString(0)] = reader.GetInt32(1);
        }

        return result;
    }
}
