using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Piedrazul.Domain.Entities;

namespace Piedrazul.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Action)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.PerformedByUserId)
            .IsRequired();

        builder.Property(a => a.AppointmentTime)
            .HasConversion(
                t => t.HasValue ? t.Value.ToString(@"hh\:mm") : null,
                s => s != null ? TimeSpan.Parse(s) : null);

        builder.Property(a => a.OccurredAt)
            .IsRequired();

        // Índices para consultas frecuentes de auditoría
        builder.HasIndex(a => a.PerformedByUserId);
        builder.HasIndex(a => a.AppointmentId);
        builder.HasIndex(a => a.OccurredAt);
    }
}
