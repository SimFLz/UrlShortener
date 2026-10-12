using Microsoft.EntityFrameworkCore;

namespace UrlShortner;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
    public DbSet<Entities.ShortenedUrl> ShortenedUrls { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Entities.ShortenedUrl>(builder =>
        {
            builder.Property(shortenedUrl => shortenedUrl.Code).HasMaxLength(ShortLinkSettings.Length);
            builder.HasIndex(shortenedUrl => shortenedUrl.Code).IsUnique();
        });

    }

}
