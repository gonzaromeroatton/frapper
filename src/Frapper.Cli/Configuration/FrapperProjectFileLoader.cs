using System.Text.Json;

namespace Frapper.Cli.Configuration;

internal sealed class FrapperProjectFileLoader
{
    public const string DefaultFileName = "frapper.config.json";

    public FrapperProjectFile LoadOrThrow(string? path = null)
    {
        var filePath = string.IsNullOrWhiteSpace(path)
            ? DefaultFileName
            : path;

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException(
                $"No se encontró el archivo de configuración del proyecto Frapper: '{filePath}'.");
        }

        var json = File.ReadAllText(filePath);

        var config = JsonSerializer.Deserialize<FrapperProjectFile>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (config is null)
        {
            throw new InvalidOperationException(
                $"No fue posible deserializar el archivo '{filePath}'.");
        }

        return config;
    }
}