using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Piedrazul.Domain.Entities;

namespace Piedrazul.Infrastructure.Persistence.Configurations;

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.DocumentId)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(p => p.DocumentId)
            .IsUnique();

        builder.Property(p => p.FullName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(p => p.Phone)
            .IsRequired()
            .HasMaxLength(15);

        builder.Property(p => p.Email)
            .HasMaxLength(100);

        builder.Property(p => p.Gender)
            .HasConversion<string>()
            .HasMaxLength(10);
    }
}