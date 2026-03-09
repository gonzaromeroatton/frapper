using System.CommandLine;
using Frapper.Cli.Configuration;

namespace Frapper.Cli.Commands.Migrate;

internal static class MigrateCommand
{
    public static Command Build(FrapperConfiguration configuration)
    {
        var command = new Command(
            name: "migrate",
            description: "Comandos de migración.");

        command.Add(MigrateAddCommand.Build(configuration));
        command.Add(MigrateApplyCommand.Build(configuration));

        return command;
    }
}