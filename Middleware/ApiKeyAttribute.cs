using Microsoft.AspNetCore.Mvc;

namespace BlazorShortUrl.Middleware;

public class ApiKeyAttribute : ServiceFilterAttribute
{
    public ApiKeyAttribute() : base(typeof(ApiKeyAuthFilter))
    {
    }
}