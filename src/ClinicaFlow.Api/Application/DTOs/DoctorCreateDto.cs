using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace ClinicaFlow.Api.Application.DTOs;

/// <summary>
/// DTO utilizzato per la creazione di un medico.
/// </summary>
[SwaggerSchema(Description = "DTO utilizzato per la creazione di un medico.")]
public class DoctorCreateDto
{
    /// <summary>
    /// Nome del medico.
    /// </summary>
    [Required(ErrorMessage = "Il nome del medico è obbligatorio.")]
    [StringLength(100, ErrorMessage = "Il nome del medico non può superare i 100 caratteri.")]
    [SwaggerSchema(Description = "Nome del medico.", Nullable = false)]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Cognome del medico.
    /// </summary>
    [Required(ErrorMessage = "Il cognome del medico è obbligatorio.")]
    [StringLength(100, ErrorMessage = "Il cognome del medico non può superare i 100 caratteri.")]
    [SwaggerSchema(Description = "Cognome del medico.", Nullable = false)]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Codice fiscale del medico.
    /// </summary>
    [Required(ErrorMessage = "Il codice fiscale del medico è obbligatorio.")]
    [StringLength(16, MinimumLength = 16, ErrorMessage = "Il codice fiscale deve contenere 16 caratteri.")]
    [SwaggerSchema(Description = "Codice fiscale del medico.", Nullable = false)]
    public string TaxCode { get; set; } = string.Empty;

    /// <summary>
    /// Identificativo della specializzazione associata al medico.
    /// </summary>
    [Required(ErrorMessage = "La specializzazione del medico è obbligatoria.")]
    [SwaggerSchema(Description = "Identificativo della specializzazione associata al medico.")]
    public int SpecialtyId { get; set; }
}