using System;
using System.Collections.Generic;
using System.Text;

using Dsw2026Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dsw2026Tpi.Data.Configurations;

public class TurnConfiguration : IEntityTypeConfiguration<Turn>
{
    public void Configure(EntityTypeBuilder<Turn> builder)
    {
        builder.HasOne(t => t.Availability)
            .WithMany()
            .HasForeignKey(t => t.AvailabilityId);
    }
}
