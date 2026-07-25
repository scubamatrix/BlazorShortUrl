using BlazorShortUrl.Entities;

namespace BlazorShortUrl.Data;

public interface IShortUrlRepository
{
    public Task<IEnumerable<ShortUrl>> GetAllAsync(string userId);
    public Task<ShortUrl> GetById(int id);
    public Task<ShortUrl> PostUrlAsync(ShortUrl shortUrl);
    public Task PutUrlAsync(ShortUrl shortUrl);
    public Task DeleteUrlAsync(int id);

    // void Update(ShortUrl urlParam);
}