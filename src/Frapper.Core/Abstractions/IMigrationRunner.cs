namespace Frapper.Core.Abstractions;

public interface IMigrationRunner
{
    Task EnsureHistoryTableExistsAsync(
        string connectionString,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetAppliedMigrationIdsAsync(
        string connectionString,
        CancellationToken cancellationToken = default);

    Task ApplyMigrationAsync(
        string connectionString,
        string migrationId,
        string sql,
        CancellationToken cancellationToken = default);
}