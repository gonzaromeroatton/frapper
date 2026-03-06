using System.CommandLine;
using Frapper.Cli.Configuration;

namespace Frapper.Cli.Commands.Migrate;

internal static class MigrateCommand
{
    public static Command Build(FrapperConfiguration configuration)
    {
        var command = new Command(
            name: "migrate",
            description: "Comandos relacionados con generación de migraciones.");

        command.Add(MigrateAddCommand.Build(configuration));

        return command;
    }
}