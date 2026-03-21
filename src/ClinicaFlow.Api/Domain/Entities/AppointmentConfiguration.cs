using ClinicaFlow.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicaFlow.Api.Infrastructure.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("Appointments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.AppointmentDate).IsRequired();
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(1000);

        builder.HasOne(x => x.Patient)
            .WithMany(x => x.Appointments)
            .HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Doctor)
            .WithMany(x => x.Appointments)
            .HasForeignKey(x => x.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}