using Frapper.Core.Domain.Schema;

namespace Frapper.SqlServer.Normalization;

internal static class SqlServerTypeNormalizer
{
    public static SqlType Normalize(string typeName, short maxLength, byte precision, byte scale)
    {
        // SQL Server stores max_length in bytes; nvarchar/nchar are 2 bytes per char.
        return typeName switch
        {
            "nvarchar" or "nchar" => new SqlType(typeName, Length: maxLength < 0 ? null : maxLength / 2),
            "varchar" or "char" or "varbinary" or "binary" => new SqlType(typeName, Length: maxLength < 0 ? null : maxLength),
            "decimal" or "numeric" => new SqlType(typeName, Precision: precision, Scale: scale),
            "datetime2" or "datetimeoffset" or "time" => new SqlType(typeName, Precision: precision), // precision = fractional seconds precision
            _ => new SqlType(typeName)
        };
    }
}