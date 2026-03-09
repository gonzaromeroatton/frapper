using System.Xml.Linq;

namespace Frapper.Cli.Configuration;

internal static class TargetProjectResolver
{
    public static TargetProjectInfo? TryResolveFromWorkingDirectory(string workingDirectory)
    {
        if (string.IsNullOrWhiteSpace(workingDirectory))
            return null;

        if (!Directory.Exists(workingDirectory))
            return null;

        var projectFiles = Directory.GetFiles(workingDirectory, "*.csproj", SearchOption.TopDirectoryOnly);

        if (projectFiles.Length == 0)
            return null;

        if (projectFiles.Length > 1)
        {
            throw new InvalidOperationException(
                $"Se encontraron múltiples proyectos .csproj en '{workingDirectory}'. " +
                "Ejecuta Frapper dentro del directorio del proyecto target.");
        }

        var projectFilePath = projectFiles[0];
        var projectDirectory = Path.GetDirectoryName(projectFilePath)
            ?? throw new InvalidOperationException("No se pudo determinar el directorio del proyecto.");

        var userSecretsId = TryReadUserSecretsId(projectFilePath);

        return new TargetProjectInfo(projectFilePath, projectDirectory, userSecretsId);
    }

    private static string? TryReadUserSecretsId(string projectFilePath)
    {
        var document = XDocument.Load(projectFilePath);

        var userSecretsElement = document
            .Descendants()
            .FirstOrDefault(x => string.Equals(x.Name.LocalName, "UserSecretsId", StringComparison.OrdinalIgnoreCase));

        var value = userSecretsElement?.Value?.Trim();

        return string.IsNullOrWhiteSpace(value)
            ? null
            : value;
    }
}