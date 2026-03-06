using System.CommandLine;
using Frapper.Cli.Commands.Migrate;
using Frapper.Cli.Commands.Snapshot;
using Frapper.Cli.Configuration;

var frapperConfiguration = FrapperConfigurationFactory.Create();

var root = new RootCommand("Frapper - Migraciones para arquitecturas Dapper-first (SQL Server).");

root.Add(SnapshotCommand.Build(frapperConfiguration));
root.Add(MigrateCommand.Build(frapperConfiguration));

var parseResult = root.Parse(args);
return await parseResult.InvokeAsync();