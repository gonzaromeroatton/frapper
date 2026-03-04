namespace Frapper.Core.Domain.Plan.Ops;

/// <summary>
/// Represents a migration operation for dropping an existing column from a database table.
/// </summary>
/// <param name="Schema"></param>
/// <param name="Table"></param>
/// <param name="Column"></param>
public sealed record DropColumnOp(string Schema, string Table, string Column) : IMigrationOp
{
    public string Kind => "DropColumn";
}