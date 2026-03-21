using Swashbuckle.AspNetCore.Annotations;

namespace ClinicaFlow.Api.Application.DTOs;

/// <summary>
/// DTO utilizzato per la lettura dei dati di un referto medico.
/// </summary>
[SwaggerSchema(Description = "DTO utilizzato per la lettura dei dati di un referto medico.")]
public class MedicalReportReadDto
{
    /// <summary>
    /// Identificativo univoco del referto medico.
    /// </summary>
    [SwaggerSchema(Description = "Identificativo univoco del referto medico.")]
    public int Id { get; set; }

    /// <summary>
    /// Identificativo dell'appuntamento associato al referto.
    /// </summary>
    [SwaggerSchema(Description = "Identificativo dell'appuntamento associato al referto.")]
    public int AppointmentId { get; set; }

    /// <summary>
    /// Diagnosi riportata nel referto.
    /// </summary>
    [SwaggerSchema(Description = "Diagnosi riportata nel referto.")]
    public string Diagnosis { get; set; } = string.Empty;

    /// <summary>
    /// Terapia prescritta nel referto.
    /// </summary>
    [SwaggerSchema(Description = "Terapia prescritta nel referto.", Nullable = true)]
    public string? Therapy { get; set; }

    /// <summary>
    /// Note aggiuntive del referto.
    /// </summary>
    [SwaggerSchema(Description = "Note aggiuntive del referto.", Nullable = true)]
    public string? Notes { get; set; }

    /// <summary>
    /// Data di creazione del referto.
    /// </summary>
    [SwaggerSchema(Description = "Data di creazione del referto.")]
    public DateTime CreatedAt { get; set; }
}