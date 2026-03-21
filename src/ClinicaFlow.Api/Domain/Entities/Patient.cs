using System.ComponentModel.DataAnnotations;

namespace ClinicaFlow.Api.Domain.Entities;

/// <summary>
/// Entità che rappresenta un paziente della clinica.
/// </summary>
public class Patient
{
    /// <summary>
    /// Identificativo univoco del paziente.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Nome del paziente.
    /// </summary>
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Cognome del paziente.
    /// </summary>
    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Codice fiscale del paziente.
    /// </summary>
    [Required]
    [StringLength(16)]
    public string TaxCode { get; set; } = string.Empty;

    /// <summary>
    /// Data di nascita del paziente.
    /// </summary>
    public DateTime BirthDate { get; set; }

    /// <summary>
    /// Numero di telefono del paziente.
    /// </summary>
    [StringLength(30)]
    public string? Phone { get; set; }

    /// <summary>
    /// Indirizzo email del paziente.
    /// </summary>
    [StringLength(150)]
    public string? Email { get; set; }

    /// <summary>
    /// Elenco degli appuntamenti associati al paziente.
    /// </summary>
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}