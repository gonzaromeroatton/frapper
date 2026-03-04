namespace Frapper.Core.Domain.Schema;

/// <summary>
/// Represents a database schema, including its provider, a list of tables, and the format version.
/// </summary>
/// <param name="Provider"></param>
/// <param name="Tables"></param>
/// <param name="FormatVersion"></param>
public sealed record DatabaseSchema(
    string Provider,                 // "SqlServer"
    IReadOnlyList<DbTable> Tables,
    string FormatVersion             // e.g. "1.0"
);