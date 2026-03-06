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

    public async Task<int> RunAsync(string? rawConnection, string outPath, CancellationToken ct)
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

        var dir = Path.GetDirectoryName(outPath);
        if (!string.IsNullOrWhiteSpace(dir))
            Directory.CreateDirectory(dir);

        await File.WriteAllTextAsync(outPath, json, ct);

        Console.WriteLine($"Snapshot generado: {outPath}");
        return 0;
    }
}