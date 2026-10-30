using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Core.Entities;
using Infrastructure.Configurations;

namespace Infrastructure.Data;

public class AgoraRivasContext(DbContextOptions<AgoraRivasContext> options) : IdentityDbContext<Profile>(options)
{
    public DbSet<Document> Documents { get; set; }
    public DbSet<Image> Images { get; set; }
    public DbSet<Profile> Profiles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DocumentConfiguration).Assembly);
    }
    
}