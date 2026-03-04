namespace Frapper.Core.Domain.Diff;

/// <summary>
/// Represents the options for database schema comparison.
/// </summary>
/// <param name="AllowDestructiveChanges"></param>
/// <param name="StrictTypeMatching"></param>
public sealed record DiffOptions(
    bool AllowDestructiveChanges = false,
    bool StrictTypeMatching = true
);