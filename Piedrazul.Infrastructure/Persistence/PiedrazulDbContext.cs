using Microsoft.EntityFrameworkCore;
using Piedrazul.Domain.Entities;

namespace Piedrazul.Infrastructure.Persistence;

public class PiedrazulDbContext : DbContext
{
    public PiedrazulDbContext(DbContextOptions<PiedrazulDbContext> options)
        : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<DoctorSchedule> DoctorSchedules => Set<DoctorSchedule>();
    public DbSet<Appointment> Appointments => Set<Appointment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Aplica automáticamente todas las IEntityTypeConfiguration
        // que estén en este ensamblado
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PiedrazulDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}