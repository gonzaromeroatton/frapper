using System.CommandLine;
using Frapper.Cli.Configuration;

namespace Frapper.Cli.Commands.Migrate;

internal static class MigrateAddCommand
{
    internal static readonly Argument<string> NameArgument =
        new("name")
        {
            Description = "Nombre de la migración."
        };

    internal static readonly Option<string> ConnectionOption =
        new("--connection")
        {
            Required = true,
            Description = "Connection string real, nombre lógico (ej: Default) o clave completa (ej: ConnectionStrings:Default)."
        };

    internal static readonly Option<string> SnapshotOption =
        new("--snapshot")
        {
            Description = "Ruta del snapshot base.",
            DefaultValueFactory = _ => "schema.snapshot.json"
        };

    internal static readonly Option<string> OutDirOption =
        new("--out-dir")
        {
            Description = "Directorio de salida para la migración.",
            DefaultValueFactory = _ => "Migrations"
        };

    public static Command Build(FrapperConfiguration configuration)
    {
        var command = new Command(
            name: "add",
            description: "Genera una migración SQL a partir del diff entre snapshot y esquema actual.");

        command.Add(NameArgument);
        command.Add(ConnectionOption);
        command.Add(SnapshotOption);
        command.Add(OutDirOption);

        command.SetAction(async (parseResult, cancellationToken) =>
        {
            var name = parseResult.GetValue(NameArgument);
            var rawConnection = parseResult.GetValue(ConnectionOption);
            var snapshotPath = parseResult.GetValue(SnapshotOption) ?? "schema.snapshot.json";
            var outDir = parseResult.GetValue(OutDirOption) ?? "Migrations";

            if (string.IsNullOrWhiteSpace(name))
            {
                Console.Error.WriteLine("Error: falta el nombre de la migración.");
                return 2;
            }

            var handler = new MigrateAddHandler(configuration);

            return await handler.RunAsync(
                name,
                rawConnection,
                snapshotPath,
                outDir,
                cancellationToken);
        });

        return command;
    }
}