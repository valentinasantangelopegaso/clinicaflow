namespace ClinicaFlow.Api.Application.DTOs;

/// <summary>
/// DTO restituito dopo il login riuscito.
/// </summary>
public class AuthResponseDto
{
    /// <summary>
    /// Token JWT da usare nelle richieste successive.
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Ruolo dell'utente autenticato.
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Identificativo del medico, se presente.
    /// </summary>
    public int? DoctorId { get; set; }

    /// <summary>
    /// Identificativo del paziente, se presente.
    /// </summary>
    public int? PatientId { get; set; }
}