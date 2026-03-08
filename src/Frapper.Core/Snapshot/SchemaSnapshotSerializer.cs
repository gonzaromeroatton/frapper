using System.Text.Json;
using System.Text.Json.Serialization;
using Frapper.Core.Domain.Schema;

namespace Frapper.Core.Snapshot;

public sealed class SchemaSnapshotSerializer : ISchemaSnapshotSerializer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never
    };

    public string Serialize(DatabaseSchema schema)
    {
        ArgumentNullException.ThrowIfNull(schema);

        var document = SchemaSnapshotMapper.ToDocument(schema);
        return JsonSerializer.Serialize(document, JsonOptions);
    }

    public DatabaseSchema Deserialize(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        var document = JsonSerializer.Deserialize<SchemaSnapshotDocument>(json, JsonOptions)
            ?? throw new InvalidOperationException("Snapshot JSON could not be deserialized.");

        ValidateDocument(document);

        return SchemaSnapshotMapper.ToDomain(document);
    }

    private static void ValidateDocument(SchemaSnapshotDocument document)
    {
        if (string.IsNullOrWhiteSpace(document.FormatVersion))
        {
            throw new NotSupportedException("Snapshot format version is required.");
        }

        if (!string.Equals(document.FormatVersion, "1.0", StringComparison.Ordinal))
        {
            throw new NotSupportedException(
                $"Unsupported snapshot format version '{document.FormatVersion}'.");
        }

        if (!string.Equals(document.DatabaseEngine, "SqlServer", StringComparison.OrdinalIgnoreCase))
        {
            throw new NotSupportedException(
                $"Unsupported database engine '{document.DatabaseEngine}'.");
        }
    }
}