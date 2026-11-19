using System.Text.Json;
using Ecommerce_Api.Shared.Application;
using Npgsql;

namespace Ecommerce_Api.Infrastructure.Persistence;

// Each caller supplies its own module-owned schema; no cross-module SQL is permitted.
public sealed class WorkflowStore(IConfiguration configuration) : IWorkflowStore
{
    private static readonly HashSet<string> Schemas = ["sales", "payments", "billing", "incidents"];

    public async Task<T?> ReadAsync<T>(string schema, string key, CancellationToken ct)
    {
        Validate(schema);
        await using var connection = new NpgsqlConnection(configuration.GetConnectionString("PostgreSQL"));
        await connection.OpenAsync(ct);
        await using var command = new NpgsqlCommand($"SELECT payload::text FROM {schema}.process1_workflow WHERE workflow_key=@key", connection);
        command.Parameters.AddWithValue("key", key);
        var json = await command.ExecuteScalarAsync(ct) as string;
        return json is null ? default : JsonSerializer.Deserialize<T>(json);
    }

    public async Task<T> UpdateAsync<T>(string schema, string key, Func<T?, Task<T>> update, CancellationToken ct)
    {
        Validate(schema);
        await using var connection = new NpgsqlConnection(configuration.GetConnectionString("PostgreSQL"));
        await connection.OpenAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(ct);
        await using (var gate = new NpgsqlCommand("SELECT pg_advisory_xact_lock(hashtextextended(@key,0))", connection, transaction))
        {
            gate.Parameters.AddWithValue("key", $"process1:{schema}:{key}");
            await gate.ExecuteNonQueryAsync(ct);
        }
        T? current = default;
        await using (var read = new NpgsqlCommand($"SELECT payload::text FROM {schema}.process1_workflow WHERE workflow_key=@key FOR UPDATE", connection, transaction))
        {
            read.Parameters.AddWithValue("key", key);
            if (await read.ExecuteScalarAsync(ct) is string json) current = JsonSerializer.Deserialize<T>(json);
        }
        var next = await update(current);
        await using (var write = new NpgsqlCommand($"""
            INSERT INTO {schema}.process1_workflow(workflow_key,payload,revision,created_at,updated_at)
            VALUES (@key,CAST(@payload AS jsonb),1,now(),now())
            ON CONFLICT(workflow_key) DO UPDATE SET payload=EXCLUDED.payload,
                revision={schema}.process1_workflow.revision+1,updated_at=now()
            """, connection, transaction))
        {
            write.Parameters.AddWithValue("key", key);
            write.Parameters.AddWithValue("payload", JsonSerializer.Serialize(next));
            await write.ExecuteNonQueryAsync(ct);
        }
        await transaction.CommitAsync(ct);
        return next;
    }

    private static void Validate(string schema)
    {
        if (!Schemas.Contains(schema)) throw new ArgumentException("Unknown module schema.");
    }
}
