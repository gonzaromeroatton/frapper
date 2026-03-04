using Frapper.Core.Domain.Schema;

namespace Frapper.SqlServer.Introspection;

/// <summary>
/// Defines a contract for reading database schema information.
/// </summary>
public interface ISchemaReader
{
    Task<DatabaseSchema> ReadAsync(string connectionString, CancellationToken ct = default);
}