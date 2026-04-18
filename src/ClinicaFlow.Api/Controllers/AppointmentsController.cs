using ClinicaFlow.Api.Application.DTOs;
using ClinicaFlow.Api.Controllers.Base;
using ClinicaFlow.Api.Domain.Entities;
using ClinicaFlow.Api.Domain.Enums;
using ClinicaFlow.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace ClinicaFlow.Api.Controllers;

/// <summary>
/// Controller per la gestione degli appuntamenti.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class AppointmentsController : AuthenticatedControllerBase
{
    /// <summary>
    /// Contesto Entity Framework utilizzato per l'accesso ai dati.
    /// </summary>
    private readonly ClinicaFlowDbContext _context;

    /// <summary>
    /// Inizializza una nuova istanza del controller degli appuntamenti.
    /// </summary>
    /// <param name="context">Contesto Entity Framework dell'applicazione.</param>
    public AppointmentsController(ClinicaFlowDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Restituisce l'elenco degli appuntamenti visibili all'utente autenticato.
    /// </summary>
    /// <returns>Lista degli appuntamenti filtrata in base al ruolo.</returns>
    [HttpGet]
    [Authorize(Roles = "Admin,Doctor,Patient")]
    [SwaggerOperation(
        Summary = "Recupera gli appuntamenti",
        Description = "Restituisce gli appuntamenti visibili all'utente autenticato. Admin vede tutti gli appuntamenti, Doctor vede solo i propri, Patient vede solo i propri.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Elenco degli appuntamenti recuperato correttamente.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Utente non autenticato.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Utente non autorizzato.")]
    [ProducesResponseType(typeof(IEnumerable<AppointmentReadDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<AppointmentReadDto>>> GetAll()
    {
        var query = _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .Include(a => a.AvailabilitySlot)
            .AsQueryable();

        if (User.IsInRole("Doctor"))
        {
            var currentDoctorId = GetCurrentDoctorId();

            if (!currentDoctorId.HasValue)
            {
                return Forbid();
            }

            query = query.Where(a => a.DoctorId == currentDoctorId.Value);
        }
        else if (User.IsInRole("Patient"))
        {
            var currentPatientId = GetCurrentPatientId();

            if (!currentPatientId.HasValue)
            {
                return Forbid();
            }

            query = query.Where(a => a.PatientId == currentPatientId.Value);
        }

        var appointments = await query
            .OrderBy(a => a.AvailabilitySlot.StartDateTime)
            .Select(a => new AppointmentReadDto
            {
                Id = a.Id,
                PatientId = a.PatientId,
                PatientFullName = a.Patient.FirstName + " " + a.Patient.LastName,
                DoctorId = a.DoctorId,
                DoctorFullName = a.Doctor.FirstName + " " + a.Doctor.LastName,
                AvailabilitySlotId = a.AvailabilitySlotId,
                StartTime = a.AppointmentDate,
                EndTime = a.AvailabilitySlot.EndDateTime,
                Status = a.Status,
                Notes = a.Notes
            })
            .ToListAsync();

        return Ok(appointments);
    }

    /// <summary>
    /// Restituisce un appuntamento tramite identificativo.
    /// </summary>
    /// <param name="id">Identificativo dell'appuntamento.</param>
    /// <returns>Dati dell'appuntamento richiesto.</returns>
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Doctor,Patient")]
    [SwaggerOperation(
        Summary = "Recupera un appuntamento per id",
        Description = "Restituisce i dati di un singolo appuntamento. Admin può accedere a qualsiasi record, Doctor solo ai propri appuntamenti, Patient solo ai propri appuntamenti.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Appuntamento recuperato correttamente.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Utente non autenticato.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Utente non autorizzato a leggere questo appuntamento.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Appuntamento non trovato.")]
    [ProducesResponseType(typeof(AppointmentReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AppointmentReadDto>> GetById(int id)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .Include(a => a.AvailabilitySlot)
            .Where(a => a.Id == id)
            .Select(a => new AppointmentReadDto
            {
                Id = a.Id,
                PatientId = a.PatientId,
                PatientFullName = a.Patient.FirstName + " " + a.Patient.LastName,
                DoctorId = a.DoctorId,
                DoctorFullName = a.Doctor.FirstName + " " + a.Doctor.LastName,
                AvailabilitySlotId = a.AvailabilitySlotId,
                StartTime = a.AppointmentDate,
                EndTime = a.AvailabilitySlot.EndDateTime,
                Status = a.Status,
                Notes = a.Notes
            })
            .FirstOrDefaultAsync();

        if (appointment is null)
        {
            return NotFound("Appuntamento non trovato.");
        }

        if (User.IsInRole("Doctor") && GetCurrentDoctorId() != appointment.DoctorId)
        {
            return Forbid();
        }

        if (User.IsInRole("Patient") && GetCurrentPatientId() != appointment.PatientId)
        {
            return Forbid();
        }

        return Ok(appointment);
    }

    /// <summary>
    /// Crea un nuovo appuntamento prenotando uno slot disponibile.
    /// </summary>
    /// <param name="dto">Dati dell'appuntamento da creare.</param>
    /// <returns>Appuntamento appena creato.</returns>
    [HttpPost]
    [Authorize(Roles = "Admin,Patient")]
    [SwaggerOperation(
        Summary = "Prenota un nuovo appuntamento",
        Description = "Crea un appuntamento associando un paziente a uno slot disponibile. Lo slot diventa automaticamente non disponibile. Il ruolo Patient può prenotare solo per se stesso.")]
    [SwaggerResponse(StatusCodes.Status201Created, "Appuntamento creato correttamente.")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Paziente o slot non validi.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Utente non autenticato.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Utente non autorizzato a prenotare per un altro paziente.")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Lo slot selezionato non è più disponibile.")]
    [ProducesResponseType(typeof(AppointmentReadDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AppointmentReadDto>> Create([FromBody] AppointmentCreateDto dto)
    {
        if (User.IsInRole("Patient") && GetCurrentPatientId() != dto.PatientId)
        {
            return Forbid();
        }

        var patientExists = await _context.Patients
            .AnyAsync(p => p.Id == dto.PatientId);

        if (!patientExists)
        {
            return BadRequest("Il paziente indicato non esiste.");
        }

        var slot = await _context.AvailabilitySlots
            .FirstOrDefaultAsync(s => s.Id == dto.AvailabilitySlotId);

        if (slot is null)
        {
            return BadRequest("Lo slot indicato non esiste.");
        }

        if (!slot.IsAvailable)
        {
            return Conflict("Lo slot selezionato non è più disponibile.");
        }

        var appointment = new Appointment
        {
            PatientId = dto.PatientId,
            DoctorId = slot.DoctorId,
            AvailabilitySlotId = slot.Id,
            AppointmentDate = slot.StartDateTime,
            Status = AppointmentStatus.Scheduled,
            Notes = dto.Notes
        };

        slot.IsAvailable = false;

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

        var result = await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .Include(a => a.AvailabilitySlot)
            .Where(a => a.Id == appointment.Id)
            .Select(a => new AppointmentReadDto
            {
                Id = a.Id,
                PatientId = a.PatientId,
                PatientFullName = a.Patient.FirstName + " " + a.Patient.LastName,
                DoctorId = a.DoctorId,
                DoctorFullName = a.Doctor.FirstName + " " + a.Doctor.LastName,
                AvailabilitySlotId = a.AvailabilitySlotId,
                StartTime = a.AppointmentDate,
                EndTime = a.AvailabilitySlot.EndDateTime,
                Status = a.Status,
                Notes = a.Notes
            })
            .FirstAsync();

        return CreatedAtAction(nameof(GetById), new { id = appointment.Id }, result);
    }

    /// <summary>
    /// Aggiorna lo stato di un appuntamento esistente.
    /// </summary>
    /// <param name="id">Identificativo dell'appuntamento da aggiornare.</param>
    /// <param name="dto">Nuovo stato dell'appuntamento.</param>
    /// <returns>Esito dell'operazione di aggiornamento.</returns>
    [HttpPut("{id:int}/status")]
    [Authorize(Roles = "Admin,Doctor,Patient")]
    [SwaggerOperation(
        Summary = "Aggiorna lo stato di un appuntamento",
        Description = "Aggiorna lo stato di un appuntamento. Admin può aggiornare qualsiasi appuntamento; Doctor può aggiornare solo i propri; Patient può soltanto annullare i propri appuntamenti non completati.")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Stato dell'appuntamento aggiornato correttamente.")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Transizione di stato non valida.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Utente non autenticato.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Utente non autorizzato a modificare questo appuntamento.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Appuntamento non trovato.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] AppointmentStatusUpdateDto dto)
    {
        var appointment = await _context.Appointments
            .Include(a => a.AvailabilitySlot)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appointment is null)
        {
            return NotFound("Appuntamento non trovato.");
        }

        if (User.IsInRole("Doctor") && GetCurrentDoctorId() != appointment.DoctorId)
        {
            return Forbid();
        }

        if (User.IsInRole("Patient"))
        {
            if (GetCurrentPatientId() != appointment.PatientId)
            {
                return Forbid();
            }

            if (dto.Status != AppointmentStatus.Cancelled)
            {
                return BadRequest("Il paziente può solo annullare i propri appuntamenti.");
            }
        }

        if (appointment.Status == AppointmentStatus.Completed && dto.Status != AppointmentStatus.Completed)
        {
            return BadRequest("Non è possibile modificare un appuntamento già completato.");
        }

        if (appointment.Status == AppointmentStatus.Cancelled)
        {
            return BadRequest("Non è possibile modificare un appuntamento già annullato.");
        }

        if (dto.Status == AppointmentStatus.Cancelled)
        {
            appointment.AvailabilitySlot.IsAvailable = true;
            _context.Appointments.Remove(appointment);

            await _context.SaveChangesAsync();
            return NoContent();
        }

        if (dto.Status == AppointmentStatus.Completed)
        {
            appointment.Status = AppointmentStatus.Completed;
        }
        else
        {
            appointment.Status = AppointmentStatus.Scheduled;
        }

        await _context.SaveChangesAsync();

        return NoContent();
    }
}