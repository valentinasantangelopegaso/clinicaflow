using ClinicaFlow.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicaFlow.Api.Infrastructure.Configurations;

/// <summary>
/// Configurazione Entity Framework dell'entità Doctor.
/// </summary>
public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    /// <summary>
    /// Configura la mappatura dell'entità Doctor.
    /// </summary>
    /// <param name="builder">Builder di configurazione dell'entità.</param>
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        // Configura il nome tabella.
        builder.ToTable("Doctors");

        // Configura la chiave primaria.
        builder.HasKey(x => x.Id);

        // Configura nome e cognome come obbligatori.
        builder.Property(x => x.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.LastName)
            .IsRequired()
            .HasMaxLength(100);

        // Configura il codice fiscale come obbligatorio.
        builder.Property(x => x.TaxCode)
            .IsRequired()
            .HasMaxLength(16);

        // Configura l'email come facoltativa.
        builder.Property(x => x.Email)
            .HasMaxLength(150)
            .IsRequired(false);

        // Configura indice univoco sul codice fiscale.
        builder.HasIndex(x => x.TaxCode)
            .IsUnique();

        // Configura la relazione con la specializzazione.
        builder.HasOne(x => x.Specialty)
            .WithMany(x => x.Doctors)
            .HasForeignKey(x => x.SpecialtyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}