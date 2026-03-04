using Frapper.Core.Domain.Schema;

namespace Frapper.Core.Domain.Plan.Ops;

/// <summary>
/// Represents a migration operation for adding a new column to an existing database table.
/// </summary>
/// <param name="Schema"></param>
/// <param name="Table"></param>
/// <param name="Column"></param>
public sealed record AddColumnOp(string Schema, string Table, DbColumn Column) : IMigrationOp
{
    public string Kind => "AddColumn";
}