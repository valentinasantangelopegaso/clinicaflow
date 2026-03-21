using ClinicaFlow.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicaFlow.Api.Infrastructure.Configurations;

public class MedicalReportConfiguration : IEntityTypeConfiguration<MedicalReport>
{
    public void Configure(EntityTypeBuilder<MedicalReport> builder)
    {
        builder.ToTable("MedicalReports");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Diagnosis)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(x => x.Therapy)
            .HasMaxLength(1000);

        builder.Property(x => x.Notes)
            .HasMaxLength(2000);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasOne(x => x.Appointment)
            .WithOne(x => x.MedicalReport)
            .HasForeignKey<MedicalReport>(x => x.AppointmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.AppointmentId)
            .IsUnique();
    }
}