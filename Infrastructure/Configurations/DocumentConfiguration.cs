using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.Property(d => d.Title).IsRequired();
        builder.Property(d => d.ContentType).IsRequired();
        builder.Property(d => d.Type)
            .HasConversion<string>()
            .IsRequired();        
    }
}