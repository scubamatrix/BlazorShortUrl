using DotNetEnv;

namespace BlazorShortUrl.Middleware;

public static class Constants
{
    public const string ApiKeyHeaderName = "X-API-Key";
    public const string ApiKeyName = "ApiKey";
}

public interface IApiKeyValidation
{
    bool IsValidApiKey(string userApiKey);
}

/// <summary>
/// Passing API Key in Request Headers
/// API Key Authentication via Custom Attributes [9].
/// </summary>
public class ApiKeyValidation : IApiKeyValidation
{
    private readonly IConfiguration _configuration;

    public ApiKeyValidation(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public bool IsValidApiKey(string userApiKey)
    {
        if (string.IsNullOrWhiteSpace(userApiKey))
            return false;

        // string? apiKey = _configuration.GetValue<string>(Constants.ApiKeyName);
        string? apiKey = Env.GetString("API_KEY");

        if (apiKey == null || apiKey != userApiKey)
            return false;

        return true;
    }
}
