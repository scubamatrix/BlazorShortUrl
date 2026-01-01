namespace BlazorShortUrl.Helpers;

/// <summary>
/// Helper class to access base address of website.
/// </summary>
public class HttpBaseUrlAccessor : IHttpBaseUrlAccessor
{
    public string? SiteUrlString { get; set; } = string.Empty;

    public string? GetHttpsUrl()
    {
        string[]? urls;
        if (SiteUrlString is not null)
        {
            urls = SiteUrlString.Split(";");
            return urls.FirstOrDefault(g => g.StartsWith("https://"));
        }
        else
        {
            return string.Empty;
        }
    }

    public string? GetHttpUrl()
    {
        string[]? urls;
        if (SiteUrlString is not null)
        {
            urls = SiteUrlString.Split(";");
            return urls.FirstOrDefault(g => g.StartsWith("http://"));
        }
        else
        {
            return string.Empty;
        }
    }
}