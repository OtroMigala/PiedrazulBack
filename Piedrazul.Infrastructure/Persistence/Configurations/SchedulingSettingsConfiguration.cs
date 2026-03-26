using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Piedrazul.Domain.Entities;

namespace Piedrazul.Infrastructure.Persistence.Configurations;

public class SchedulingSettingsConfiguration : IEntityTypeConfiguration<SchedulingSettings>
{
    public void Configure(EntityTypeBuilder<SchedulingSettings> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.WeeksAhead)
            .IsRequired();
    }
}