namespace Frapper.Cli.Commands.Diff;

internal sealed record DiffSummary(
    int CreatedTables,
    int DroppedTables,
    int AddedColumns,
    int DroppedColumns,
    int AlteredColumns,
    int TotalOperations
);