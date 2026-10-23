using Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<Profile>(options)
{
    public DbSet<Image> Images => Set<Image>();

    public DbSet<Library> Libraries => Set<Library>();

    public DbSet<Music> Music => Set<Music>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Image>()
            .HasOne(image => image.Uploader)
            .WithMany()
            .HasForeignKey(image => image.UploaderId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Library>()
            .HasOne(library => library.Uploader)
            .WithMany()
            .HasForeignKey(library => library.UploaderId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Music>()
            .HasOne(music => music.UploadBy)
            .WithMany()
            .HasForeignKey(music => music.UploadById)
            .OnDelete(DeleteBehavior.SetNull);
    }
}