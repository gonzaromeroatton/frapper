using Frapper.Core.Domain.Plan;
using Frapper.Core.Domain.Plan.Ops;

namespace Frapper.Cli.Commands.Diff;

internal static class DiffSummaryMapper
{
    public static DiffSummary FromPlan(MigrationPlan plan)
    {
        var up = plan.Up;

        return new DiffSummary(
            CreatedTables: up.Count(x => x is CreateTableOp),
            DroppedTables: up.Count(x => x is DropTableOp),
            AddedColumns: up.Count(x => x is AddColumnOp),
            DroppedColumns: up.Count(x => x is DropColumnOp),
            AlteredColumns: up.Count(x => x is AlterColumnOp),
            TotalOperations: up.Count);
    }
}