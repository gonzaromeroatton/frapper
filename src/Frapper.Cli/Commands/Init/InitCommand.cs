using System.CommandLine;
using Frapper.Cli.Configuration;
using Frapper.Core.Snapshot;
using Frapper.SqlServer;
using Frapper.Cli.Commands.Snapshot;
using Frapper.SqlServer.Introspection;

namespace Frapper.Cli.Commands.Init;

internal static class InitCommand
{
    internal static readonly Option<string> ConnectionOption =
        new("--connection")
        {
            Required = true,
            Description = "Connection string real, nombre lógico (ej: Default) o clave completa (ej: ConnectionStrings:Default)."
        };

    internal static readonly Option<string> MigrationsPathOption =
        new("--migrations-path")
        {
            Description = "Ruta de la carpeta de migraciones.",
            DefaultValueFactory = _ => "migrations"
        };

    internal static readonly Option<string> BaseSnapshotOption =
        new("--base-snapshot")
        {
            Description = "Ruta del snapshot base inicial.",
            DefaultValueFactory = _ => "schema.snapshot.base.json"
        };

    internal static readonly Option<string> SnapshotOption =
        new("--snapshot")
        {
            Description = "Ruta del snapshot deseado inicial.",
            DefaultValueFactory = _ => "schema.snapshot.json"
        };

    public static Command Build(FrapperConfiguration configuration)
    {
        var command = new Command(
            name: "init",
            description: "Inicializa un proyecto Frapper en el directorio actual.");

        command.Add(ConnectionOption);
        command.Add(MigrationsPathOption);
        command.Add(BaseSnapshotOption);
        command.Add(SnapshotOption);

        command.SetAction(async (parseResult, cancellationToken) =>
        {
            var rawConnection = parseResult.GetValue(ConnectionOption) ?? string.Empty;
            var migrationsPath = parseResult.GetValue(MigrationsPathOption) ?? "migrations";
            var baseSnapshotPath = parseResult.GetValue(BaseSnapshotOption) ?? "schema.snapshot.base.json";
            var snapshotPath = parseResult.GetValue(SnapshotOption) ?? "schema.snapshot.json";

            var handler = new InitHandler(
                configuration,
                new SnapshotHandler(
                    configuration,
                    new SqlServerSchemaReader(),
                    new SchemaSnapshotSerializer()));

            return await handler.RunAsync(
                rawConnection,
                migrationsPath,
                baseSnapshotPath,
                snapshotPath,
                cancellationToken);
        });

        return command;
    }
}