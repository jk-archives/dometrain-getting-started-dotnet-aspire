using Microsoft.EntityFrameworkCore;

namespace Entities;

public class PodcastDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Podcast> Podcasts { get; set; }
}