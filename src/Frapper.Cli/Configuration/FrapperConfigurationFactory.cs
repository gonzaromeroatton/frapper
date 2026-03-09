using Microsoft.Extensions.Configuration;

namespace Frapper.Cli.Configuration;

internal static class FrapperConfigurationFactory
{
    public static FrapperConfiguration Create()
    {
        var workingDirectory = Directory.GetCurrentDirectory();
        var targetProject = TargetProjectResolver.TryResolveFromWorkingDirectory(workingDirectory);

        var builder = new ConfigurationBuilder();

        if (targetProject is not null)
        {
            builder.SetBasePath(targetProject.ProjectDirectory);

            builder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
            builder.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false);

            if (!string.IsNullOrWhiteSpace(targetProject.UserSecretsId))
            {
                var secretsFilePath = TryResolveUserSecretsFilePath(targetProject.UserSecretsId!);

                if (!string.IsNullOrWhiteSpace(secretsFilePath))
                {
                    builder.AddJsonFile(secretsFilePath, optional: true, reloadOnChange: false);
                }
            }
        }

        builder.AddEnvironmentVariables();

        var configuration = builder.Build();

        return new FrapperConfiguration(configuration);
    }

    private static string? TryResolveUserSecretsFilePath(string userSecretsId)
    {
        if (string.IsNullOrWhiteSpace(userSecretsId))
            return null;

        string? rootDirectory = null;

        if (OperatingSystem.IsWindows())
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            if (!string.IsNullOrWhiteSpace(appData))
            {
                rootDirectory = Path.Combine(appData, "Microsoft", "UserSecrets");
            }
        }
        else
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            if (!string.IsNullOrWhiteSpace(home))
            {
                rootDirectory = Path.Combine(home, ".microsoft", "usersecrets");
            }
        }

        if (string.IsNullOrWhiteSpace(rootDirectory))
            return null;

        return Path.Combine(rootDirectory, userSecretsId, "secrets.json");
    }
}