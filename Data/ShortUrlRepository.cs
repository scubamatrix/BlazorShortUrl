using BlazorShortUrl.Entities;
using BlazorShortUrl.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
// using MongoDB.Driver;

namespace BlazorShortUrl.Data;

public class ShortUrlRepository : IShortUrlRepository
{
    private readonly DataContext _db;

    public ShortUrlRepository(DataContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<ShortUrl>> GetAllAsync(string userId)
    {
        if (_db == null)
            return Enumerable.Empty<ShortUrl>();

        var queryShortUrls =
            from shortUrl in _db.ShortUrls
            where shortUrl.UserId == userId
            select shortUrl;

        return await queryShortUrls.ToListAsync();
        // return await _db.ShortUrls.ToListAsync();
    }

    public async Task<ShortUrl> GetById(int id)
    {
        ShortUrl? shortUrl = await _db.ShortUrls.FindAsync(id);
        
        if (shortUrl == null)
            throw new AppException("Url not found.");

        return shortUrl;
    }

    public async Task<ShortUrl> PostUrlAsync(ShortUrl shortUrl)
    {
        if (_db.ShortUrls.Any(x => x.Url == shortUrl.Url && x.UserId == shortUrl.UserId))
            throw new AppException("Url \"" + shortUrl.Url + "\" is already taken");

        if (shortUrl == null)
            throw new AppException("Url is empty.");

        _db.ShortUrls.Add(shortUrl);
        await _db.SaveChangesAsync();

        return shortUrl;
    }

    public async Task PutUrlAsync(ShortUrl updateUrl)
    {
        ShortUrl? shortUrl;

        shortUrl = await _db.ShortUrls.FindAsync(updateUrl.Id);

        if (shortUrl == null)
            throw new AppException("Url not found.");

        shortUrl.Url = updateUrl.Url;
        shortUrl.TinyUrl = updateUrl.TinyUrl;
        shortUrl.Description = updateUrl.Description;
        await _db.SaveChangesAsync();
    }

    public async Task DeleteUrlAsync(int id)
    {
        if (await _db.ShortUrls.FindAsync(id) is ShortUrl shortUrl)
        {
            _db.ShortUrls.Remove(shortUrl);
            await _db.SaveChangesAsync();
        }
    }
}