using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicaFlow.Api.Domain.Entities;

/// <summary>
/// Configurazione Entity Framework dell'entità Appointment.
/// </summary>
public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    /// <summary>
    /// Configura la mappatura dell'entità Appointment.
    /// </summary>
    /// <param name="builder">Builder di configurazione dell'entità.</param>
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        // Configura la chiave primaria della tabella.
        builder.HasKey(a => a.Id);

        // Configura il campo Note come facoltativo con lunghezza massima.
        builder.Property(a => a.Notes)
            .HasMaxLength(500)
            .IsRequired(false);

        // Configura il campo Status come obbligatorio.
        builder.Property(a => a.Status)
            .IsRequired();

        // Configura la relazione con il paziente.
        builder.HasOne(a => a.Patient)
            .WithMany(p => p.Appointments)
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configura la relazione con il medico.
        builder.HasOne(a => a.Doctor)
            .WithMany(d => d.Appointments)
            .HasForeignKey(a => a.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configura la relazione uno a uno con lo slot di disponibilità.
        builder.HasOne(a => a.AvailabilitySlot)
            .WithOne(s => s.Appointment)
            .HasForeignKey<Appointment>(a => a.AvailabilitySlotId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configura la relazione uno a uno con il referto medico.
        builder.HasOne(a => a.MedicalReport)
            .WithOne(r => r.Appointment)
            .HasForeignKey<MedicalReport>(r => r.AppointmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}