using Frapper.Cli.Configuration;
using Frapper.Core.Abstractions;
using Frapper.Core.Domain.Diff;
using Frapper.Core.Domain.Plan;
using Frapper.Core.Domain.Schema;
using Frapper.Core.Snapshot;

namespace Frapper.Cli.Commands.Diff;

internal sealed class DiffHandler
{
    private readonly FrapperConfiguration _configuration;
    private readonly IDatabaseSchemaReader _schemaReader;
    private readonly ISchemaSnapshotSerializer _serializer;
    private readonly ISchemaDiffer _differ;

    public DiffHandler(
        FrapperConfiguration configuration,
        IDatabaseSchemaReader schemaReader,
        ISchemaSnapshotSerializer serializer)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _schemaReader = schemaReader ?? throw new ArgumentNullException(nameof(schemaReader));
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        _differ = new SchemaDiffer();
    }

    public async Task<int> RunAsync(
        string basePath,
        string? targetPath,
        string? rawConnection,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(basePath))
        {
            await Console.Error.WriteLineAsync("Base snapshot path is required.");
            return 1;
        }

        var hasTarget = !string.IsNullOrWhiteSpace(targetPath);
        var hasConnection = !string.IsNullOrWhiteSpace(rawConnection);

        if (hasTarget == hasConnection)
        {
            await Console.Error.WriteLineAsync("Debe especificarse exactamente uno de estos parámetros: --target o --connection.");
            return 2;
        }

        try
        {
            if (!File.Exists(basePath))
            {
                await Console.Error.WriteLineAsync($"Base snapshot file not found: {basePath}");
                return 1;
            }

            var baseJson = await File.ReadAllTextAsync(basePath, cancellationToken);
            var baseSchema = _serializer.Deserialize(baseJson);

            MigrationPlan plan;

            if (hasTarget)
            {
                if (!File.Exists(targetPath!))
                {
                    await Console.Error.WriteLineAsync($"Target snapshot file not found: {targetPath}");
                    return 1;
                }

                var targetJson = await File.ReadAllTextAsync(targetPath!, cancellationToken);
                var targetSchema = _serializer.Deserialize(targetJson);

                // snapshot base -> snapshot target
                plan = _differ.Diff(
                    baseSchema,
                    targetSchema,
                    new DiffOptions(
                        AllowDestructiveChanges: true,
                        StrictTypeMatching: true));
            }
            else
            {
                var connectionString = _configuration.RequireConnectionString(rawConnection);
                var liveDatabaseSchema = await _schemaReader.ReadAsync(connectionString, cancellationToken);

                // DB real -> snapshot base
                plan = _differ.Diff(
                    liveDatabaseSchema,
                    baseSchema,
                    new DiffOptions(
                        AllowDestructiveChanges: true,
                        StrictTypeMatching: true));
            }

            var summary = DiffSummaryMapper.FromPlan(plan);

            PrintSummary(summary);

            return 0;
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync("Diff operation failed.");
            await Console.Error.WriteLineAsync(ex.Message);
            return 1;
        }
    }

    private static void PrintSummary(DiffSummary summary)
    {
        Console.WriteLine("Diff completed successfully.");
        Console.WriteLine($"Created tables: {summary.CreatedTables}");
        Console.WriteLine($"Dropped tables: {summary.DroppedTables}");
        Console.WriteLine($"Added columns: {summary.AddedColumns}");
        Console.WriteLine($"Dropped columns: {summary.DroppedColumns}");
        Console.WriteLine($"Altered columns: {summary.AlteredColumns}");
        Console.WriteLine($"Total operations: {summary.TotalOperations}");
    }
}