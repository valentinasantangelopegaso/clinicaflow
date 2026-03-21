using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace ClinicaFlow.Api.DTOs;

/// <summary>
/// DTO utilizzato per la creazione di un paziente.
/// </summary>
[SwaggerSchema(Description = "DTO utilizzato per la creazione di un paziente.")]
public class PatientCreateDto
{
    /// <summary>
    /// Nome del paziente.
    /// </summary>
    [Required(ErrorMessage = "Il nome del paziente è obbligatorio.")]
    [StringLength(100, ErrorMessage = "Il nome del paziente non può superare i 100 caratteri.")]
    [SwaggerSchema(Description = "Nome del paziente.", Nullable = false)]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Cognome del paziente.
    /// </summary>
    [Required(ErrorMessage = "Il cognome del paziente è obbligatorio.")]
    [StringLength(100, ErrorMessage = "Il cognome del paziente non può superare i 100 caratteri.")]
    [SwaggerSchema(Description = "Cognome del paziente.", Nullable = false)]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Data di nascita del paziente.
    /// </summary>
    [Required(ErrorMessage = "La data di nascita del paziente è obbligatoria.")]
    [DataType(DataType.Date)]
    [SwaggerSchema(Description = "Data di nascita del paziente.")]
    public DateTime DateOfBirth { get; set; }

    /// <summary>
    /// Numero di telefono del paziente.
    /// </summary>
    [Phone(ErrorMessage = "Il numero di telefono non è valido.")]
    [StringLength(30, ErrorMessage = "Il numero di telefono non può superare i 30 caratteri.")]
    [SwaggerSchema(Description = "Numero di telefono del paziente.", Nullable = true)]
    public string? Phone { get; set; }

    /// <summary>
    /// Indirizzo email del paziente.
    /// </summary>
    [EmailAddress(ErrorMessage = "L'indirizzo email non è valido.")]
    [StringLength(150, ErrorMessage = "L'indirizzo email non può superare i 150 caratteri.")]
    [SwaggerSchema(Description = "Indirizzo email del paziente.", Nullable = true)]
    public string? Email { get; set; }
}