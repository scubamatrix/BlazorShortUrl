using BlazorShortUrl.Entities;
using DotNetEnv;
using Microsoft.AspNetCore.WebUtilities;

namespace BlazorShortUrl.Models;

public class ShortUrlDto
{
    public int Id { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? Url { get; set; }
    public string? TinyUrl { get; set; }
    public string? Description { get; set; }

    public ShortUrlDto() {}

    // Copy ShortUrl
    public ShortUrlDto(ShortUrl shortUrl)
    {
        Id = shortUrl.Id;
        UserId = shortUrl.UserId;
        Url = shortUrl.Url;
        TinyUrl = shortUrl.TinyUrl;
        Description = shortUrl.Description;
    }

    // Copy ShortUrlDto
    public ShortUrlDto(ShortUrlDto shortUrlDto)
    {
        Id = shortUrlDto.Id;
        UserId = shortUrlDto.UserId;
        Url = shortUrlDto.Url;
        TinyUrl = shortUrlDto.TinyUrl;
        Description = shortUrlDto.Description;
    }

    public string GetUrlChunk(int id)
    {
        // Generate shortened URL
        var urlChunk = WebEncoders.Base64UrlEncode(BitConverter.GetBytes(id));
        var result = $"{Env.GetString("APP_URL")}/api/ShortUrl/{urlChunk}";
        return result;
    }

    public static int GetId(string urlChunk)
    {
        // Reverse shortened url back to int
        return BitConverter.ToInt32(WebEncoders.Base64UrlDecode(urlChunk));
    }
}