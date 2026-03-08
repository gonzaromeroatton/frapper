using Frapper.Core.Domain.Schema;
using Frapper.Core.Snapshot;

namespace Frapper.Cli.Commands.Diff;

internal sealed class DiffHandler
{
    private readonly ISchemaSnapshotSerializer _serializer;

    public DiffHandler()
    {
        _serializer = new SchemaSnapshotSerializer();
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

            var diffResult = ComputeDiff(baseSchema, targetSchema);

            PrintSummary(diffResult);

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Diff operation failed.");
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }

    private static object ComputeDiff(DatabaseSchema baseSchema, DatabaseSchema targetSchema)
    {
        // TODO:
        // Reemplazar esto por tu SchemaDiffEngine real.
        // Ejemplo esperado:
        // return SchemaDiffEngine.Compare(baseSchema, targetSchema);

        throw new NotImplementedException(
            "Connect DiffHandler to the real schema diff engine.");
    }

    private static void PrintSummary(object diffResult)
    {
        // TODO:
        // Reemplazar por impresión real del resultado del diff.
        Console.WriteLine("Diff completed successfully.");
        Console.WriteLine("Connect PrintSummary() to the real diff result type.");
    }
}