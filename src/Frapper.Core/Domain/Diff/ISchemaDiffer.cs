using Frapper.Core.Domain.Plan;
using Frapper.Core.Domain.Schema;

namespace Frapper.Core.Domain.Diff;

/// <summary>
/// Defines an interface for comparing two database schemas and generating a migration plan that describes the necessary changes to transform the old schema into the new schema, based on specified options.
/// </summary>
public interface ISchemaDiffer
{
    MigrationPlan Diff(DatabaseSchema oldSchema, DatabaseSchema newSchema, DiffOptions options);
}