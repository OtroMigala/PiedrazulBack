using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Piedrazul.Domain.Entities;

namespace Piedrazul.Infrastructure.Persistence.Configurations;

public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.FullName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(d => d.Type)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(d => d.Specialty)
            .HasConversion<string>()
            .HasMaxLength(30);

        // Relación Doctor → DoctorSchedules
        builder.HasMany(d => d.Schedules)
            .WithOne()
            .HasForeignKey(s => s.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);

        // Campo privado _schedules mapeado por EF Core
        builder.Navigation(d => d.Schedules)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}