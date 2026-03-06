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
            Description = "Ruta del archivo snapshot.",
            DefaultValueFactory = _ => "schema.snapshot.json"
        };

    public static Command Build(FrapperConfiguration configuration)
    {
        var command = new Command(
            name: "snapshot",
            description: "Genera un snapshot determinístico del esquema actual de SQL Server.");

        command.Add(ConnectionOption);
        command.Add(OutOption);

        command.SetAction(async (parseResult, cancellationToken) =>
        {
            var rawConnection = parseResult.GetValue(ConnectionOption);
            var outPath = parseResult.GetValue(OutOption) ?? "schema.snapshot.json";

            var handler = new SnapshotHandler(configuration);

            return await handler.RunAsync(rawConnection, outPath, cancellationToken);
        });

        return command;
    }
}