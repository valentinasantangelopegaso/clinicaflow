using Swashbuckle.AspNetCore.Annotations;

namespace ClinicaFlow.Api.Application.DTOs;

/// <summary>
/// DTO utilizzato per la lettura dei dati di un medico.
/// </summary>
[SwaggerSchema(Description = "DTO utilizzato per la lettura dei dati di un medico.")]
public class DoctorReadDto
{
    /// <summary>
    /// Identificativo univoco del medico.
    /// </summary>
    [SwaggerSchema(Description = "Identificativo univoco del medico.")]
    public int Id { get; set; }

    /// <summary>
    /// Nome del medico.
    /// </summary>
    [SwaggerSchema(Description = "Nome del medico.")]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Cognome del medico.
    /// </summary>
    [SwaggerSchema(Description = "Cognome del medico.")]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Codice fiscale del medico.
    /// </summary>
    [SwaggerSchema(Description = "Codice fiscale del medico.")]
    public string TaxCode { get; set; } = string.Empty;

    /// <summary>
    /// Identificativo della specializzazione associata.
    /// </summary>
    [SwaggerSchema(Description = "Identificativo della specializzazione associata.")]
    public int SpecialtyId { get; set; }

    /// <summary>
    /// Nome della specializzazione del medico.
    /// </summary>
    [SwaggerSchema(Description = "Nome della specializzazione del medico.")]
    public string SpecialtyName { get; set; } = string.Empty;

    /// <summary>
    /// Username dell'account applicativo associato al medico.
    /// </summary>
    [SwaggerSchema(Description = "Username dell'account applicativo associato al medico.", Nullable = true)]
    public string? Username { get; set; }
}
