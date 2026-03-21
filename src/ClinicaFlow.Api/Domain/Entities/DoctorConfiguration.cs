using ClinicaFlow.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicaFlow.Api.Infrastructure.Configurations;

public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        builder.ToTable("Doctors");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.LastName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Email).HasMaxLength(150);

        builder.HasOne(x => x.Specialty)
            .WithMany(x => x.Doctors)
            .HasForeignKey(x => x.SpecialtyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}