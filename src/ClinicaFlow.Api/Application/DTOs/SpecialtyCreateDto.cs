using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace ClinicaFlow.Api.Application.DTOs;

/// <summary>
/// DTO utilizzato per la creazione di una specializzazione.
/// </summary>
[SwaggerSchema(Description = "DTO utilizzato per la creazione di una specializzazione.")]
public class SpecialtyCreateDto
{
    /// <summary>
    /// Nome della specializzazione medica.
    /// </summary>
    [Required(ErrorMessage = "Il nome della specializzazione è obbligatorio.")]
    [StringLength(100, ErrorMessage = "Il nome della specializzazione non può superare i 100 caratteri.")]
    [SwaggerSchema(Description = "Nome della specializzazione medica.", Nullable = false)]
    public string Name { get; set; } = string.Empty;
}