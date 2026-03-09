using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Piedrazul.Domain.Entities;

namespace Piedrazul.Infrastructure.Persistence.Configurations;

public class DoctorScheduleConfiguration : IEntityTypeConfiguration<DoctorSchedule>
{
    public void Configure(EntityTypeBuilder<DoctorSchedule> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.DayOfWeek)
            .HasConversion<string>()
            .HasMaxLength(10);

        // TimeSpan → string en PostgreSQL
        builder.Property(s => s.StartTime)
            .HasConversion(
                v => v.ToString(@"hh\:mm"),
                v => TimeSpan.Parse(v));

        builder.Property(s => s.EndTime)
            .HasConversion(
                v => v.ToString(@"hh\:mm"),
                v => TimeSpan.Parse(v));
    }
}