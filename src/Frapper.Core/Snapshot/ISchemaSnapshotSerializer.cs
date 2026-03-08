using Frapper.Core.Domain.Schema;

namespace Frapper.Core.Snapshot;

public interface ISchemaSnapshotSerializer
{
    string Serialize(DatabaseSchema schema);
    DatabaseSchema Deserialize(string json);
}