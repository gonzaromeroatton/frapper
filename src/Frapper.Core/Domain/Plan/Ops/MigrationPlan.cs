namespace Frapper.Core.Domain.Plan;

/// <summary>
/// Represents a migration plan, including the operations to be applied in the "up" direction and the "down" direction.
/// </summary>
/// <param name="Up"></param>
/// <param name="Down"></param>
public sealed record MigrationPlan(
    IReadOnlyList<IMigrationOp> Up,
    IReadOnlyList<IMigrationOp> Down
);