using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicaFlow.Api.Domain.Entities;

/// <summary>
/// Configurazione Entity Framework dell'entità Patient.
/// </summary>
public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    /// <summary>
    /// Configura la mappatura dell'entità Patient.
    /// </summary>
    /// <param name="builder">Builder di configurazione dell'entità.</param>
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        // Configura la chiave primaria.
        builder.HasKey(p => p.Id);

        // Configura il nome come obbligatorio.
        builder.Property(p => p.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        // Configura il cognome come obbligatorio.
        builder.Property(p => p.LastName)
            .HasMaxLength(100)
            .IsRequired();

        // Configura il codice fiscale come obbligatorio.
        builder.Property(p => p.TaxCode)
            .HasMaxLength(16)
            .IsRequired();

        // Configura il telefono come facoltativo.
        builder.Property(p => p.Phone)
            .HasMaxLength(30)
            .IsRequired(false);

        // Configura l'email come facoltativa.
        builder.Property(p => p.Email)
            .HasMaxLength(150)
            .IsRequired(false);

        // Configura indice univoco sul codice fiscale.
        builder.HasIndex(p => p.TaxCode)
            .IsUnique();
    }
}