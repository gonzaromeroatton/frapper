using Frapper.Cli.Configuration;
using Frapper.Core.Domain.Diff;
using Frapper.Core.Serialization;
using Frapper.EFMigrationEmitter;

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
        string snapshotPath,
        string baseSnapshotPath,
        string outDir,
        bool allowDestructiveChanges,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(snapshotPath))
        {
            Console.Error.WriteLine($"Error: no se encontró el snapshot deseado en '{snapshotPath}'.");
            return 2;
        }

        if (!File.Exists(baseSnapshotPath))
        {
            Console.Error.WriteLine($"Error: no se encontró el snapshot base en '{baseSnapshotPath}'.");
            return 2;
        }

        var desiredJson = await File.ReadAllTextAsync(snapshotPath, cancellationToken);
        var desiredSchema = SchemaSnapshotSerializer.Deserialize(desiredJson);

        var baseJson = await File.ReadAllTextAsync(baseSnapshotPath, cancellationToken);
        var baseSchema = SchemaSnapshotSerializer.Deserialize(baseJson);

        var differ = new SchemaDiffer();
        var plan = differ.Diff(
            baseSchema,
            desiredSchema,
            new DiffOptions(
                AllowDestructiveChanges: allowDestructiveChanges,
                StrictTypeMatching: true));

        if (!plan.Up.Any())
        {
            Console.WriteLine("No se detectaron cambios entre snapshot base y snapshot deseado.");
            return 0;
        }

        Directory.CreateDirectory(outDir);

        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var safeName = SanitizeName(migrationName);
        var fileName = $"{timestamp}_{safeName}.sql";
        var outputPath = Path.Combine(outDir, fileName);

        var sql = SqlMigrationEmitter.Emit(plan);
        await File.WriteAllTextAsync(outputPath, sql, cancellationToken);

        // El nuevo estado deseado pasa a ser la nueva base
        await File.WriteAllTextAsync(baseSnapshotPath, desiredJson, cancellationToken);

        Console.WriteLine($"Migración generada: {outputPath}");
        Console.WriteLine($"Snapshot base actualizado: {baseSnapshotPath}");

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