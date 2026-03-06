using Frapper.Core.Domain.Plan;
using Frapper.Core.Domain.Plan.Ops;
using Frapper.Core.Domain.Schema;
using Frapper.EFMigrationEmitter;

namespace Frapper.EFMigrationEmitter.Tests;

public sealed class SqlMigrationEmitterTests
{
    [Fact]
    public void Emit_ShouldGenerateAddColumnSql()
    {
        // Arrange
        var column = new DbColumn(
            ColumnId: 2,
            Name: "Age",
            Type: new SqlType("int"),
            IsNullable: true,
            IsIdentity: false,
            DefaultSql: null);

        var plan = new MigrationPlan(
            Up: new IMigrationOp[]
            {
                new AddColumnOp("dbo", "Users", column)
            },
            Down: Array.Empty<IMigrationOp>());

        // Act
        var sql = SqlMigrationEmitter.Emit(plan);

        // Assert
        Assert.Contains("ALTER TABLE [dbo].[Users] ADD [Age] int NULL;", sql);
    }

    [Fact]
    public void Emit_ShouldGenerateDropColumnSql()
    {
        // Arrange
        var plan = new MigrationPlan(
            Up: new IMigrationOp[]
            {
                new DropColumnOp("dbo", "Users", "Age")
            },
            Down: Array.Empty<IMigrationOp>());

        // Act
        var sql = SqlMigrationEmitter.Emit(plan);

        // Assert
        Assert.Contains("ALTER TABLE [dbo].[Users] DROP COLUMN [Age];", sql);
        Assert.Contains("DECLARE @df_name sysname;", sql);
    }

    [Fact]
    public void Emit_ShouldGenerateAlterColumnSql()
    {
        // Arrange
        var oldColumn = new DbColumn(
            ColumnId: 1,
            Name: "Name",
            Type: new SqlType("varchar", Length: 50),
            IsNullable: true,
            IsIdentity: false,
            DefaultSql: null);

        var newColumn = new DbColumn(
            ColumnId: 1,
            Name: "Name",
            Type: new SqlType("varchar", Length: 100),
            IsNullable: false,
            IsIdentity: false,
            DefaultSql: null);

        var plan = new MigrationPlan(
            Up: new IMigrationOp[]
            {
                new AlterColumnOp("dbo", "Users", oldColumn, newColumn)
            },
            Down: Array.Empty<IMigrationOp>());

        // Act
        var sql = SqlMigrationEmitter.Emit(plan);

        // Assert
        Assert.Contains("ALTER TABLE [dbo].[Users] ALTER COLUMN [Name] varchar(100) NOT NULL;", sql);
    }

    [Fact]
    public void Emit_ShouldGenerateCreateTableWithPrimaryKey()
    {
        // Arrange
        var table = new DbTable(
            Schema: "dbo",
            Name: "Users",
            Columns: new[]
            {
                new DbColumn(
                    ColumnId: 1,
                    Name: "Id",
                    Type: new SqlType("int"),
                    IsNullable: false,
                    IsIdentity: true,
                    DefaultSql: null),

                new DbColumn(
                    ColumnId: 2,
                    Name: "Name",
                    Type: new SqlType("varchar", Length: 50),
                    IsNullable: false,
                    IsIdentity: false,
                    DefaultSql: null)
            },
            PrimaryKey: new DbPrimaryKey(
                Name: "PK_Users",
                Columns: new[] { "Id" },
                IsClustered: true)
        );

        var plan = new MigrationPlan(
            Up: new IMigrationOp[]
            {
                new CreateTableOp(table)
            },
            Down: Array.Empty<IMigrationOp>());

        // Act
        var sql = SqlMigrationEmitter.Emit(plan);

        // Assert
        Assert.Contains("CREATE TABLE [dbo].[Users]", sql);
        Assert.Contains("[Id] int IDENTITY(1,1) NOT NULL", sql);
        Assert.Contains("CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([Id])", sql);
    }

    [Fact]
    public void Emit_ShouldGenerateDropTableSql()
    {
        // Arrange
        var plan = new MigrationPlan(
            Up: new IMigrationOp[]
            {
                new DropTableOp("dbo", "Users")
            },
            Down: Array.Empty<IMigrationOp>());

        // Act
        var sql = SqlMigrationEmitter.Emit(plan);

        // Assert
        Assert.Contains("DROP TABLE [dbo].[Users];", sql);
    }

    [Fact]
    public void Emit_ShouldWarnWhenDefaultChanges()
    {
        var oldColumn = new DbColumn(1, "Age", new SqlType("int"), true, false, null);
        var newColumn = new DbColumn(1, "Age", new SqlType("int"), true, false, "(0)");

        var plan = new MigrationPlan(
            Up: new IMigrationOp[]
            {
            new AlterColumnOp("dbo", "Users", oldColumn, newColumn)
            },
            Down: Array.Empty<IMigrationOp>());

        var sql = SqlMigrationEmitter.Emit(plan);

        Assert.Contains("DEFAULT constraint change detected", sql);
    }

    [Fact]
    public void Emit_ShouldWarnWhenIdentityChanges()
    {
        var oldColumn = new DbColumn(1, "Id", new SqlType("int"), false, false, null);
        var newColumn = new DbColumn(1, "Id", new SqlType("int"), false, true, null);

        var plan = new MigrationPlan(
            Up: new IMigrationOp[]
            {
            new AlterColumnOp("dbo", "Users", oldColumn, newColumn)
            },
            Down: Array.Empty<IMigrationOp>());

        var sql = SqlMigrationEmitter.Emit(plan);

        Assert.Contains("IDENTITY change detected", sql);
        Assert.Contains("Manual migration may be required", sql);
    }
}