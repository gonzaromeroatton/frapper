using Frapper.Core.Domain.Schema;

namespace Frapper.SqlServer.Normalization;

/// <summary>
/// Provides functionality to normalize SQL Server data types by converting raw type information (type name, max length, precision, scale) into a standardized SqlType representation.
/// </summary>
internal static class SqlServerTypeNormalizer
{
    /// <summary>
    /// Normalizes SQL Server data type information into a standardized SqlType representation. It takes the raw type name, maximum length, precision, and scale as input and returns a SqlType object with the appropriate properties set based on the type of data.
    /// </summary>
    /// <param name="typeName"></param>
    /// <param name="maxLength"></param>
    /// <param name="precision"></param>
    /// <param name="scale"></param>
    /// <returns></returns>
    public static SqlType Normalize(string typeName, short maxLength, byte precision, byte scale)
    {
        // max_length: bytes, -1 significa (max) en (n)varchar/varbinary
        return typeName switch
        {
            "nvarchar" or "nchar" => new SqlType(
                StoreType: typeName,
                Length: maxLength < 0 ? null : maxLength / 2),

            "varchar" or "char" or "varbinary" or "binary" => new SqlType(
                StoreType: typeName,
                Length: maxLength < 0 ? null : maxLength),

            "decimal" or "numeric" => new SqlType(
                StoreType: typeName,
                Precision: precision,
                Scale: scale),

            // datetime2/time/datetimeoffset: precision es fractional seconds precision
            "datetime2" or "datetimeoffset" or "time" => new SqlType(
                StoreType: typeName,
                Precision: precision),

            _ => new SqlType(StoreType: typeName)
        };
    }
}