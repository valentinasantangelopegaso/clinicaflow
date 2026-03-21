using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace ClinicaFlow.Api.Application.DTOs;

/// <summary>
/// DTO utilizzato per la creazione di un appuntamento.
/// </summary>
[SwaggerSchema(Description = "DTO utilizzato per la creazione di un appuntamento.")]
public class AppointmentCreateDto
{
    /// <summary>
    /// Identificativo del paziente che effettua la prenotazione.
    /// </summary>
    [Required(ErrorMessage = "L'identificativo del paziente è obbligatorio.")]
    [SwaggerSchema(Description = "Identificativo del paziente che effettua la prenotazione.")]
    public int PatientId { get; set; }

    /// <summary>
    /// Identificativo dello slot di disponibilità da prenotare.
    /// </summary>
    [Required(ErrorMessage = "L'identificativo dello slot è obbligatorio.")]
    [SwaggerSchema(Description = "Identificativo dello slot di disponibilità da prenotare.")]
    public int AvailabilitySlotId { get; set; }

    /// <summary>
    /// Eventuali note associate all'appuntamento.
    /// </summary>
    [StringLength(500, ErrorMessage = "Le note non possono superare i 500 caratteri.")]
    [SwaggerSchema(Description = "Eventuali note associate all'appuntamento.", Nullable = true)]
    public string? Notes { get; set; }
}