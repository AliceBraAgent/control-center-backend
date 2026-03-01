using ControlCenter.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControlCenter.Api.Data.Configurations;

public class SpaceRelationConfiguration : IEntityTypeConfiguration<SpaceRelation>
{
    public void Configure(EntityTypeBuilder<SpaceRelation> builder)
    {
        builder.ToTable("space_relations");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedOnAdd();

        builder.Property(r => r.RelationType).HasMaxLength(100).IsRequired();

        builder.HasOne(r => r.SourceSpace)
            .WithMany(s => s.SourceRelations)
            .HasForeignKey(r => r.SourceSpaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.TargetSpace)
            .WithMany(s => s.TargetRelations)
            .HasForeignKey(r => r.TargetSpaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => new { r.SourceSpaceId, r.TargetSpaceId, r.RelationType })
            .IsUnique();

        builder.HasIndex(r => r.TargetSpaceId);
    }
}
