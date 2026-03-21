using Swashbuckle.AspNetCore.Annotations;

namespace ClinicaFlow.Api.DTOs;

/// <summary>
/// DTO utilizzato per la lettura dei dati di uno slot di disponibilità.
/// </summary>
[SwaggerSchema(Description = "DTO utilizzato per la lettura dei dati di uno slot di disponibilità.")]
public class AvailabilitySlotReadDto
{
    /// <summary>
    /// Identificativo univoco dello slot.
    /// </summary>
    [SwaggerSchema(Description = "Identificativo univoco dello slot.")]
    public int Id { get; set; }

    /// <summary>
    /// Identificativo del medico associato allo slot.
    /// </summary>
    [SwaggerSchema(Description = "Identificativo del medico associato allo slot.")]
    public int DoctorId { get; set; }

    /// <summary>
    /// Nome completo del medico associato allo slot.
    /// </summary>
    [SwaggerSchema(Description = "Nome completo del medico associato allo slot.")]
    public string DoctorFullName { get; set; } = string.Empty;

    /// <summary>
    /// Data e ora di inizio dello slot.
    /// </summary>
    [SwaggerSchema(Description = "Data e ora di inizio dello slot.")]
    public DateTime StartTime { get; set; }

    /// <summary>
    /// Data e ora di fine dello slot.
    /// </summary>
    [SwaggerSchema(Description = "Data e ora di fine dello slot.")]
    public DateTime EndTime { get; set; }

    /// <summary>
    /// Indica se lo slot è ancora disponibile per la prenotazione.
    /// </summary>
    [SwaggerSchema(Description = "Indica se lo slot è ancora disponibile per la prenotazione.")]
    public bool IsAvailable { get; set; }
}