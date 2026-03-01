using ControlCenter.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControlCenter.Api.Data.Configurations;

public class SpaceConfiguration : IEntityTypeConfiguration<Space>
{
    public void Configure(EntityTypeBuilder<Space> builder)
    {
        builder.ToTable("spaces");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(s => s.Name).HasMaxLength(200).IsRequired();
        builder.Property(s => s.Description).HasMaxLength(2000);

        builder.HasMany(s => s.Tasks)
            .WithOne(t => t.Space)
            .HasForeignKey(t => t.SpaceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
