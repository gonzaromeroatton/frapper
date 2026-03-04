namespace Frapper.Core.Domain.Plan.Ops;

/// <summary>
/// Represents a migration operation for dropping an existing database table.
/// </summary>
/// <param name="Schema"></param>
/// <param name="Name"></param>
public sealed record DropTableOp(string Schema, string Name) : IMigrationOp
{
    public string Kind => "DropTable";
}