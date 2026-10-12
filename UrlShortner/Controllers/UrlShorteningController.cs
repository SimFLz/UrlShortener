using Microsoft.AspNetCore.Mvc;
using UrlShortner.Entities;
using UrlShortner.Requests;
using UrlShortner.Services;
using Microsoft.EntityFrameworkCore;
namespace UrlShortner.Controllers;

[ApiController]
[Route("")]
public class UrlShorteningController(UrlShorteningService _service, ApplicationDbContext _context) : ControllerBase
{
    [HttpPost("shorten")]
    public async Task<IActionResult> ShortenUrl(ShortenUrlRequest request)
    {
        if (!Uri.TryCreate(request.Url, UriKind.Absolute, out _))
        {
            return BadRequest("The URL is not valid.");
        }
        var code = await _service.GenerateUniqueCodeAsync();

        var shortUrl = new ShortenedUrl
        {
            Id = new Guid(),
            Code = code,
            LongUrl = request.Url,
            CreatedOnUtc = DateTime.UtcNow
        };
        _context.ShortenedUrls.Add(shortUrl);
        await _context.SaveChangesAsync();
        return Ok(new { ShortUrl = $"{Request.Scheme}://{Request.Host}/{code}" });
    }

    [HttpGet("{code}")]
    public async Task<IActionResult> RedirectUrl(string code)
    {
        var shortUrl = await _context.ShortenedUrls.FirstOrDefaultAsync(u => u.Code == code);
        if (shortUrl == null)
        {
            return NotFound("The requested shortened URL was not found.");
        }

        return Redirect(shortUrl.LongUrl);
    }
}