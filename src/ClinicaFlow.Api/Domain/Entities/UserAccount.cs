using System.ComponentModel.DataAnnotations;

namespace ClinicaFlow.Api.Domain.Entities;

/// <summary>
/// Entità che rappresenta l'account applicativo usato per l'autenticazione.
/// </summary>
public class UserAccount
{
    /// <summary>
    /// Identificativo univoco dell'account.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Nome utente utilizzato per il login.
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Password salvata in forma hashata.
    /// </summary>
    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Ruolo applicativo dell'utente.
    /// Valori previsti: Admin, Doctor, Patient.
    /// </summary>
    [Required]
    [StringLength(20)]
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Identificativo del medico associato, se il ruolo è Doctor.
    /// </summary>
    public int? DoctorId { get; set; }

    /// <summary>
    /// Medico associato all'account.
    /// </summary>
    public Doctor? Doctor { get; set; }

    /// <summary>
    /// Identificativo del paziente associato, se il ruolo è Patient.
    /// </summary>
    public int? PatientId { get; set; }

    /// <summary>
    /// Paziente associato all'account.
    /// </summary>
    public Patient? Patient { get; set; }

    /// <summary>
    /// Indica se l'account è attivo.
    /// </summary>
    public bool IsActive { get; set; } = true;
}