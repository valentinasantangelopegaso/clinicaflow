using System.ComponentModel.DataAnnotations;
using ClinicaFlow.Api.Domain.Enums;
using Swashbuckle.AspNetCore.Annotations;

namespace ClinicaFlow.Api.Application.DTOs;

/// <summary>
/// DTO utilizzato per l'aggiornamento dello stato di un appuntamento.
/// </summary>
[SwaggerSchema(Description = "DTO utilizzato per l'aggiornamento dello stato di un appuntamento.")]
public class AppointmentStatusUpdateDto
{
    /// <summary>
    /// Nuovo stato dell'appuntamento.
    /// </summary>
    [Required(ErrorMessage = "Lo stato dell'appuntamento è obbligatorio.")]
    [SwaggerSchema(Description = "Nuovo stato dell'appuntamento.")]
    public AppointmentStatus Status { get; set; }
}