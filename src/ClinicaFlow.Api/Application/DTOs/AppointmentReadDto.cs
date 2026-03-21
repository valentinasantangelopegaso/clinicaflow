using ClinicaFlow.Api.Domain.Enums;
using Swashbuckle.AspNetCore.Annotations;

namespace ClinicaFlow.Api.Application.DTOs;

/// <summary>
/// DTO utilizzato per la lettura dei dati di un appuntamento.
/// </summary>
[SwaggerSchema(Description = "DTO utilizzato per la lettura dei dati di un appuntamento.")]
public class AppointmentReadDto
{
    /// <summary>
    /// Identificativo univoco dell'appuntamento.
    /// </summary>
    [SwaggerSchema(Description = "Identificativo univoco dell'appuntamento.")]
    public int Id { get; set; }

    /// <summary>
    /// Identificativo del paziente associato all'appuntamento.
    /// </summary>
    [SwaggerSchema(Description = "Identificativo del paziente associato all'appuntamento.")]
    public int PatientId { get; set; }

    /// <summary>
    /// Nome completo del paziente associato all'appuntamento.
    /// </summary>
    [SwaggerSchema(Description = "Nome completo del paziente associato all'appuntamento.")]
    public string PatientFullName { get; set; } = string.Empty;

    /// <summary>
    /// Identificativo del medico associato all'appuntamento.
    /// </summary>
    [SwaggerSchema(Description = "Identificativo del medico associato all'appuntamento.")]
    public int DoctorId { get; set; }

    /// <summary>
    /// Nome completo del medico associato all'appuntamento.
    /// </summary>
    [SwaggerSchema(Description = "Nome completo del medico associato all'appuntamento.")]
    public string DoctorFullName { get; set; } = string.Empty;

    /// <summary>
    /// Identificativo dello slot di disponibilità associato all'appuntamento.
    /// </summary>
    [SwaggerSchema(Description = "Identificativo dello slot di disponibilità associato all'appuntamento.")]
    public int AvailabilitySlotId { get; set; }

    /// <summary>
    /// Data e ora di inizio dell'appuntamento.
    /// </summary>
    [SwaggerSchema(Description = "Data e ora di inizio dell'appuntamento.")]
    public DateTime StartTime { get; set; }

    /// <summary>
    /// Data e ora di fine dell'appuntamento.
    /// </summary>
    [SwaggerSchema(Description = "Data e ora di fine dell'appuntamento.")]
    public DateTime EndTime { get; set; }

    /// <summary>
    /// Stato corrente dell'appuntamento.
    /// </summary>
    [SwaggerSchema(Description = "Stato corrente dell'appuntamento.")]
    public AppointmentStatus Status { get; set; }

    /// <summary>
    /// Eventuali note associate all'appuntamento.
    /// </summary>
    [SwaggerSchema(Description = "Eventuali note associate all'appuntamento.", Nullable = true)]
    public string? Notes { get; set; }
}