using Frapper.Cli.Configuration;
using Frapper.Core.Abstractions;

namespace Frapper.Cli.Commands.Migrate;

internal sealed class MigrateApplyHandler
{
    private readonly FrapperConfiguration _configuration;
    private readonly IMigrationRunner _migrationRunner;

    public MigrateApplyHandler(
        FrapperConfiguration configuration,
        IMigrationRunner migrationRunner)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _migrationRunner = migrationRunner ?? throw new ArgumentNullException(nameof(migrationRunner));
    }

    public async Task<int> RunAsync(
        string rawConnection,
        string migrationsDirectory,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(rawConnection))
        {
            Console.Error.WriteLine("Error: --connection es requerido.");
            return 2;
        }

        if (string.IsNullOrWhiteSpace(migrationsDirectory))
        {
            Console.Error.WriteLine("Error: --dir es requerido.");
            return 2;
        }

        if (!Directory.Exists(migrationsDirectory))
        {
            Console.Error.WriteLine($"Error: no existe el directorio de migraciones '{migrationsDirectory}'.");
            return 2;
        }

        try
        {
            var connectionString = _configuration.RequireConnectionString(rawConnection);

            await _migrationRunner.EnsureHistoryTableExistsAsync(connectionString, cancellationToken);

            var appliedMigrationIds = await _migrationRunner.GetAppliedMigrationIdsAsync(connectionString, cancellationToken);
            var appliedMigrationSet = appliedMigrationIds.ToHashSet(StringComparer.OrdinalIgnoreCase);

            var migrationFiles = Directory
                .GetFiles(migrationsDirectory, "*.sql", SearchOption.TopDirectoryOnly)
                .OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (migrationFiles.Length == 0)
            {
                Console.WriteLine("No se encontraron migraciones SQL para aplicar.");
                return 0;
            }

            var pendingFiles = migrationFiles
                .Where(file => !appliedMigrationSet.Contains(Path.GetFileName(file)))
                .ToArray();

            if (pendingFiles.Length == 0)
            {
                Console.WriteLine("No hay migraciones pendientes.");
                return 0;
            }

            foreach (var file in pendingFiles)
            {
                var migrationId = Path.GetFileName(file);
                var sql = await File.ReadAllTextAsync(file, cancellationToken);

                Console.WriteLine($"Aplicando migración: {migrationId}");

                await _migrationRunner.ApplyMigrationAsync(
                    connectionString,
                    migrationId,
                    sql,
                    cancellationToken);
            }

            Console.WriteLine($"Migraciones aplicadas correctamente: {pendingFiles.Length}");

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Error: migrate apply failed.");
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }
}