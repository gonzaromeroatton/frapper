namespace Frapper.SqlServer.Introspection;

/// <summary>
/// Contains SQL queries for retrieving information from the system catalog.
/// TODO: would love to read the sql from embedded resource files instead of hardcoding them here, but this is simpler for now and we can refactor later if needed.
/// </summary>
internal static class SysCatalogQueries
{
    // Tables (user tables only)
    public const string Tables = """
    SELECT
        s.name  AS SchemaName,
        t.name  AS TableName,
        t.object_id AS ObjectId
    FROM sys.tables t
    INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
    WHERE t.is_ms_shipped = 0
    ORDER BY s.name, t.name;
    """;

    // Columns
    public const string Columns = """
SELECT
    t.object_id      AS ObjectId,
    c.column_id      AS ColumnId,
    c.name           AS ColumnName,
    ty.name          AS TypeName,
    c.max_length     AS MaxLength,
    c.precision      AS Precision,
    c.scale          AS Scale,
    c.is_nullable    AS IsNullable,
    c.is_identity    AS IsIdentity,
    dc.definition    AS DefaultDefinition
FROM sys.tables t
JOIN sys.columns c 
  ON c.object_id = t.object_id
JOIN sys.types ty 
  ON ty.user_type_id = c.user_type_id
LEFT JOIN sys.default_constraints dc
  ON dc.parent_object_id = c.object_id
 AND dc.parent_column_id = c.column_id
WHERE t.is_ms_shipped = 0
ORDER BY t.object_id, c.column_id;
""";

    // Primary Key columns (in order)
    public const string PrimaryKeys = """
    SELECT
        t.object_id AS ObjectId,
        kc.name AS ConstraintName,
        ic.key_ordinal AS KeyOrdinal,
        c.name AS ColumnName,
        i.type_desc AS IndexTypeDesc,
        i.is_unique AS IsUnique
    FROM sys.tables t
    INNER JOIN sys.key_constraints kc
        ON kc.parent_object_id = t.object_id
       AND kc.type = 'PK'
    INNER JOIN sys.indexes i
        ON i.object_id = kc.parent_object_id
       AND i.index_id = kc.unique_index_id
    INNER JOIN sys.index_columns ic
        ON ic.object_id = i.object_id
       AND ic.index_id = i.index_id
    INNER JOIN sys.columns c
        ON c.object_id = ic.object_id
       AND c.column_id = ic.column_id
    WHERE t.is_ms_shipped = 0
    ORDER BY t.object_id, ic.key_ordinal;
    """;
}