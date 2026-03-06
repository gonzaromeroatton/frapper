using Frapper.Cli.Configuration;
using Frapper.Core.Domain.Diff;
using Frapper.Core.Serialization;
using Frapper.EFMigrationEmitter;
using Frapper.SqlServer.Introspection;

namespace Frapper.Cli.Commands.Migrate;

internal sealed class MigrateAddHandler
{
    private readonly FrapperConfiguration _configuration;

    public MigrateAddHandler(FrapperConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<int> RunAsync(
        string migrationName,
        string? rawConnection,
        string snapshotPath,
        string outDir,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(snapshotPath))
        {
            Console.Error.WriteLine($"Error: no se encontró el snapshot base en '{snapshotPath}'.");
            return 2;
        }

        var connectionString = _configuration.ResolveConnectionString(rawConnection);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            Console.Error.WriteLine("Error: no se pudo resolver --connection.");
            return 2;
        }

        var oldJson = await File.ReadAllTextAsync(snapshotPath, cancellationToken);
        var oldSchema = SchemaSnapshotSerializer.Deserialize(oldJson);

        var reader = new SqlServerSchemaReader();
        var newSchema = await reader.ReadAsync(connectionString, cancellationToken);

        var differ = new SchemaDiffer();
        var plan = differ.Diff(
            oldSchema,
            newSchema,
            new DiffOptions(AllowDestructiveChanges: false, StrictTypeMatching: true));

        if (!plan.Up.Any())
        {
            Console.WriteLine("No se detectaron cambios en el esquema.");
            return 0;
        }

        Directory.CreateDirectory(outDir);

        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var safeName = SanitizeName(migrationName);
        var fileName = $"{timestamp}_{safeName}.sql";
        var outputPath = Path.Combine(outDir, fileName);

        var sql = SqlMigrationEmitter.Emit(plan);

        await File.WriteAllTextAsync(outputPath, sql, cancellationToken);

        Console.WriteLine($"Migración generada: {outputPath}");

        return 0;
    }

    private static string SanitizeName(string name)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new string(name.Where(c => !invalidChars.Contains(c)).ToArray());

        return string.IsNullOrWhiteSpace(sanitized)
            ? "Migration"
            : sanitized.Replace(' ', '_');
    }
}