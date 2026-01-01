using BlazorShortUrl.Data;
using BlazorShortUrl.Entities;
using BlazorShortUrl.Helpers;
using BlazorShortUrl.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorShortUrl.Services;

public class ShortUrlService : IShortUrlService
{
    private readonly IShortUrlRepository _repo;

    public ShortUrlService(IShortUrlRepository shortUrlRepository)
    {
        _repo = shortUrlRepository;
    }


    public async Task<List<ShortUrlDto>> GetAllAsync(string userId)
    {
        var shortUrls = await _repo.GetAllAsync(userId);
        return shortUrls.Select(shortUrl => new ShortUrlDto(shortUrl)).ToList();
    }

    public async Task<ShortUrlDto> GetByIdAsync(int id)
    {
        ShortUrl shortUrl = await _repo.GetById(id);
        ShortUrlDto shortUrlDto = new ShortUrlDto(shortUrl);
        return shortUrlDto;
    }

    public async Task<ShortUrlDto> PostUrlAsync(ShortUrlDto shortUrlDto)
    {
        if (shortUrlDto == null)
            throw new AppException("shortUrl is empty.");

        var addShortUrl = new ShortUrl();
        addShortUrl.Id = shortUrlDto.Id;
        addShortUrl.UserId = shortUrlDto.UserId;
        addShortUrl.Url = shortUrlDto.Url;
        addShortUrl.TinyUrl = shortUrlDto.TinyUrl;
        addShortUrl.Description = shortUrlDto.Description;

        var newShortUrl = await _repo.PostUrlAsync(addShortUrl);
        return new ShortUrlDto(newShortUrl);
    }

    public async Task PutUrlAsync(ShortUrlDto inputUrlDto)
    {
        ShortUrl updateUrl = new ShortUrl();
        updateUrl.Id = inputUrlDto.Id;
        updateUrl.UserId = inputUrlDto.UserId;
        updateUrl.Url = inputUrlDto.Url;
        updateUrl.TinyUrl = inputUrlDto.TinyUrl;
        updateUrl.Description = inputUrlDto.Description;
        await _repo.PutUrlAsync(updateUrl);
    }
    public async Task DeleteUrlAsync(int id)
    {
        await _repo.DeleteUrlAsync(id);
    }
}
