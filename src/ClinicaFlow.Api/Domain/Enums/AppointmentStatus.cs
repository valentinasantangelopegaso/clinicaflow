using Swashbuckle.AspNetCore.Annotations;

namespace ClinicaFlow.Api.Domain.Enums;

/// <summary>
/// Elenco degli stati possibili di un appuntamento nel dominio applicativo.
/// </summary>
[SwaggerSchema(Description = "Elenco degli stati possibili di un appuntamento nel dominio applicativo.")]
public enum AppointmentStatus
{
    /// <summary>
    /// Appuntamento pianificato.
    /// </summary>
    Scheduled = 0,

    /// <summary>
    /// Appuntamento completato.
    /// </summary>
    Completed = 1,

    /// <summary>
    /// Appuntamento annullato.
    /// </summary>
    Cancelled = 2
}