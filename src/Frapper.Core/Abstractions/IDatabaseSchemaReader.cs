using Frapper.Core.Domain.Schema;

namespace Frapper.Core.Abstractions;

public interface IDatabaseSchemaReader
{
    Task<DatabaseSchema> ReadAsync(string connectionString, CancellationToken cancellationToken = default);
}