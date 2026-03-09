using Frapper.Cli.Commands.Diff;
using Frapper.Cli.Configuration;
using Frapper.Core.Abstractions;
using Frapper.Core.Domain.Schema;
using Frapper.Core.Snapshot;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Frapper.Cli.Tests.Commands.Diff;

public sealed class DiffHandlerTests
{
    [Fact]
    public async Task RunAsync_ShouldReturnOne_WhenBaseSnapshotDoesNotExist()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>());
        var schemaReader = new FakeDatabaseSchemaReader(BuildSchema());
        var serializer = new SchemaSnapshotSerializer();

        var handler = new DiffHandler(configuration, schemaReader, serializer);

        var exitCode = await handler.RunAsync(
            "missing-base.json",
            "missing-target.json",
            null,
            CancellationToken.None);

        Assert.Equal(1, exitCode);
    }

    [Fact]
    public async Task RunAsync_ShouldDiffBaseSnapshotAgainstLiveDatabase_WhenConnectionIsProvided()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);

        var originalOut = Console.Out;
        var originalError = Console.Error;

        await using var outWriter = new StringWriter();
        await using var errorWriter = new StringWriter();

        Console.SetOut(outWriter);
        Console.SetError(errorWriter);

        try
        {
            var basePath = Path.Combine(tempDirectory, "schema.snapshot.base.json");

            var baseSchema = new DatabaseSchema(
                "SqlServer",
                new[]
                {
                new DbTable(
                    "dbo",
                    "Orders",
                    new[]
                    {
                        new DbColumn(1, "Id", new SqlType("int"), false, true, null),
                        new DbColumn(2, "Status", new SqlType("nvarchar", 20), false, false, "'Pending'")
                    },
                    new DbPrimaryKey("PK_Orders", new[] { "Id" }, true))
                },
                "1.0");

            var liveSchema = new DatabaseSchema(
                "SqlServer",
                new[]
                {
                new DbTable(
                    "dbo",
                    "Orders",
                    new[]
                    {
                        new DbColumn(1, "Id", new SqlType("int"), false, true, null),
                        new DbColumn(2, "Status", new SqlType("nvarchar", 20), false, false, "'Pending'"),
                        new DbColumn(3, "CreatedAt", new SqlType("datetime2"), false, false, "GETUTCDATE()")
                    },
                    new DbPrimaryKey("PK_Orders", new[] { "Id" }, true))
                },
                "1.0");

            var serializer = new SchemaSnapshotSerializer();
            var baseJson = serializer.Serialize(baseSchema);
            await File.WriteAllTextAsync(basePath, baseJson);

            var configuration = BuildConfiguration(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = "Server=test;Database=Frapper;Trusted_Connection=True;"
            });

            var schemaReader = new FakeDatabaseSchemaReader(liveSchema);
            var handler = new DiffHandler(configuration, schemaReader, serializer);

            var exitCode = await handler.RunAsync(
                basePath,
                targetPath: null,
                rawConnection: "Default",
                cancellationToken: CancellationToken.None);

            var stdOut = outWriter.ToString();
            var stdErr = errorWriter.ToString();

            Assert.Equal(0, exitCode);
            Assert.True(string.IsNullOrWhiteSpace(stdErr), $"Expected no stderr output, but got: {stdErr}");
            Assert.Contains("Diff completed successfully.", stdOut, StringComparison.Ordinal);
            Assert.Contains("Added columns: 1", stdOut, StringComparison.Ordinal);
            Assert.Contains("Total operations: 1", stdOut, StringComparison.Ordinal);
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetError(originalError);

            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }
    }

    [Fact]
    public async Task RunAsync_ShouldReturnTwo_WhenTargetAndConnectionAreBothProvided()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);

        var originalOut = Console.Out;
        var originalError = Console.Error;

        try
        {
            var basePath = Path.Combine(tempDirectory, "schema.snapshot.base.json");
            var targetPath = Path.Combine(tempDirectory, "schema.snapshot.json");

            var serializer = new SchemaSnapshotSerializer();
            var schema = BuildSchema();

            // Setup SIN capturar consola
            await File.WriteAllTextAsync(basePath, serializer.Serialize(schema));
            await File.WriteAllTextAsync(targetPath, serializer.Serialize(schema));

            var configuration = BuildConfiguration(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = "Server=test;Database=Frapper;Trusted_Connection=True;"
            });

            var schemaReader = new FakeDatabaseSchemaReader(schema);
            var handler = new DiffHandler(configuration, schemaReader, serializer);

            await using var outWriter = new StringWriter();
            await using var errorWriter = new StringWriter();

            outWriter.GetStringBuilder().Clear();
            errorWriter.GetStringBuilder().Clear();

            Console.SetOut(outWriter);
            Console.SetError(errorWriter);

            var exitCode = await handler.RunAsync(
                basePath,
                targetPath: targetPath,
                rawConnection: "Default",
                cancellationToken: CancellationToken.None);

            var stdOut = outWriter.ToString();
            var stdErr = errorWriter.ToString();

            Assert.Equal(2, exitCode);
            Assert.True(string.IsNullOrWhiteSpace(stdOut), $"Expected no stdout output, but got: {stdOut}");
            Assert.Contains(
                "Debe especificarse exactamente uno de estos parámetros: --target o --connection.",
                stdErr,
                StringComparison.Ordinal);
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetError(originalError);

            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }
    }

    [Fact]
    public async Task RunAsync_ShouldReturnTwo_WhenNeitherTargetNorConnectionIsProvided()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);

        var originalOut = Console.Out;
        var originalError = Console.Error;

        await using var outWriter = new StringWriter();
        await using var errorWriter = new StringWriter();

        Console.SetOut(outWriter);
        Console.SetError(errorWriter);

        try
        {
            var basePath = Path.Combine(tempDirectory, "schema.snapshot.base.json");

            var serializer = new SchemaSnapshotSerializer();
            var schema = BuildSchema();

            await File.WriteAllTextAsync(basePath, serializer.Serialize(schema));

            var configuration = BuildConfiguration(new Dictionary<string, string?>());
            var schemaReader = new FakeDatabaseSchemaReader(schema);
            var handler = new DiffHandler(configuration, schemaReader, serializer);

            var exitCode = await handler.RunAsync(
                basePath,
                targetPath: null,
                rawConnection: null,
                cancellationToken: CancellationToken.None);

            var stdOut = outWriter.ToString();
            var stdErr = errorWriter.ToString();

            Assert.Equal(2, exitCode);
            Assert.True(string.IsNullOrWhiteSpace(stdOut), $"Expected no stdout output, but got: {stdOut}");
            Assert.Contains(
                "Debe especificarse exactamente uno de estos parámetros: --target o --connection.",
                stdErr,
                StringComparison.Ordinal);
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetError(originalError);

            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }
    }

    private static FrapperConfiguration BuildConfiguration(Dictionary<string, string?> values)
    {
        var configurationRoot = new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();

        return new FrapperConfiguration(configurationRoot);
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
                        new DbColumn(1, "Id", new SqlType("int"), false, true, null),
                        new DbColumn(2, "Status", new SqlType("nvarchar", 20), false, false, "'Pending'")
                    },
                    new DbPrimaryKey("PK_Orders", new[] { "Id" }, true))
            },
            "1.0");
    }

    private sealed class FakeDatabaseSchemaReader : IDatabaseSchemaReader
    {
        private readonly DatabaseSchema _schema;

        public FakeDatabaseSchemaReader(DatabaseSchema schema)
        {
            _schema = schema;
        }

        public Task<DatabaseSchema> ReadAsync(string connectionString, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_schema);
        }
    }
}