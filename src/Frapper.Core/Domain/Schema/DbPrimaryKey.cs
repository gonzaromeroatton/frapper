namespace Frapper.Core.Domain.Schema;

public sealed record DbPrimaryKey(
    string Name,
    IReadOnlyList<string> Columns,
    bool IsClustered
);