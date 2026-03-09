using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Piedrazul.Domain.Entities;

namespace Piedrazul.Infrastructure.Persistence.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Specialty)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(a => a.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(a => a.Time)
            .HasConversion(
                v => v.ToString(@"hh\:mm"),
                v => TimeSpan.Parse(v));

        // Índice compuesto: evita duplicar médico+fecha+hora
        builder.HasIndex(a => new { a.DoctorId, a.Date, a.Time })
            .IsUnique();

        // Relaciones de navegación
        builder.HasOne(a => a.Patient)
            .WithMany()
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Doctor)
            .WithMany()
            .HasForeignKey(a => a.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}