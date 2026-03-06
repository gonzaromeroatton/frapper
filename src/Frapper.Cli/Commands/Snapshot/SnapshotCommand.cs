using System.CommandLine;
using Frapper.Cli.Configuration;

namespace Frapper.Cli.Commands.Snapshot;

internal static class SnapshotCommand
{
    internal static readonly Option<string> ConnectionOption =
        new("--connection")
        {
            Required = true,
            Description = "Connection string real, nombre lógico (ej: Default) o clave completa (ej: ConnectionStrings:Default)."
        };

    internal static readonly Option<string> OutOption =
        new("--out")
        {
            Description = "Ruta del snapshot deseado.",
            DefaultValueFactory = _ => "schema.snapshot.json"
        };

    internal static readonly Option<string> BaseOutOption =
        new("--base-out")
        {
            Description = "Ruta del snapshot base.",
            DefaultValueFactory = _ => "schema.snapshot.base.json"
        };

    public static Command Build(FrapperConfiguration configuration)
    {
        var command = new Command(
            name: "snapshot",
            description: "Genera snapshots iniciales a partir del esquema actual de SQL Server.");

        command.Add(ConnectionOption);
        command.Add(OutOption);
        command.Add(BaseOutOption);

        command.SetAction(async (parseResult, cancellationToken) =>
        {
            var rawConnection = parseResult.GetValue(ConnectionOption);
            var outPath = parseResult.GetValue(OutOption) ?? "schema.snapshot.json";
            var baseOutPath = parseResult.GetValue(BaseOutOption) ?? "schema.snapshot.base.json";

            var handler = new SnapshotHandler(configuration);

            return await handler.RunAsync(rawConnection, outPath, baseOutPath, cancellationToken);
        });

        return command;
    }
}