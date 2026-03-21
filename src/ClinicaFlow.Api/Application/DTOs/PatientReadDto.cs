using Swashbuckle.AspNetCore.Annotations;

namespace ClinicaFlow.Api.Application.DTOs;

/// <summary>
/// DTO utilizzato per la lettura dei dati di un paziente.
/// </summary>
[SwaggerSchema(Description = "DTO utilizzato per la lettura dei dati di un paziente.")]
public class PatientReadDto
{
    /// <summary>
    /// Identificativo univoco del paziente.
    /// </summary>
    [SwaggerSchema(Description = "Identificativo univoco del paziente.")]
    public int Id { get; set; }

    /// <summary>
    /// Nome del paziente.
    /// </summary>
    [SwaggerSchema(Description = "Nome del paziente.")]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Cognome del paziente.
    /// </summary>
    [SwaggerSchema(Description = "Cognome del paziente.")]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Data di nascita del paziente.
    /// </summary>
    [SwaggerSchema(Description = "Data di nascita del paziente.")]
    public DateTime DateOfBirth { get; set; }

    /// <summary>
    /// Numero di telefono del paziente.
    /// </summary>
    [SwaggerSchema(Description = "Numero di telefono del paziente.", Nullable = true)]
    public string? Phone { get; set; }

    /// <summary>
    /// Indirizzo email del paziente.
    /// </summary>
    [SwaggerSchema(Description = "Indirizzo email del paziente.", Nullable = true)]
    public string? Email { get; set; }
}