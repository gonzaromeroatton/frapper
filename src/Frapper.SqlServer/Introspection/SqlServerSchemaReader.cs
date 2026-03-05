using System.Data;
using Microsoft.Data.SqlClient;
using Frapper.Core.Domain.Schema;
using Frapper.SqlServer.Normalization;
using Frapper.SqlServer.Internal;

namespace Frapper.SqlServer.Introspection;

/// <summary>
/// Reads the database schema from a SQL Server database.
/// </summary>
public sealed class SqlServerSchemaReader : ISchemaReader
{
    /// <summary>
    /// Reads the database schema from a SQL Server database.
    /// </summary>
    /// <param name="connectionString"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<DatabaseSchema> ReadAsync(string connectionString, CancellationToken ct = default)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(ct);

        // 1) Read tables
        var tables = new List<(int ObjectId, string Schema, string Name)>();
        await using (var cmd = new SqlCommand(SysCatalogQueries.Tables, conn))
        await using (var reader = await cmd.ExecuteReaderAsync(ct))
        {
            while (await reader.ReadAsync(ct))
            {
                var schema = reader.GetString(0);
                var name = reader.GetString(1);
                var objectId = reader.GetInt32(2);
                tables.Add((objectId, schema, name));
            }
        }

        // 2) Read columns (grouped by ObjectId)
        var columnsByObjectId = new Dictionary<int, List<DbColumn>>();
        await using (var cmd = new SqlCommand(SysCatalogQueries.Columns, conn))
        await using (var reader = await cmd.ExecuteReaderAsync(ct))
        {
            while (await reader.ReadAsync(ct))
            {
                var objectId = reader.GetInt32(reader.GetOrdinal("ObjectId"));

                var colName = reader.GetString(reader.GetOrdinal("ColumnName"));
                var typeName = reader.GetString(reader.GetOrdinal("TypeName"));
                var maxLength = reader.GetInt16(reader.GetOrdinal("MaxLength"));
                var precision = reader.GetByte(reader.GetOrdinal("Precision"));
                var scale = reader.GetByte(reader.GetOrdinal("Scale"));
                var isNullable = reader.GetBoolean(reader.GetOrdinal("IsNullable"));
                var isIdentity = reader.GetBoolean(reader.GetOrdinal("IsIdentity"));

                string? defaultDef = null;
                var defOrdinal = reader.GetOrdinal("DefaultDefinition");
                if (!await reader.IsDBNullAsync(defOrdinal))
                    defaultDef = reader.GetString(defOrdinal);

                var sqlType = SqlServerTypeNormalizer.Normalize(typeName, maxLength, precision, scale);

                if (!columnsByObjectId.TryGetValue(objectId, out var list))
                    columnsByObjectId[objectId] = list = new List<DbColumn>();

                list.Add(new DbColumn(
                    Name: colName,
                    Type: sqlType,
                    IsNullable: isNullable,
                    IsIdentity: isIdentity,
                    DefaultSql: defaultDef
                ));
            }
        }

        // 3) Read primary keys
        var pkByObjectId = new Dictionary<int, (string Name, bool IsClustered, List<string> Columns)>();
        await using (var cmd = new SqlCommand(SysCatalogQueries.PrimaryKeys, conn))
        await using (var reader = await cmd.ExecuteReaderAsync(ct))
        {
            while (await reader.ReadAsync(ct))
            {
                var objectId = reader.GetInt32(reader.GetOrdinal("ObjectId"));
                var pkName = reader.GetString(reader.GetOrdinal("ConstraintName"));
                var colName = reader.GetString(reader.GetOrdinal("ColumnName"));
                var indexType = reader.GetString(reader.GetOrdinal("IndexTypeDesc"));

                var isClustered = indexType.Contains("CLUSTERED", StringComparison.OrdinalIgnoreCase);

                if (!pkByObjectId.TryGetValue(objectId, out var entry))
                {
                    entry = (pkName, isClustered, new List<string>());
                    pkByObjectId[objectId] = entry;
                }

                entry.Columns.Add(colName);
                pkByObjectId[objectId] = entry;
            }
        }

        // 4) Build deterministic schema objects
        var dbTables = new List<DbTable>();

        foreach (var t in tables.OrderBy(t => t.Schema).ThenBy(t => t.Name))
        {
            columnsByObjectId.TryGetValue(t.ObjectId, out var cols);
            cols ??= new List<DbColumn>();

            // Ensure deterministic column ordering
            var orderedCols = cols.OrderBy(c => c.Name).ToList();

            DbPrimaryKey? pk = null;
            if (pkByObjectId.TryGetValue(t.ObjectId, out var pkEntry))
            {
                pk = new DbPrimaryKey(
                    Name: pkEntry.Name,
                    Columns: pkEntry.Columns, // already ordered by key_ordinal in query
                    IsClustered: pkEntry.IsClustered
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
}