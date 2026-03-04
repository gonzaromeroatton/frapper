using Frapper.Core.Domain.Schema;

namespace Frapper.Core.Domain.Plan.Ops;

/// <summary>
/// Represents a migration operation for altering an existing database column.
/// </summary>
/// <param name="Schema"></param>
/// <param name="Table"></param>
/// <param name="Old"></param>
/// <param name="New"></param>
public sealed record AlterColumnOp(string Schema, string Table, DbColumn Old, DbColumn New) : IMigrationOp
{
    public string Kind => "AlterColumn";
}