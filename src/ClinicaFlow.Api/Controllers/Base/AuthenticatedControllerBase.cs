using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace ClinicaFlow.Api.Controllers.Base;

/// <summary>
/// Classe base per i controller che richiedono accesso ai claim dell'utente autenticato.
/// </summary>
public abstract class AuthenticatedControllerBase : ControllerBase
{
    /// <summary>
    /// Recupera l'identificativo del medico dai claim del token JWT.
    /// Restituisce null se il claim non è presente o non è valido.
    /// </summary>
    /// <returns>Id del medico autenticato, se presente.</returns>
    protected int? GetCurrentDoctorId()
    {
        var value = User.FindFirst("doctorId")?.Value;

        if (int.TryParse(value, out var doctorId))
        {
            return doctorId;
        }

        return null;
    }

    /// <summary>
    /// Recupera l'identificativo del paziente dai claim del token JWT.
    /// Restituisce null se il claim non è presente o non è valido.
    /// </summary>
    /// <returns>Id del paziente autenticato, se presente.</returns>
    protected int? GetCurrentPatientId()
    {
        var value = User.FindFirst("patientId")?.Value;

        if (int.TryParse(value, out var patientId))
        {
            return patientId;
        }

        return null;
    }

    /// <summary>
    /// Verifica se l'utente autenticato appartiene al ruolo Admin.
    /// Helper opzionale ma utile nei controlli di autorizzazione applicativa.
    /// </summary>
    /// <returns>True se l'utente è Admin, altrimenti false.</returns>
    protected bool IsAdmin()
    {
        return User.IsInRole("Admin");
    }
}