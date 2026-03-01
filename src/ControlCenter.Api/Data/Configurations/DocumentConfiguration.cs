using ControlCenter.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControlCenter.Api.Data.Configurations;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("documents");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).ValueGeneratedOnAdd();

        builder.Property(d => d.Title).HasMaxLength(500).IsRequired();
        builder.Property(d => d.Slug).HasMaxLength(500).IsRequired();
        builder.Property(d => d.DocumentType).HasMaxLength(50).IsRequired();
        builder.Property(d => d.CreatedBy).HasMaxLength(200);
        builder.Property(d => d.UpdatedBy).HasMaxLength(200);

        builder.HasIndex(d => new { d.SpaceId, d.Slug }).IsUnique();
        builder.HasIndex(d => new { d.SpaceId, d.SortOrder });
    }
}
