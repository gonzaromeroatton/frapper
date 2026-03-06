using Microsoft.Data.SqlClient;
using Frapper.Core.Domain.Schema;
using Frapper.SqlServer.Internal;
using Frapper.SqlServer.Normalization;

namespace Frapper.SqlServer.Introspection;

/// <summary>
/// 
/// </summary>
public sealed class SqlServerSchemaReader : ISchemaReader
{
    /// <summary>
    /// Reads the database schema information from a SQL Server database using the provided connection string. It retrieves information about tables, columns, and primary keys, and constructs a DatabaseSchema object that represents the structure of the database. The method executes multiple queries against the system catalog views to gather the necessary metadata and organizes it into a deterministic schema representation.
    /// </summary>
    /// <param name="connectionString"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<DatabaseSchema> ReadAsync(string connectionString, CancellationToken ct = default)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(ct);

        // 1) Tables
        var tables = new List<(int ObjectId, string Schema, string Name)>();
        await using (var cmd = new SqlCommand(SysCatalogQueries.Tables, conn))
        await using (var r = await cmd.ExecuteReaderAsync(ct))
        {
            while (await r.ReadAsync(ct))
            {
                tables.Add((
                    ObjectId: r.GetInt32("ObjectId"),
                    Schema: r.GetString("SchemaName"),
                    Name: r.GetString("TableName")
                ));
            }
        }

        // 2) Columns
        var columnsByObjectId = new Dictionary<int, List<DbColumn>>();
        await using (var cmd = new SqlCommand(SysCatalogQueries.Columns, conn))
        await using (var r = await cmd.ExecuteReaderAsync(ct))
        {
            while (await r.ReadAsync(ct))
            {
                var objectId = r.GetInt32("ObjectId");

                var columnId = r.GetInt32("ColumnId");
                var name = r.GetString("ColumnName");
                var typeName = r.GetString("TypeName");
                var maxLength = r.GetInt16("MaxLength");
                var precision = r.GetByte("Precision");
                var scale = r.GetByte("Scale");
                var isNullable = r.GetBoolean("IsNullable");
                var isIdentity = r.GetBoolean("IsIdentity");
                var defaultSql = r.GetNullableString("DefaultDefinition");

                var sqlType = SqlServerTypeNormalizer.Normalize(typeName, maxLength, precision, scale);

                if (!columnsByObjectId.TryGetValue(objectId, out var list))
                    columnsByObjectId[objectId] = list = new List<DbColumn>();

                list.Add(new DbColumn(
                    ColumnId: columnId,
                    Name: name,
                    Type: sqlType,
                    IsNullable: isNullable,
                    IsIdentity: isIdentity,
                    DefaultSql: defaultSql
                ));
            }
        }

        // 3) Primary keys
        var pkByObjectId = new Dictionary<int, PrimaryKeyAcc>();
        await using (var cmd = new SqlCommand(SysCatalogQueries.PrimaryKeys, conn))
        await using (var r = await cmd.ExecuteReaderAsync(ct))
        {
            while (await r.ReadAsync(ct))
            {
                var objectId = r.GetInt32("ObjectId");
                var pkName = r.GetString("ConstraintName");
                var colName = r.GetString("ColumnName");
                var indexTypeDesc = r.GetString("IndexTypeDesc");

                var isClustered = indexTypeDesc.Contains("CLUSTERED", StringComparison.OrdinalIgnoreCase);

                if (!pkByObjectId.TryGetValue(objectId, out var acc))
                    pkByObjectId[objectId] = acc = new PrimaryKeyAcc(pkName, isClustered);

                // PK columns come already ordered by key_ordinal in the query
                acc.Columns.Add(colName);

                // si por algún motivo el índice cambia, conservamos "clustered" si aparece en alguna fila
                acc.IsClustered |= isClustered;
            }
        }

        // 4) Build deterministic schema
        var dbTables = new List<DbTable>(tables.Count);

        foreach (var t in tables.OrderBy(x => x.Schema, StringComparer.OrdinalIgnoreCase)
                                .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase))
        {
            columnsByObjectId.TryGetValue(t.ObjectId, out var cols);
            cols ??= new List<DbColumn>();

            // determinístico y semánticamente correcto: por ColumnId
            var orderedCols = cols.OrderBy(c => c.ColumnId).ToList();

            DbPrimaryKey? pk = null;
            if (pkByObjectId.TryGetValue(t.ObjectId, out var acc))
            {
                pk = new DbPrimaryKey(
                    Name: acc.Name,
                    Columns: acc.Columns,
                    IsClustered: acc.IsClustered
                );
            }

            dbTables.Add(new DbTable(
                Schema: t.Schema,
                Name: t.Name,
                Columns: orderedCols,
                PrimaryKey: pk
            ));
        }

        return new DatabaseSchema(
            Provider: "SqlServer",
            Tables: dbTables,
            FormatVersion: "1.0"
        );
    }

    private sealed class PrimaryKeyAcc
    {
        public PrimaryKeyAcc(string name, bool isClustered)
        {
            Name = name;
            IsClustered = isClustered;
        }

        public string Name { get; }
        public bool IsClustered { get; set; }
        public List<string> Columns { get; } = new();
    }
}