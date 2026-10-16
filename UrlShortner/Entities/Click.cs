namespace UrlShortner.Entities;

public class Click
{
    public Guid Id { get; set; }
    public Guid ShortenedUrlId { get; set; }
    public DateTime ClickedOnUtc { get; set; }
    public string? UserAgent { get; set; }
    public string? Referer { get; set; }
}
