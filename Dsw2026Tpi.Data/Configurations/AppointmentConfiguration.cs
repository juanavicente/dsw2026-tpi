using Dsw2026Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dsw2026Tpi.Data.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("Appointments");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.AppointmentDate)
            .IsRequired();

        builder.Property(a => a.CancellationDate);

        builder.Property(a => a.Reason)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(a => a.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.HasOne(a => a.Patient)
            .WithMany()
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Turn)
            .WithMany()
            .HasForeignKey(a => a.TurnId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => a.PatientId);

        builder.HasIndex(a => a.TurnId);

        builder.HasIndex(a => a.Status);
    }
}