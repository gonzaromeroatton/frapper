using Frapper.Core.Domain.Schema;

namespace Frapper.Core.Domain.Plan.Ops;

/// <summary>
/// Represents a migration operation for creating a new database table.
/// </summary>
/// <param name="Table"></param>
public sealed record CreateTableOp(DbTable Table) : IMigrationOp
{
    public string Kind => "CreateTable";
}