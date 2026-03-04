namespace Frapper.Core.Domain.Schema;

/// <summary>
/// Represents a database table, including its schema name, table name, and a list of columns defined in the table.
/// </summary>
/// <param name="Schema"></param>
/// <param name="Name"></param>
/// <param name="Columns"></param>

public sealed record DbTable(
    string Schema,
    string Name,
    IReadOnlyList<DbColumn> Columns,
    DbPrimaryKey? PrimaryKey = null
);