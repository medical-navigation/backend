using Dapper;
using Npgsql;
using System.Threading;

namespace ArmNavigation.Infrastructure.Postgres.Repositories;

public class BaseRepository
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

    protected static async Task<T[]> ExecuteQueryAsync<T>(
        string sql,
        object? param = null,
        CancellationToken token = default,
        int? timeout = null)
    {
        var command = new CommandDefinition(sql, param, cancellationToken: token, commandTimeout: timeout ?? DefaultTimeoutSeconds);
        await using var conn = CreateConnection();
        var result = await conn.QueryAsync<T>(command);
        return result.ToArray();
    }

    protected static async Task<T> ExecuteQuerySingleAsync<T>(
        string sql,
        DynamicParameters param,
        CancellationToken token = default,
        int? timeout = null)
    {
        var command = new CommandDefinition(sql, param, cancellationToken: token, commandTimeout: timeout ?? DefaultTimeoutSeconds);
        await using var conn = CreateConnection();
        return await conn.QuerySingleAsync<T>(command);
    }

    protected static async Task<T?> ExecuteQuerySingleOrDefaultAsync<T>(
        string sql,
        object? param = null,
        CancellationToken token = default,
        int? timeout = null)
    {
        var command = new CommandDefinition(sql, param, cancellationToken: token, commandTimeout: timeout ?? DefaultTimeoutSeconds);
        await using var conn = CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<T>(command);
    }

    protected async Task<TResult?> ExecuteNonQueryAsync<TResult>(
        string sql,
        object? param = null,
        CancellationToken token = default,
        int? timeout = null)
    {
        var command = new CommandDefinition(
            sql, param,
            commandTimeout: timeout ?? DefaultTimeoutSeconds,
            cancellationToken: token);

        await using var connection = CreateConnection();

        return await connection.ExecuteScalarAsync<TResult?>(command);
    }

    protected static async Task<T> ExecuteScalarAsync<T>(
        string sql,
        object? param = null,
        CancellationToken token = default,
        int? timeout = null)
    {
        var command = new CommandDefinition(sql, param, cancellationToken: token, commandTimeout: timeout ?? DefaultTimeoutSeconds);
        await using var conn = CreateConnection();
        return await conn.ExecuteScalarAsync<T>(command);
    }
}