using Microsoft.Extensions.Configuration;

namespace Frapper.Cli.Configuration;

internal sealed class FrapperConfiguration
{
    private readonly IConfiguration _configuration;

    public FrapperConfiguration(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string? ResolveConnectionString(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;

        input = input.Trim();

        // Caso 1: parece un connection string completo
        if (LooksLikeRawConnectionString(input))
            return input;

        // Caso 2: nombre lógico, por ejemplo "Default"
        var fromNamedConnection = _configuration.GetConnectionString(input);
        if (!string.IsNullOrWhiteSpace(fromNamedConnection))
            return fromNamedConnection;

        // Caso 3: clave completa, por ejemplo "ConnectionStrings:Default"
        var fromKey = _configuration[input];
        if (!string.IsNullOrWhiteSpace(fromKey))
            return fromKey;

        return null;
    }

    private static bool LooksLikeRawConnectionString(string value)
    {
        return value.Contains('=') && value.Contains(';');
    }
}