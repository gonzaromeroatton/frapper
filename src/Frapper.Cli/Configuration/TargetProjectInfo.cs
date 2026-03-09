namespace Frapper.Cli.Configuration;

internal sealed class TargetProjectInfo
{
    public string ProjectFilePath { get; }
    public string ProjectDirectory { get; }
    public string? UserSecretsId { get; }

    public TargetProjectInfo(
        string projectFilePath,
        string projectDirectory,
        string? userSecretsId)
    {
        ProjectFilePath = projectFilePath;
        ProjectDirectory = projectDirectory;
        UserSecretsId = userSecretsId;
    }
}