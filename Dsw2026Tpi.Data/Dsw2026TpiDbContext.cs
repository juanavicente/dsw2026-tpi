using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Dsw2026Tpi.Domain.Entities;

namespace Dsw2026Tpi.Data;

public class Dsw2026TpiDbContext : DbContext
{
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Speciality> Specialities => Set<Speciality>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Availability> Availabilities => Set<Availability>();
    public DbSet<Turn> Turns => Set<Turn>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public Dsw2026TpiDbContext(DbContextOptions<Dsw2026TpiDbContext> options):
        base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
