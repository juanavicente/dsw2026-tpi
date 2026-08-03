using Dsw2026Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dsw2026Tpi.Data.Configurations;

public class TurnConfiguration : IEntityTypeConfiguration<Turn>
{
    public void Configure(EntityTypeBuilder<Turn> builder)
    {
        builder.ToTable("Turns");

        builder.HasKey(t => t.Id);

        builder.HasOne(t => t.Availability)
            .WithMany(a => a.Turns)
            .HasForeignKey(t => t.AvailabilityId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}