using ClinicaFlow.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicaFlow.Api.Infrastructure.Configurations;

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("Patients");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.LastName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.TaxCode).IsRequired().HasMaxLength(16);
        builder.Property(x => x.Phone).HasMaxLength(30);
        builder.Property(x => x.Email).HasMaxLength(150);

        builder.HasIndex(x => x.TaxCode).IsUnique();
    }
}