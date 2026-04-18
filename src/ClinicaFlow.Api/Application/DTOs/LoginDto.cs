using System.ComponentModel.DataAnnotations;

namespace ClinicaFlow.Api.Application.DTOs;

/// <summary>
/// DTO utilizzato per il login applicativo.
/// </summary>
public class LoginDto
{
    /// <summary>
    /// Username dell'utente.
    /// </summary>
    [Required]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Password in chiaro inviata dal client.
    /// </summary>
    [Required]
    public string Password { get; set; } = string.Empty;
}