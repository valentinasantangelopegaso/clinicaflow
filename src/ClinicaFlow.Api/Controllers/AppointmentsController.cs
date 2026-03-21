using ClinicaFlow.Api.Application.DTOs;
using ClinicaFlow.Api.Domain.Entities;
using ClinicaFlow.Api.Domain.Enums;
using ClinicaFlow.Api.Infrastructure.Data;
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
public class AppointmentsController : ControllerBase
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
    /// Restituisce l'elenco completo degli appuntamenti.
    /// </summary>
    /// <returns>Lista degli appuntamenti con dati di paziente, medico e slot.</returns>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Recupera tutti gli appuntamenti",
        Description = "Restituisce l'elenco completo degli appuntamenti prenotati nel sistema.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Elenco degli appuntamenti recuperato correttamente.")]
    [ProducesResponseType(typeof(IEnumerable<AppointmentReadDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AppointmentReadDto>>> GetAll()
    {
        var appointments = await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .Include(a => a.AvailabilitySlot)
            .OrderBy(a => a.AvailabilitySlot.StartDateTime)
            .Select(a => new AppointmentReadDto
            {
                Id = a.Id,
                PatientId = a.PatientId,
                PatientFullName = a.Patient.FirstName + " " + a.Patient.LastName,
                DoctorId = a.DoctorId,
                DoctorFullName = a.Doctor.FirstName + " " + a.Doctor.LastName,
                AvailabilitySlotId = a.AvailabilitySlotId,
                StartTime = a.AvailabilitySlot.StartDateTime,
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
    [SwaggerOperation(
        Summary = "Recupera un appuntamento per id",
        Description = "Restituisce i dati di un singolo appuntamento.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Appuntamento recuperato correttamente.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Appuntamento non trovato.")]
    [ProducesResponseType(typeof(AppointmentReadDto), StatusCodes.Status200OK)]
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
                StartTime = a.AvailabilitySlot.StartDateTime,
                EndTime = a.AvailabilitySlot.EndDateTime,
                Status = a.Status,
                Notes = a.Notes
            })
            .FirstOrDefaultAsync();

        if (appointment is null)
        {
            return NotFound("Appuntamento non trovato.");
        }

        return Ok(appointment);
    }

    /// <summary>
    /// Crea un nuovo appuntamento prenotando uno slot disponibile.
    /// </summary>
    /// <param name="dto">Dati dell'appuntamento da creare.</param>
    /// <returns>Appuntamento appena creato.</returns>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Prenota un nuovo appuntamento",
        Description = "Crea un appuntamento associando un paziente a uno slot disponibile. Lo slot diventa automaticamente non disponibile.")]
    [SwaggerResponse(StatusCodes.Status201Created, "Appuntamento creato correttamente.")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Paziente o slot non validi.")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Lo slot selezionato non è più disponibile.")]
    [ProducesResponseType(typeof(AppointmentReadDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AppointmentReadDto>> Create([FromBody] AppointmentCreateDto dto)
    {
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
                StartTime = a.AvailabilitySlot.StartDateTime,
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
    [SwaggerOperation(
        Summary = "Aggiorna lo stato di un appuntamento",
        Description = "Aggiorna lo stato di un appuntamento. In caso di annullamento, lo slot viene nuovamente reso disponibile.")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Stato dell'appuntamento aggiornato correttamente.")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Transizione di stato non valida.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Appuntamento non trovato.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

        if (appointment.Status == AppointmentStatus.Completed && dto.Status != AppointmentStatus.Completed)
        {
            return BadRequest("Non è possibile modificare un appuntamento già completato.");
        }

        if (appointment.Status == AppointmentStatus.Cancelled && dto.Status != AppointmentStatus.Cancelled)
        {
            return BadRequest("Non è possibile riattivare un appuntamento annullato.");
        }

        if (dto.Status == AppointmentStatus.Cancelled)
        {
            appointment.Status = AppointmentStatus.Cancelled;
            appointment.AvailabilitySlot.IsAvailable = true;
        }
        else if (dto.Status == AppointmentStatus.Completed)
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
