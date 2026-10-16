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
    [HttpPost("link/create")]
    public async Task<IActionResult> ShortenUrl(ShortenUrlRequest request)
    {
        if (!Uri.TryCreate(request.Url, UriKind.Absolute, out var uriResult) ||
            (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
        {
            return BadRequest("The URL is not valid");
        }
        const int maxRetries = 3;
        for (int i = 0; i < maxRetries; i++)
        {
            var code = await _service.GenerateUniqueCodeAsync();

            var shortUrl = new ShortenedUrl
            {
                Id = Guid.NewGuid(),
                Code = code,
                LongUrl = request.Url,
                CreatedOnUtc = DateTime.UtcNow
            };
            try
            {
                _context.ShortenedUrls.Add(shortUrl);
                await _context.SaveChangesAsync();

                
                var localDomain = $"{Request.Scheme}://{Request.Host}";
                return Ok(new { ShortUrl = $"{localDomain}/{code}" });
            }
            catch (DbUpdateException) 
            {
                
                if (i == maxRetries - 1) throw;
                _context.Entry(shortUrl).State = EntityState.Detached;
            }
        }

        return StatusCode(500, "An error occurred while generating a unique code.");
    }
    

    [HttpGet("{code}")]
    public async Task<IActionResult> RedirectUrl(string code)
    {
        var shortUrl = await _context.ShortenedUrls.FirstOrDefaultAsync(u => u.Code == code);
        if (shortUrl == null)
        {
            return NotFound("The requested shortened URL was not found.");
        }
        var userAgent = Request.Headers.UserAgent.ToString();
        var referer = Request.Headers.Referer.ToString();

        var click = new Click
        {
            Id = Guid.NewGuid(),
            ShortenedUrlId = shortUrl.Id,
            ClickedOnUtc = DateTime.UtcNow,
            UserAgent = string.IsNullOrEmpty(userAgent) ? null : userAgent,
            Referer = string.IsNullOrEmpty(referer) ? null : referer
        };

        _context.Clicks.Add(click);
        await _context.SaveChangesAsync();

        return Redirect(shortUrl.LongUrl);
    }

    [HttpGet("clicks/{code}")]
    public async Task<IActionResult> GetClickStats(string code)
    {
        var shortUrl = await _context.ShortenedUrls.FirstOrDefaultAsync(u => u.Code == code);
        if (shortUrl == null)
        {
            return NotFound("The requested shortened URL was not found.");
        }

        var clickQuery = _context.Clicks.Where(u => u.ShortenedUrlId == shortUrl.Id);
        var totalClicks = await clickQuery.CountAsync();
        var dailyClicks = await clickQuery.GroupBy(c => c.ClickedOnUtc.Date)
            .Select(g => new DailyClicksDto(g.Key, g.Count()))
            .ToListAsync();

        return Ok(new { TotalClicks = totalClicks, DailyClicks = dailyClicks });
    }
    

    [HttpGet("link/all")]
    public async Task<IActionResult> GetAllShortenedUrls()
    {
        var localDomain = $"{Request.Scheme}://{Request.Host}";

        var totalLinks = await _context.ShortenedUrls.CountAsync();

        var Data = await _context.ShortenedUrls.Select(u => new
        { u.Code, u.LongUrl, u.CreatedOnUtc}).ToListAsync();


        var linksDto = Data.Select(u => new ShortenUrlDto
        (
            u.Code,
            $"{localDomain}/{u.Code}",
            u.LongUrl,
            u.CreatedOnUtc.ToString("yyyy-MM-dd HH:mm:ss")
        )).ToList();

        return Ok(new { Total = totalLinks, Links = linksDto });
    }
}