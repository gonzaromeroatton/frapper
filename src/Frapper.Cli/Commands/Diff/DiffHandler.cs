using Frapper.Core.Domain.Diff;
using Frapper.Core.Snapshot;

namespace Frapper.Cli.Commands.Diff;

internal sealed class DiffHandler
{
    private readonly ISchemaSnapshotSerializer _serializer;
    private readonly ISchemaDiffer _differ;

    public DiffHandler()
    {
        _serializer = new SchemaSnapshotSerializer();
        _differ = new SchemaDiffer();
    }

    public async Task<int> RunAsync(
        string basePath,
        string targetPath,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(basePath))
        {
            Console.Error.WriteLine("Base snapshot path is required.");
            return 1;
        }

        if (string.IsNullOrWhiteSpace(targetPath))
        {
            Console.Error.WriteLine("Target snapshot path is required.");
            return 1;
        }

        try
        {
            if (!File.Exists(basePath))
            {
                Console.Error.WriteLine($"Base snapshot file not found: {basePath}");
                return 1;
            }

            if (!File.Exists(targetPath))
            {
                Console.Error.WriteLine($"Target snapshot file not found: {targetPath}");
                return 1;
            }

            var baseJson = await File.ReadAllTextAsync(basePath, cancellationToken);
            var targetJson = await File.ReadAllTextAsync(targetPath, cancellationToken);

            var baseSchema = _serializer.Deserialize(baseJson);
            var targetSchema = _serializer.Deserialize(targetJson);

            var plan = _differ.Diff(
                baseSchema,
                targetSchema,
                new DiffOptions(AllowDestructiveChanges: true));

            var summary = DiffSummaryMapper.FromPlan(plan);

            PrintSummary(summary);

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Diff operation failed.");
            Console.Error.WriteLine(ex.Message);
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