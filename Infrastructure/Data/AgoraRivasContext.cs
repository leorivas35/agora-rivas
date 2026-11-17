using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Core.Entities;
using Infrastructure.Configurations;

namespace Infrastructure.Data;

public class AgoraRivasContext(DbContextOptions<AgoraRivasContext> options) 
: IdentityDbContext<AppUser>(options)
{
    public DbSet<AgoraFile> Documents { get; set; }
    public DbSet<DocumentContent> DocumentContents { get; set; }
    public DbSet<Image> Images { get; set; }
    public DbSet<Profile> Profiles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DocumentConfiguration).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DocumentContentConfig).Assembly);
        /*
        // Or chain calls with a predicate filter
            modelBuilder
                .ApplyConfigurationsFromAssembly(
                    typeof(AppDbContext).Assembly, 
                    t => t.Namespace?.Contains("Core") == true)
                .ApplyConfigurationsFromAssembly(
                    typeof(AppDbContext).Assembly, 
                    t => t.Namespace?.Contains("Audit") == true);
        */
    }
    
}