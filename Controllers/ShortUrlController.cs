using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.Mvc;

using BlazorShortUrl.Helpers;
using BlazorShortUrl.Models;
using BlazorShortUrl.Services;
using BlazorShortUrl.Data;

namespace BlazorShortUrl.Controllers
{
    // [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ShortUrlController : ControllerBase
    {
        private readonly IShortUrlService _urlService;
        private readonly DataContext _context;
        private readonly ILogger<ShortUrlController> _logger;

        public ShortUrlController(DataContext context, IShortUrlService urlService, ILogger<ShortUrlController> logger)
        {
            _context = context;
            _urlService = urlService;
            _logger = logger;
        }

        // [HttpGet]
        // GET: ShortUrl
        // public async Task<ActionResult<IEnumerable<ShortUrl>>> GetAll()
        // {
        //     _logger.LogError($"GoToUrl: GetAll called");
        //     return await _context.ShortUrls.ToListAsync();
        // }

        // [AllowAnonymous]
        // [HttpPost]
        [HttpGet("{id}")]
        // GET: ShortUrl/AAXXGG
        public IActionResult RedirectToUrl(string id)
        {
            try
            {
                var path = HttpContext.Request.Path.ToUriComponent().Trim('/');
                var decodeId = BitConverter.ToInt32(WebEncoders.Base64UrlDecode(id));

                // Return value from async method
                ShortUrlDto shortUrl = _urlService.GetByIdAsync(decodeId).Result;

                var sUrl = shortUrl.Url ?? "/";
                var uri = new Uri(sUrl);

                // string msg = $"GoToUrl: {uri.AbsoluteUri}";
                // _logger.LogInformation(msg);

                // Redirect to Url
                return Redirect(uri.AbsoluteUri);
            }
            catch (AppException ex)
            {
                // return error message
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
