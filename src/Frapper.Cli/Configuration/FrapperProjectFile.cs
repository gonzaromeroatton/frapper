namespace Frapper.Cli.Configuration;

internal sealed class FrapperProjectFile
{
    public string Provider { get; init; } = "SqlServer";
    public string Connection { get; init; } = "Default";
    public string MigrationsPath { get; init; } = "migrations";
    public string BaseSnapshot { get; init; } = "schema.snapshot.base.json";
    public string Snapshot { get; init; } = "schema.snapshot.json";
}