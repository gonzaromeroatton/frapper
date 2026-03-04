using System.Text.Encodings.Web;
using System.Text.Json;
using Frapper.Core.Domain.Schema;

namespace Frapper.Core.Serialization;

/// <summary>
/// Provides methods for serializing and deserializing database schema snapshots to and from JSON format.
/// </summary>
public static class SchemaSnapshotSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static string Serialize(DatabaseSchema schema)
        => JsonSerializer.Serialize(schema, Options);

    public static DatabaseSchema Deserialize(string json)
        => JsonSerializer.Deserialize<DatabaseSchema>(json, Options)
           ?? throw new InvalidOperationException("Invalid schema snapshot JSON.");
}