namespace Frapper.Core.Domain.Schema;

/// <summary>
/// Represents a column in a database table, including its name, data type, nullability, identity property, and default value SQL expression.
/// </summary>
/// <param name="Name"></param>
/// <param name="Type"></param>
/// <param name="IsNullable"></param>
/// <param name="IsIdentity"></param>
/// <param name="DefaultSql"></param>
public sealed record DbColumn(
    string Name,
    SqlType Type,
    bool IsNullable,
    bool IsIdentity,
    string? DefaultSql
);