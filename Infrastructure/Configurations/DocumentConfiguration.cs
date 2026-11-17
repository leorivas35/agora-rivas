using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class DocumentConfiguration : IEntityTypeConfiguration<AgoraFile>
{
    public void Configure(EntityTypeBuilder<AgoraFile> builder)
    {        
        builder.Property(d => d.Title).HasMaxLength(200).IsRequired();        
        builder.Property(d => d.ContentType).HasMaxLength(150).IsRequired();
        builder.Property(d => d.Extension).HasMaxLength(10).IsRequired();        
        builder.HasOne(d => d.Content)
               .WithOne(c => c.Document)
               .HasForeignKey<DocumentContent>(c => c.DocumentId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}