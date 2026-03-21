using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace ClinicaFlow.Api.DTOs;

/// <summary>
/// DTO utilizzato per la creazione di un referto medico.
/// </summary>
[SwaggerSchema(Description = "DTO utilizzato per la creazione di un referto medico.")]
public class MedicalReportCreateDto
{
    /// <summary>
    /// Identificativo dell'appuntamento a cui è associato il referto.
    /// </summary>
    [Required(ErrorMessage = "L'identificativo dell'appuntamento è obbligatorio.")]
    [SwaggerSchema(Description = "Identificativo dell'appuntamento a cui è associato il referto.")]
    public int AppointmentId { get; set; }

    /// <summary>
    /// Diagnosi riportata nel referto.
    /// </summary>
    [Required(ErrorMessage = "La diagnosi è obbligatoria.")]
    [StringLength(1000, ErrorMessage = "La diagnosi non può superare i 1000 caratteri.")]
    [SwaggerSchema(Description = "Diagnosi riportata nel referto.", Nullable = false)]
    public string Diagnosis { get; set; } = string.Empty;

    /// <summary>
    /// Terapia prescritta nel referto.
    /// </summary>
    [StringLength(1000, ErrorMessage = "La terapia non può superare i 1000 caratteri.")]
    [SwaggerSchema(Description = "Terapia prescritta nel referto.", Nullable = true)]
    public string? Therapy { get; set; }

    /// <summary>
    /// Note aggiuntive del referto.
    /// </summary>
    [StringLength(2000, ErrorMessage = "Le note non possono superare i 2000 caratteri.")]
    [SwaggerSchema(Description = "Note aggiuntive del referto.", Nullable = true)]
    public string? Notes { get; set; }
}