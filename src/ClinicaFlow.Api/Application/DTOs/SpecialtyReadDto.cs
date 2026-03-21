using Swashbuckle.AspNetCore.Annotations;

namespace ClinicaFlow.Api.DTOs;

/// <summary>
/// DTO utilizzato per la lettura dei dati di una specializzazione.
/// </summary>
[SwaggerSchema(Description = "DTO utilizzato per la lettura dei dati di una specializzazione.")]
public class SpecialtyReadDto
{
    /// <summary>
    /// Identificativo univoco della specializzazione.
    /// </summary>
    [SwaggerSchema(Description = "Identificativo univoco della specializzazione.")]
    public int Id { get; set; }

    /// <summary>
    /// Nome della specializzazione medica.
    /// </summary>
    [SwaggerSchema(Description = "Nome della specializzazione medica.")]
    public string Name { get; set; } = string.Empty;
}