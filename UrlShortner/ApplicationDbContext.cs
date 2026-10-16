using Microsoft.EntityFrameworkCore;
using UrlShortner.Entities;
namespace UrlShortner;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
    public DbSet<ShortenedUrl> ShortenedUrls { get; set; } = null!;
    public DbSet<Click> Clicks { get; set; } = null!;
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Entities.ShortenedUrl>(builder =>
        {
            builder.Property(shortenedUrl => shortenedUrl.Code).HasMaxLength(ShortLinkSettings.Length);
            builder.HasIndex(shortenedUrl => shortenedUrl.Code).IsUnique();
        });

    }

}
