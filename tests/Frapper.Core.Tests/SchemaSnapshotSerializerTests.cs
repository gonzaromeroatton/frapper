using Frapper.Core.Domain.Schema;
using Frapper.Core.Snapshot;
using Xunit;

namespace Frapper.Core.Tests.Snapshot;

public sealed class SchemaSnapshotSerializerTests
{
    [Fact]
    public void Serialize_ShouldBeDeterministic()
    {
        var schema = BuildSchema();
        var serializer = new SchemaSnapshotSerializer();

        var json1 = serializer.Serialize(schema);
        var json2 = serializer.Serialize(schema);

        Assert.Equal(json1, json2);
    }

    [Fact]
    public void Serialize_ShouldOrderTablesDeterministically()
    {
        var schema = new DatabaseSchema(
            "SqlServer",
            new[]
            {
                new DbTable(
                    "dbo",
                    "Users",
                    new[]
                    {
                        new DbColumn(
                            1,
                            "Id",
                            new SqlType("int"),
                            false,
                            true,
                            null)
                    },
                    new DbPrimaryKey("PK_Users", new[] { "Id" }, true)),

                new DbTable(
                    "dbo",
                    "Accounts",
                    new[]
                    {
                        new DbColumn(
                            1,
                            "Id",
                            new SqlType("int"),
                            false,
                            true,
                            null)
                    },
                    new DbPrimaryKey("PK_Accounts", new[] { "Id" }, true))
            },
            "1.0");

        var serializer = new SchemaSnapshotSerializer();
        var json = serializer.Serialize(schema);

        var accountsIndex = json.IndexOf(@"""name"": ""Accounts""", StringComparison.Ordinal);
        var usersIndex = json.IndexOf(@"""name"": ""Users""", StringComparison.Ordinal);

        Assert.True(accountsIndex >= 0);
        Assert.True(usersIndex >= 0);
        Assert.True(accountsIndex < usersIndex);
    }

    [Fact]
    public void Serialize_ShouldOrderColumnsByColumnId()
    {
        var table = new DbTable(
            "dbo",
            "Orders",
            new[]
            {
                new DbColumn(
                    2,
                    "Status",
                    new SqlType("nvarchar", 20),
                    false,
                    false,
                    "'Pending'"),
                new DbColumn(
                    1,
                    "Id",
                    new SqlType("int"),
                    false,
                    true,
                    null)
            },
            new DbPrimaryKey("PK_Orders", new[] { "Id" }, true));

        var schema = new DatabaseSchema("SqlServer", new[] { table }, "1.0");
        var serializer = new SchemaSnapshotSerializer();

        var json = serializer.Serialize(schema);

        var idIndex = json.IndexOf(@"""name"": ""Id""", StringComparison.Ordinal);
        var statusIndex = json.IndexOf(@"""name"": ""Status""", StringComparison.Ordinal);

        Assert.True(idIndex >= 0);
        Assert.True(statusIndex >= 0);
        Assert.True(idIndex < statusIndex);
    }

    [Fact]
    public void Deserialize_ShouldThrow_WhenDatabaseEngineIsUnsupported()
    {
        var json = """
        {
          "formatVersion": "1.0",
          "databaseEngine": "PostgreSql",
          "tables": []
        }
        """;

        var serializer = new SchemaSnapshotSerializer();

        Assert.Throws<NotSupportedException>(() => serializer.Deserialize(json));
    }

    [Fact]
    public void Deserialize_ShouldThrow_WhenFormatVersionIsMissing()
    {
        var json = """
        {
          "formatVersion": "",
          "databaseEngine": "SqlServer",
          "tables": []
        }
        """;

        var serializer = new SchemaSnapshotSerializer();

        Assert.Throws<NotSupportedException>(() => serializer.Deserialize(json));
    }

    [Fact]
    public void Serialize_ThenDeserialize_ShouldPreserveKeyData()
    {
        var original = BuildSchema();
        var serializer = new SchemaSnapshotSerializer();

        var json = serializer.Serialize(original);
        var restored = serializer.Deserialize(json);

        Assert.Equal(original.Provider, restored.Provider);
        Assert.Equal(original.FormatVersion, restored.FormatVersion);
        Assert.Equal(original.Tables.Count, restored.Tables.Count);

        var originalTable = original.Tables.Single(t => t.Schema == "dbo" && t.Name == "Orders");
        var restoredTable = restored.Tables.Single(t => t.Schema == "dbo" && t.Name == "Orders");

        Assert.Equal(originalTable.Schema, restoredTable.Schema);
        Assert.Equal(originalTable.Name, restoredTable.Name);
        Assert.Equal(originalTable.Columns.Count, restoredTable.Columns.Count);

        var originalId = originalTable.Columns.Single(c => c.Name == "Id");
        var restoredId = restoredTable.Columns.Single(c => c.Name == "Id");

        Assert.Equal(originalId.ColumnId, restoredId.ColumnId);
        Assert.Equal(originalId.Name, restoredId.Name);
        Assert.Equal(originalId.Type.StoreType, restoredId.Type.StoreType);
        Assert.Equal(originalId.IsNullable, restoredId.IsNullable);
        Assert.Equal(originalId.IsIdentity, restoredId.IsIdentity);
        Assert.Equal(originalId.DefaultSql, restoredId.DefaultSql);

        var originalStatus = originalTable.Columns.Single(c => c.Name == "Status");
        var restoredStatus = restoredTable.Columns.Single(c => c.Name == "Status");

        Assert.Equal(originalStatus.ColumnId, restoredStatus.ColumnId);
        Assert.Equal(originalStatus.Name, restoredStatus.Name);
        Assert.Equal(originalStatus.Type.StoreType, restoredStatus.Type.StoreType);
        Assert.Equal(originalStatus.Type.Length, restoredStatus.Type.Length);
        Assert.Equal(originalStatus.Type.Precision, restoredStatus.Type.Precision);
        Assert.Equal(originalStatus.Type.Scale, restoredStatus.Type.Scale);
        Assert.Equal(originalStatus.IsNullable, restoredStatus.IsNullable);
        Assert.Equal(originalStatus.IsIdentity, restoredStatus.IsIdentity);
        Assert.Equal(originalStatus.DefaultSql, restoredStatus.DefaultSql);

        Assert.NotNull(restoredTable.PrimaryKey);
        Assert.Equal(originalTable.PrimaryKey!.Name, restoredTable.PrimaryKey!.Name);
        Assert.Equal(originalTable.PrimaryKey.Columns, restoredTable.PrimaryKey.Columns);
        Assert.Equal(originalTable.PrimaryKey.IsClustered, restoredTable.PrimaryKey.IsClustered);
    }

    private static DatabaseSchema BuildSchema()
    {
        return new DatabaseSchema(
            "SqlServer",
            new[]
            {
                new DbTable(
                    "dbo",
                    "Orders",
                    new[]
                    {
                        new DbColumn(
                            1,
                            "Id",
                            new SqlType("int"),
                            false,
                            true,
                            null),
                        new DbColumn(
                            2,
                            "Status",
                            new SqlType("nvarchar", 20),
                            false,
                            false,
                            "'Pending'")
                    },
                    new DbPrimaryKey("PK_Orders", new[] { "Id" }, true))
            },
            "1.0");
    }
}