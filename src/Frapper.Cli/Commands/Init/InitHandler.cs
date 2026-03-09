using System.Text.Json;
using Frapper.Cli.Commands.Snapshot;
using Frapper.Cli.Configuration;

namespace Frapper.Cli.Commands.Init;

internal sealed class InitHandler
{
    private const string ConfigFileName = "frapper.config.json";

    private readonly FrapperConfiguration _configuration;
    private readonly SnapshotHandler _snapshotHandler;

    public InitHandler(
        FrapperConfiguration configuration,
        SnapshotHandler snapshotHandler)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _snapshotHandler = snapshotHandler ?? throw new ArgumentNullException(nameof(snapshotHandler));
    }

    public async Task<int> RunAsync(
        string rawConnection,
        string migrationsPath,
        string baseSnapshotPath,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(rawConnection))
        {
            Console.Error.WriteLine("Error: --connection es requerido.");
            return 2;
        }

        if (string.IsNullOrWhiteSpace(migrationsPath))
        {
            Console.Error.WriteLine("Error: --migrations-path es requerido.");
            return 2;
        }

        if (string.IsNullOrWhiteSpace(baseSnapshotPath))
        {
            Console.Error.WriteLine("Error: --base-snapshot es requerido.");
            return 2;
        }

        try
        {
            if (File.Exists(ConfigFileName))
            {
                Console.Error.WriteLine($"Error: ya existe '{ConfigFileName}' en el directorio actual.");
                return 2;
            }

            // Valida que la conexión pueda resolverse antes de crear nada.
            _configuration.RequireConnectionString(rawConnection);

            Directory.CreateDirectory(migrationsPath);

            await WriteConfigFileAsync(
                rawConnection,
                migrationsPath,
                baseSnapshotPath,
                cancellationToken);

            var snapshotResult = await _snapshotHandler.RunAsync(
                rawConnection,
                baseSnapshotPath,
                baseSnapshotPath,
                cancellationToken);

            if (snapshotResult != 0)
            {
                Console.Error.WriteLine("Error: no fue posible generar el snapshot base durante la inicialización.");
                return snapshotResult;
            }

            await EnsureMigrationsReadmeAsync(migrationsPath, cancellationToken);

            Console.WriteLine("Proyecto Frapper inicializado correctamente.");
            Console.WriteLine($"Config: {Path.GetFullPath(ConfigFileName)}");
            Console.WriteLine($"Base snapshot: {Path.GetFullPath(baseSnapshotPath)}");
            Console.WriteLine($"Migrations: {Path.GetFullPath(migrationsPath)}");

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Error: initialization failed.");
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }

    private static async Task WriteConfigFileAsync(
        string rawConnection,
        string migrationsPath,
        string baseSnapshotPath,
        CancellationToken cancellationToken)
    {
        var config = new FrapperProjectConfig
        {
            Provider = "SqlServer",
            Connection = rawConnection,
            MigrationsPath = migrationsPath,
            BaseSnapshot = baseSnapshotPath
        };

        var json = JsonSerializer.Serialize(config, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        await File.WriteAllTextAsync(ConfigFileName, json, cancellationToken);
    }

    private static async Task EnsureMigrationsReadmeAsync(
        string migrationsPath,
        CancellationToken cancellationToken)
    {
        var readmePath = Path.Combine(migrationsPath, "README.md");

        if (File.Exists(readmePath))
        {
            return;
        }

        var content =
            "# Migrations\r\n\r\n" +
            "Esta carpeta contiene las migraciones SQL generadas por Frapper.\r\n";

        await File.WriteAllTextAsync(readmePath, content, cancellationToken);
    }

    private sealed class FrapperProjectConfig
    {
        public string Provider { get; init; } = "SqlServer";
        public string Connection { get; init; } = string.Empty;
        public string MigrationsPath { get; init; } = "migrations";
        public string BaseSnapshot { get; init; } = "schema.snapshot.base.json";
    }
}