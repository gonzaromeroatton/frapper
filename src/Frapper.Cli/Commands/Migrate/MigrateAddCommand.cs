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

    internal static readonly Option<string> SnapshotOption =
        new("--snapshot")
        {
            Description = "Ruta del snapshot deseado.",
            DefaultValueFactory = _ => "schema.snapshot.json"
        };

    internal static readonly Option<string> BaseSnapshotOption =
        new("--base-snapshot")
        {
            Description = "Ruta del snapshot base.",
            DefaultValueFactory = _ => "schema.snapshot.base.json"
        };

    internal static readonly Option<string> OutDirOption =
        new("--out-dir")
        {
            Description = "Directorio de salida para la migración.",
            DefaultValueFactory = _ => "Migrations"
        };

    internal static readonly Option<bool> AllowDestructiveOption =
        new("--allow-destructive")
        {
            Description = "Permite generar operaciones destructivas, como eliminar columnas o tablas."
        };

    public static Command Build(FrapperConfiguration configuration)
    {
        var command = new Command(
            name: "add",
            description: "Genera una migración SQL a partir del diff entre snapshot base y snapshot deseado.");

        command.Add(NameArgument);
        command.Add(SnapshotOption);
        command.Add(BaseSnapshotOption);
        command.Add(OutDirOption);
        command.Add(AllowDestructiveOption);

        command.SetAction(async (parseResult, cancellationToken) =>
        {
            var name = parseResult.GetValue(NameArgument);
            var snapshotPath = parseResult.GetValue(SnapshotOption) ?? "schema.snapshot.json";
            var baseSnapshotPath = parseResult.GetValue(BaseSnapshotOption) ?? "schema.snapshot.base.json";
            var outDir = parseResult.GetValue(OutDirOption) ?? "Migrations";
            var allowDestructive = parseResult.GetValue(AllowDestructiveOption);

            if (string.IsNullOrWhiteSpace(name))
            {
                Console.Error.WriteLine("Error: falta el nombre de la migración.");
                return 2;
            }

            var handler = new MigrateAddHandler(configuration);

            return await handler.RunAsync(
                name,
                snapshotPath,
                baseSnapshotPath,
                outDir,
                allowDestructive,
                cancellationToken);
        });

        return command;
    }
}