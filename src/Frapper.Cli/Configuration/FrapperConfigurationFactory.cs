using Microsoft.Extensions.Configuration;

namespace Frapper.Cli.Configuration;

internal static class FrapperConfigurationFactory
{
    public static FrapperConfiguration Create()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .AddEnvironmentVariables()
            .Build();

        return new FrapperConfiguration(configuration);
    }
}