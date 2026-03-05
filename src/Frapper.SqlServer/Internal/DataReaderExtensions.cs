using System.Data.Common;

namespace Frapper.SqlServer.Internal;

/// <summary>
/// Provides extension methods for DbDataReader to simplify retrieving values by column name, including handling of nullable values.
/// </summary>
internal static class DataReaderExtensions
{
    public static string GetString(this DbDataReader r, string name)
        => r.GetString(r.GetOrdinal(name));

    public static int GetInt32(this DbDataReader r, string name)
        => r.GetInt32(r.GetOrdinal(name));

    public static short GetInt16(this DbDataReader r, string name)
        => r.GetInt16(r.GetOrdinal(name));

    public static byte GetByte(this DbDataReader r, string name)
        => r.GetByte(r.GetOrdinal(name));

    public static bool GetBoolean(this DbDataReader r, string name)
        => r.GetBoolean(r.GetOrdinal(name));

    public static string? GetNullableString(this DbDataReader r, string name)
    {
        var o = r.GetOrdinal(name);
        return r.IsDBNull(o) ? null : r.GetString(o);
    }
}