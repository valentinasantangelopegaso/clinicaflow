using System.ComponentModel.DataAnnotations;

namespace ClinicaFlow.Api.Domain.Entities;

/// <summary>
/// Entità che rappresenta un medico della clinica.
/// </summary>
public class Doctor
{
    /// <summary>
    /// Identificativo univoco del medico.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Nome del medico.
    /// </summary>
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Cognome del medico.
    /// </summary>
    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Codice fiscale del medico.
    /// </summary>
    [Required]
    [StringLength(16)]
    public string TaxCode { get; set; } = string.Empty;

    /// <summary>
    /// Indirizzo email del medico.
    /// </summary>
    [StringLength(150)]
    public string? Email { get; set; }

    /// <summary>
    /// Identificativo della specializzazione associata.
    /// </summary>
    public int SpecialtyId { get; set; }

    /// <summary>
    /// Specializzazione associata al medico.
    /// </summary>
    public Specialty Specialty { get; set; } = null!;

    /// <summary>
    /// Slot di disponibilità del medico.
    /// </summary>
    public ICollection<AvailabilitySlot> AvailabilitySlots { get; set; } = new List<AvailabilitySlot>();

    /// <summary>
    /// Appuntamenti associati al medico.
    /// </summary>
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}