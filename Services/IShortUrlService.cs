using BlazorShortUrl.Models;

namespace BlazorShortUrl.Services;

public interface IShortUrlService
{
    public Task<List<ShortUrlDto>> GetAllAsync(string userId);
    public Task<ShortUrlDto> GetByIdAsync(int id);
    public Task<ShortUrlDto> PostUrlAsync(ShortUrlDto shortUrlDto);
    public Task PutUrlAsync(ShortUrlDto shortUrlDto);
    public Task DeleteUrlAsync(int id);

    // void Update(ShortUrlDto urlParam);
}