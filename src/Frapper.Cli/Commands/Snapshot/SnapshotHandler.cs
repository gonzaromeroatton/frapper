using Frapper.Cli.Configuration;
using Frapper.Core.Serialization;
using Frapper.SqlServer.Introspection;

namespace Frapper.Cli.Commands.Snapshot;

internal sealed class SnapshotHandler
{
    private readonly FrapperConfiguration _configuration;

    public SnapshotHandler(FrapperConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<int> RunAsync(
        string? rawConnection,
        string outPath,
        string baseOutPath,
        CancellationToken ct)
    {
        var connectionString = _configuration.ResolveConnectionString(rawConnection);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            Console.Error.WriteLine("Error: no se pudo resolver --connection.");
            return 2;
        }

        var reader = new SqlServerSchemaReader();
        var schema = await reader.ReadAsync(connectionString, ct);

        var json = SchemaSnapshotSerializer.Serialize(schema);

        EnsureDirectory(outPath);
        EnsureDirectory(baseOutPath);

        await File.WriteAllTextAsync(outPath, json, ct);
        await File.WriteAllTextAsync(baseOutPath, json, ct);

        Console.WriteLine($"Snapshot generado: {outPath}");
        Console.WriteLine($"Snapshot base generado: {baseOutPath}");

        return 0;
    }

    private static void EnsureDirectory(string path)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(dir))
            Directory.CreateDirectory(dir);
    }
}