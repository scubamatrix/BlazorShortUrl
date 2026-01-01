namespace BlazorShortUrl.Entities
{
    public class ShortUrl
    {
        public ShortUrl() { }

        // Copy ShortUrl
        public ShortUrl(ShortUrl shortUrl)
        {
            Id = shortUrl.Id;
            UserId = shortUrl.UserId;
            Url = shortUrl.Url;
            TinyUrl = shortUrl.TinyUrl;
        }

        public int Id { get; set; }
        public string? UserId { get; set; }
        public string? Url { get; set; }
        public string? TinyUrl { get; set; }
        public string? Description { get; set; }
    }
}