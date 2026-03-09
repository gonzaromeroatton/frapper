using System.CommandLine;
using Frapper.Cli.Configuration;

namespace Frapper.Cli.Commands.Diff;

internal static class DiffCommand
{
    internal static readonly Option<string?> BaseOption =
        new("--base")
        {
            Description = "Ruta del snapshot base."
        };

    internal static readonly Option<string?> TargetOption =
        new("--target")
        {
            Description = "Ruta del snapshot objetivo."
        };

    public static Command Build(FrapperConfiguration configuration)
    {
        var command = new Command(
            name: "diff",
            description: "Compara dos snapshots de esquema y muestra las diferencias detectadas.");

        command.Add(BaseOption);
        command.Add(TargetOption);

        command.SetAction(async (parseResult, cancellationToken) =>
        {
            var projectFile = new FrapperProjectFileLoader().LoadOrThrow();

            var basePath = parseResult.GetValue(BaseOption) ?? projectFile.BaseSnapshot;
            var targetPath = parseResult.GetValue(TargetOption) ?? projectFile.Snapshot;

            var handler = new DiffHandler();

            return await handler.RunAsync(basePath, targetPath, cancellationToken);
        });

        return command;
    }
}