namespace Frapper.Core.Domain.Schema;

/// <summary>
/// Represents a database column, including its column ID, name, SQL data type, nullability, identity property, and default SQL definition if applicable.
/// </summary>
/// <param name="ColumnId"></param>
/// <param name="Name"></param>
/// <param name="Type"></param>
/// <param name="IsNullable"></param>
/// <param name="IsIdentity"></param>
/// <param name="DefaultSql"></param>
public sealed record DbColumn(
    int ColumnId,
    string Name,
    SqlType Type,
    bool IsNullable,
    bool IsIdentity,
    string? DefaultSql
);