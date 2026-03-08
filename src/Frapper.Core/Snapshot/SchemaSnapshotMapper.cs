using Frapper.Core.Domain.Schema;

namespace Frapper.Core.Snapshot;

public static class SchemaSnapshotMapper
{
    public static SchemaSnapshotDocument ToDocument(DatabaseSchema schema)
    {
        ArgumentNullException.ThrowIfNull(schema);

        return new SchemaSnapshotDocument
        {
            FormatVersion = schema.FormatVersion,
            DatabaseEngine = schema.Provider,
            Tables = schema.Tables
                .OrderBy(t => t.Schema, StringComparer.OrdinalIgnoreCase)
                .ThenBy(t => t.Name, StringComparer.OrdinalIgnoreCase)
                .Select(t => new TableSnapshotDocument
                {
                    Schema = t.Schema,
                    Name = t.Name,
                    Columns = t.Columns
                        .OrderBy(c => c.ColumnId)
                        .ThenBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
                        .Select(c => new ColumnSnapshotDocument
                        {
                            ColumnId = c.ColumnId,
                            Name = c.Name,
                            StoreType = c.Type.StoreType,
                            IsNullable = c.IsNullable,
                            IsIdentity = c.IsIdentity,
                            Length = c.Type.Length,
                            Precision = c.Type.Precision,
                            Scale = c.Type.Scale,
                            DefaultSql = c.DefaultSql
                        })
                        .ToArray(),
                    PrimaryKey = t.PrimaryKey is null
                        ? null
                        : new PrimaryKeySnapshotDocument
                        {
                            Name = t.PrimaryKey.Name,
                            Columns = t.PrimaryKey.Columns.ToArray(),
                            IsClustered = t.PrimaryKey.IsClustered
                        }
                })
                .ToArray()
        };
    }

    public static DatabaseSchema ToDomain(SchemaSnapshotDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var tables = document.Tables
            .OrderBy(t => t.Schema, StringComparer.OrdinalIgnoreCase)
            .ThenBy(t => t.Name, StringComparer.OrdinalIgnoreCase)
            .Select(t => new DbTable(
                t.Schema,
                t.Name,
                t.Columns
                    .OrderBy(c => c.ColumnId)
                    .ThenBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
                    .Select(c => new DbColumn(
                        c.ColumnId,
                        c.Name,
                        new SqlType(
                            c.StoreType,
                            c.Length,
                            c.Precision,
                            c.Scale),
                        c.IsNullable,
                        c.IsIdentity,
                        c.DefaultSql))
                    .ToArray(),
                t.PrimaryKey is null
                    ? null
                    : new DbPrimaryKey(
                        t.PrimaryKey.Name,
                        t.PrimaryKey.Columns.ToArray(),
                        t.PrimaryKey.IsClustered)))
            .ToArray();

        return new DatabaseSchema(
            document.DatabaseEngine,
            tables,
            document.FormatVersion);
    }
}