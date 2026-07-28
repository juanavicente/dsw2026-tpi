using Dsw2026Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dsw2026Tpi.Data.Configurations;

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("Patients");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Dni)
            .IsRequired()
            .HasMaxLength(8);

        builder.Property(p => p.Email)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(p => p.Phone)
            .IsRequired()
            .HasMaxLength(30);

        builder.HasIndex(p => p.Dni)
            .IsUnique();

        builder.HasIndex(p => p.Email)
            .IsUnique();
    }
}