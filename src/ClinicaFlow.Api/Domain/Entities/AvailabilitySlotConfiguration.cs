using ClinicaFlow.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicaFlow.Api.Infrastructure.Configurations;

public class AvailabilitySlotConfiguration : IEntityTypeConfiguration<AvailabilitySlot>
{
    public void Configure(EntityTypeBuilder<AvailabilitySlot> builder)
    {
        builder.ToTable("AvailabilitySlots");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.StartDateTime).IsRequired();
        builder.Property(x => x.EndDateTime).IsRequired();
        builder.Property(x => x.IsAvailable).IsRequired();

        builder.HasOne(x => x.Doctor)
            .WithMany(x => x.AvailabilitySlots)
            .HasForeignKey(x => x.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Appointment)
            .WithOne(x => x.AvailabilitySlot)
            .HasForeignKey<Appointment>(x => x.AvailabilitySlotId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}