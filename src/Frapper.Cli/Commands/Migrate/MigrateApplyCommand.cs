using System.CommandLine;
using Frapper.Cli.Configuration;
using Frapper.SqlServer.MigrationExecution;

namespace Frapper.Cli.Commands.Migrate;

internal static class MigrateApplyCommand
{
    internal static readonly Option<string?> ConnectionOption =
        new("--connection")
        {
            Description = "Connection string real, nombre lógico o clave completa."
        };

    internal static readonly Option<string?> DirectoryOption =
        new("--dir")
        {
            Description = "Directorio que contiene los archivos .sql de migración."
        };

    public static Command Build(FrapperConfiguration configuration)
    {
        var command = new Command(
            name: "apply",
            description: "Aplica las migraciones SQL pendientes sobre la base de datos.");

        command.Add(ConnectionOption);
        command.Add(DirectoryOption);

        command.SetAction(async (parseResult, cancellationToken) =>
        {
            var projectFile = new FrapperProjectFileLoader().LoadOrThrow();

            var rawConnection = parseResult.GetValue(ConnectionOption) ?? projectFile.Connection;
            var directory = parseResult.GetValue(DirectoryOption) ?? projectFile.MigrationsPath;

            var handler = new MigrateApplyHandler(
                configuration,
                new SqlServerMigrationRunner());

            return await handler.RunAsync(rawConnection, directory, cancellationToken);
        });

        return command;
    }
}