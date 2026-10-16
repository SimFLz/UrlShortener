
namespace UrlShortner.Requests;

public record ShortenUrlRequest(string Url);
public record LinkStatsResponse(int TotalClicks, List<DailyClicksDto> DailyClicks);

public record DailyClicksDto(DateTime Date, int Clicks);

public record ShortenUrlDto(string Code, string ShortUrl, string LongUrl, string CreatedOnUtc);