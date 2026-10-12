using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
namespace UrlShortner.Services;

public class UrlShorteningService(ApplicationDbContext dbContext)
{
    private readonly Random _random = new();

    public async Task<string> GenerateUniqueCodeAsync()
    {
        var codeChars = new char[ShortLinkSettings.Length];
        var maxValue = ShortLinkSettings.Alphabet.Length;
        while(true)
        {
            for(var i = 0; i < ShortLinkSettings.Length; i++)
            {
                var randomIndex = _random.Next(maxValue);
                codeChars[i] = ShortLinkSettings.Alphabet[randomIndex];
            }

            var code = new string(codeChars);
            if(!await dbContext.ShortenedUrls.AnyAsync(u => u.Code == code))
            {
                return code;
            }
        }
    }
}
