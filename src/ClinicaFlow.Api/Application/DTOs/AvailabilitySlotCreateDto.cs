using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace ClinicaFlow.Api.Application.DTOs;

/// <summary>
/// DTO utilizzato per la creazione di uno slot di disponibilità.
/// </summary>
[SwaggerSchema(Description = "DTO utilizzato per la creazione di uno slot di disponibilità.")]
public class AvailabilitySlotCreateDto
{
    /// <summary>
    /// Identificativo del medico associato allo slot.
    /// </summary>
    [Required(ErrorMessage = "L'identificativo del medico è obbligatorio.")]
    [SwaggerSchema(Description = "Identificativo del medico associato allo slot.")]
    public int DoctorId { get; set; }

    /// <summary>
    /// Data e ora di inizio dello slot.
    /// </summary>
    [Required(ErrorMessage = "La data e ora di inizio dello slot sono obbligatorie.")]
    [SwaggerSchema(Description = "Data e ora di inizio dello slot.")]
    public DateTime StartTime { get; set; }

    /// <summary>
    /// Data e ora di fine dello slot.
    /// </summary>
    [Required(ErrorMessage = "La data e ora di fine dello slot sono obbligatorie.")]
    [SwaggerSchema(Description = "Data e ora di fine dello slot.")]
    public DateTime EndTime { get; set; }
}