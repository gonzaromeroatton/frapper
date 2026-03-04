using Frapper.Core.Domain.Plan;
using Frapper.Core.Domain.Plan.Ops;
using Frapper.Core.Domain.Schema;

namespace Frapper.Core.Domain.Diff;

/// <summary>
/// Implements a schema differ that compares two database schemas and generates a migration plan to transition from the old schema to the new schema.
/// </summary>
public sealed class SchemaDiffer : ISchemaDiffer
{
    /// <summary>
    /// Compares the old and new database schemas and generates a migration plan that includes operations for creating new tables, dropping removed tables, adding new columns, dropping removed columns, and altering modified columns based on the specified options. The generated migration plan contains separate lists of operations for applying the changes (up) and reverting the changes (down).
    /// </summary>
    /// <param name="oldSchema"></param>
    /// <param name="newSchema"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public MigrationPlan Diff(DatabaseSchema oldSchema, DatabaseSchema newSchema, DiffOptions options)
    {
        var up = new List<IMigrationOp>();
        var down = new List<IMigrationOp>();

        var oldTables = oldSchema.Tables.ToDictionary(t => $"{t.Schema}.{t.Name}");
        var newTables = newSchema.Tables.ToDictionary(t => $"{t.Schema}.{t.Name}");

        // Detect new tables
        foreach (var newTable in newTables.Values)
        {
            var key = $"{newTable.Schema}.{newTable.Name}";

            if (!oldTables.ContainsKey(key))
            {
                up.Add(new CreateTableOp(newTable));
                down.Insert(0, new DropTableOp(newTable.Schema, newTable.Name));
            }
        }

        // Detect removed tables
        foreach (var oldTable in oldTables.Values)
        {
            var key = $"{oldTable.Schema}.{oldTable.Name}";

            if (!newTables.ContainsKey(key))
            {
                if (!options.AllowDestructiveChanges)
                    continue;

                up.Add(new DropTableOp(oldTable.Schema, oldTable.Name));
                down.Insert(0, new CreateTableOp(oldTable));
            }
        }

        // Detect column changes
        foreach (var newTable in newTables.Values)
        {
            var key = $"{newTable.Schema}.{newTable.Name}";

            if (!oldTables.TryGetValue(key, out var oldTable))
                continue;

            DiffColumns(oldTable, newTable, up, down, options);
        }

        return new MigrationPlan(up, down);
    }

    /// <summary>
    /// Compares the columns of two tables and generates migration operations for added, removed, and modified columns based on the specified options.
    /// </summary>
    /// <param name="oldTable"></param>
    /// <param name="newTable"></param>
    /// <param name="up"></param>
    /// <param name="down"></param>
    /// <param name="options"></param>
    private static void DiffColumns(
        DbTable oldTable,
        DbTable newTable,
        List<IMigrationOp> up,
        List<IMigrationOp> down,
        DiffOptions options)
    {
        var oldCols = oldTable.Columns.ToDictionary(c => c.Name);
        var newCols = newTable.Columns.ToDictionary(c => c.Name);

        // New columns
        foreach (var newCol in newCols.Values)
        {
            if (!oldCols.ContainsKey(newCol.Name))
            {
                up.Add(new AddColumnOp(newTable.Schema, newTable.Name, newCol));
                down.Insert(0, new DropColumnOp(newTable.Schema, newTable.Name, newCol.Name));
            }
        }

        // Removed columns
        foreach (var oldCol in oldCols.Values)
        {
            if (!newCols.ContainsKey(oldCol.Name))
            {
                if (!options.AllowDestructiveChanges)
                    continue;

                up.Add(new DropColumnOp(oldTable.Schema, oldTable.Name, oldCol.Name));
                down.Insert(0, new AddColumnOp(oldTable.Schema, oldTable.Name, oldCol));
            }
        }

        // Modified columns
        foreach (var newCol in newCols.Values)
        {
            if (!oldCols.TryGetValue(newCol.Name, out var oldCol))
                continue;

            if (!ColumnsEqual(oldCol, newCol))
            {
                up.Add(new AlterColumnOp(newTable.Schema, newTable.Name, oldCol, newCol));
                down.Insert(0, new AlterColumnOp(oldTable.Schema, oldTable.Name, newCol, oldCol));
            }
        }
    }

    /// <summary>
    /// Compares two database columns for equality based on their type, nullability, identity property, and default SQL expression. Returns true if the columns are considered equal, otherwise false.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    private static bool ColumnsEqual(DbColumn a, DbColumn b)
    {
        return a.Type == b.Type
            && a.IsNullable == b.IsNullable
            && a.IsIdentity == b.IsIdentity
            && a.DefaultSql == b.DefaultSql;
    }
}