namespace Frapper.Core.Snapshot;

public sealed class SchemaSnapshotDocument
{
    public string FormatVersion { get; init; } = "1.0";
    public string DatabaseEngine { get; init; } = "SqlServer";
    public IReadOnlyList<TableSnapshotDocument> Tables { get; init; } = Array.Empty<TableSnapshotDocument>();
}

public sealed class TableSnapshotDocument
{
    public string Schema { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public IReadOnlyList<ColumnSnapshotDocument> Columns { get; init; } = Array.Empty<ColumnSnapshotDocument>();
    public PrimaryKeySnapshotDocument? PrimaryKey { get; init; }
}

public sealed class ColumnSnapshotDocument
{
    public int ColumnId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string StoreType { get; init; } = string.Empty;
    public bool IsNullable { get; init; }
    public bool IsIdentity { get; init; }
    public int? Length { get; init; }
    public byte? Precision { get; init; }
    public byte? Scale { get; init; }
    public string? DefaultSql { get; init; }
}

public sealed class PrimaryKeySnapshotDocument
{
    public string Name { get; init; } = string.Empty;
    public IReadOnlyList<string> Columns { get; init; } = Array.Empty<string>();
    public bool IsClustered { get; init; }
}