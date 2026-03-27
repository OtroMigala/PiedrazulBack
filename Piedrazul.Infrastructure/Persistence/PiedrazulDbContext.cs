using Microsoft.EntityFrameworkCore;
using Piedrazul.Domain.Entities;

namespace Piedrazul.Infrastructure.Persistence;

/// <summary>
/// DbContext principal de la aplicación Piedrazul.
/// Centraliza el acceso a todas las entidades del dominio.
/// Las configuraciones de mapeo se aplican automáticamente
/// desde las clases IEntityTypeConfiguration del ensamblado.
/// </summary>
public class PiedrazulDbContext : DbContext
{
    public PiedrazulDbContext(DbContextOptions<PiedrazulDbContext> options)
        : base(options) { }

    // ── Entidades de identidad y acceso ───────────────────────────────────────

    /// <summary>Usuarios del sistema (Admin, Scheduler).</summary>
    public DbSet<User> Users => Set<User>();

    // ── Entidades del dominio médico ──────────────────────────────────────────

    /// <summary>Pacientes registrados.</summary>
    public DbSet<Patient> Patients => Set<Patient>();

    /// <summary>Médicos y terapeutas activos en el sistema.</summary>
    public DbSet<Doctor> Doctors => Set<Doctor>();

    /// <summary>Horarios de atención por médico y día de la semana.</summary>
    public DbSet<DoctorSchedule> DoctorSchedules => Set<DoctorSchedule>();

    /// <summary>Citas agendadas, completadas, canceladas o reagendadas.</summary>
    public DbSet<Appointment> Appointments => Set<Appointment>();

    // ── Auditoría ─────────────────────────────────────────────────────────────

    /// <summary>Registro de auditoría de acciones relevantes del sistema.</summary>
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    // ── Configuración del modelo ──────────────────────────────────────────────

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Aplica automáticamente todas las IEntityTypeConfiguration
        // que estén definidas en este ensamblado (Infrastructure).
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PiedrazulDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
