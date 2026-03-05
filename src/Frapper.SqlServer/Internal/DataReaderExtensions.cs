using System.Data.Common;

namespace Frapper.SqlServer.Internal;

internal static class DataReaderExtensions
{
    public static string? GetStringSafe(this DbDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);

        return reader.IsDBNull(ordinal)
            ? null
            : reader.GetString(ordinal);
    }
}
