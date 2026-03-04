namespace Frapper.Core.Domain.Schema;

/// <summary>
/// Represents a SQL data type, including its store type and optional parameters such as length, precision, and scale.
/// </summary>
/// <param name="StoreType"></param>
/// <param name="Length"></param>
/// <param name="Precision"></param>
/// <param name="Scale"></param>
public sealed record SqlType
(
    string StoreType,
    int? Length = null,
    byte? Precision = null,
    byte? Scale = null
);