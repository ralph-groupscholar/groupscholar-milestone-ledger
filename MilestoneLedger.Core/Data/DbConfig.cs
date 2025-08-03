using Npgsql;

namespace MilestoneLedger.Core.Data;

public sealed class DbConfig
{
    public DbConfig(string host, int port, string database, string username, string password, string sslMode)
    {
        Host = host;
        Port = port;
        Database = database;
        Username = username;
        Password = password;
        SslMode = sslMode;
    }

    public string Host { get; }
    public int Port { get; }
    public string Database { get; }
    public string Username { get; }
    public string Password { get; }
    public string SslMode { get; }

    public string BuildConnectionString()
    {
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = Host,
            Port = Port,
            Database = Database,
            Username = Username,
            Password = Password,
            SslMode = Enum.TryParse<SslMode>(SslMode, true, out var mode) ? mode : Npgsql.SslMode.Require
        };

        return builder.ConnectionString;
    }

    public static DbConfig FromEnvironment()
    {
        var host = ReadRequired("GS_MILESTONE_DB_HOST");
        var port = int.TryParse(ReadRequired("GS_MILESTONE_DB_PORT"), out var parsedPort) ? parsedPort : 5432;
        var database = ReadRequired("GS_MILESTONE_DB_NAME");
        var username = ReadRequired("GS_MILESTONE_DB_USER");
        var password = ReadRequired("GS_MILESTONE_DB_PASSWORD");
        var sslMode = Environment.GetEnvironmentVariable("GS_MILESTONE_DB_SSLMODE") ?? "Disable";

        return new DbConfig(host, port, database, username, password, sslMode);
    }

    private static string ReadRequired(string key)
    {
        var value = Environment.GetEnvironmentVariable(key);
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Missing required environment variable: {key}");
        }

        return value.Trim();
    }
}
