using Dapper;
using Npgsql;

namespace ArmNavigation.Infrastructure.Repositories;

public abstract class BaseRepository
{
    private const string ConnectionEnv = "POSTGRES_CONNECTION_STRING";
    private const int DefaultTimeoutSeconds = 30;

    private static string GetConnectionString()
    {
        var cs = Environment.GetEnvironmentVariable(ConnectionEnv)
                 ?? throw new InvalidOperationException($"Environment variable '{ConnectionEnv}' is not set.");
        return cs;
    }

    private static NpgsqlConnection CreateConnection() => new(GetConnectionString());

    //массив
    protected static async Task<T[]> ExecuteQueryAsync<T>(
        string sql,
        object? param = null,
        CancellationToken ct = default,
        int? timeout = null)
    {
        var command = new CommandDefinition(sql, param, cancellationToken: ct, commandTimeout: timeout ?? DefaultTimeoutSeconds);
        await using var conn = CreateConnection();
        var result = await conn.QueryAsync<T>(command);
        return result.ToArray();
    }

    //один объект (или исключение)
    protected static async Task<T> ExecuteQuerySingleAsync<T>(
        string sql,
        object? param = null,
        CancellationToken ct = default,
        int? timeout = null)
    {
        var command = new CommandDefinition(sql, param, cancellationToken: ct, commandTimeout: timeout ?? DefaultTimeoutSeconds);
        await using var conn = CreateConnection();
        return await conn.QuerySingleAsync<T>(command);
    }

    //один объект или null
    protected static async Task<T?> ExecuteQuerySingleOrDefaultAsync<T>(
        string sql,
        object? param = null,
        CancellationToken ct = default,
        int? timeout = null)
    {
        var command = new CommandDefinition(sql, param, cancellationToken: ct, commandTimeout: timeout ?? DefaultTimeoutSeconds);
        await using var conn = CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<T>(command);
    }

    //INSERT / UPDATE / DELETE (без возврата)
    protected static async Task ExecuteNonQueryAsync(
        string sql,
        object? param = null,
        CancellationToken ct = default,
        int? timeout = null)
    {
        var command = new CommandDefinition(sql, param, cancellationToken: ct, commandTimeout: timeout ?? DefaultTimeoutSeconds);
        await using var conn = CreateConnection();
        await conn.ExecuteAsync(command);
    }

    //INSERT + RETURNING (для ID)
    protected static async Task<T> ExecuteScalarAsync<T>(
        string sql,
        object? param = null,
        CancellationToken ct = default,
        int? timeout = null)
    {
        var command = new CommandDefinition(sql, param, cancellationToken: ct, commandTimeout: timeout ?? DefaultTimeoutSeconds);
        await using var conn = CreateConnection();
        return await conn.ExecuteScalarAsync<T>(command);
    }
}