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
/// Controller per la gestione dei referti medici.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class MedicalReportsController : AuthenticatedControllerBase
{
    /// <summary>
    /// Contesto Entity Framework utilizzato per l'accesso ai dati.
    /// </summary>
    private readonly ClinicaFlowDbContext _context;

    /// <summary>
    /// Inizializza una nuova istanza del controller dei referti medici.
    /// </summary>
    /// <param name="context">Contesto Entity Framework dell'applicazione.</param>
    public MedicalReportsController(ClinicaFlowDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Restituisce l'elenco completo dei referti medici.
    /// Operazione riservata al Back Office.
    /// </summary>
    /// <returns>Lista dei referti presenti nel sistema.</returns>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(
        Summary = "Recupera tutti i referti medici",
        Description = "Restituisce l'elenco completo dei referti medici presenti nel sistema. Endpoint riservato al ruolo Admin.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Elenco dei referti recuperato correttamente.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Utente non autenticato.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Utente non autorizzato.")]
    [ProducesResponseType(typeof(IEnumerable<MedicalReportReadDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<MedicalReportReadDto>>> GetAll()
    {
        var reports = await _context.MedicalReports
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new MedicalReportReadDto
            {
                Id = r.Id,
                AppointmentId = r.AppointmentId,
                Diagnosis = r.Diagnosis,
                Therapy = r.Therapy,
                Notes = r.Notes,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        return Ok(reports);
    }

    /// <summary>
    /// Restituisce un referto medico tramite identificativo.
    /// Admin può vedere qualsiasi referto.
    /// Doctor può vedere solo i referti dei propri appuntamenti.
    /// Patient può vedere solo i referti dei propri appuntamenti.
    /// </summary>
    /// <param name="id">Identificativo del referto.</param>
    /// <returns>Dati del referto richiesto.</returns>
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Doctor,Patient")]
    [SwaggerOperation(
        Summary = "Recupera un referto per id",
        Description = "Restituisce i dati di un singolo referto medico. Admin può accedere a qualsiasi referto, Doctor solo ai referti dei propri appuntamenti, Patient solo ai referti dei propri appuntamenti.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Referto recuperato correttamente.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Utente non autenticato.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Utente non autorizzato a leggere questo referto.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Referto non trovato.")]
    [ProducesResponseType(typeof(MedicalReportReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MedicalReportReadDto>> GetById(int id)
    {
        var reportData = await _context.MedicalReports
            .Include(r => r.Appointment)
            .Where(r => r.Id == id)
            .Select(r => new
            {
                Report = new MedicalReportReadDto
                {
                    Id = r.Id,
                    AppointmentId = r.AppointmentId,
                    Diagnosis = r.Diagnosis,
                    Therapy = r.Therapy,
                    Notes = r.Notes,
                    CreatedAt = r.CreatedAt
                },
                DoctorId = r.Appointment.DoctorId,
                PatientId = r.Appointment.PatientId
            })
            .FirstOrDefaultAsync();

        if (reportData is null)
        {
            return NotFound("Referto medico non trovato.");
        }

        if (User.IsInRole("Doctor") && GetCurrentDoctorId() != reportData.DoctorId)
        {
            return Forbid();
        }

        if (User.IsInRole("Patient") && GetCurrentPatientId() != reportData.PatientId)
        {
            return Forbid();
        }

        return Ok(reportData.Report);
    }

    /// <summary>
    /// Restituisce il referto associato a uno specifico appuntamento.
    /// </summary>
    /// <param name="appointmentId">Identificativo dell'appuntamento.</param>
    /// <returns>Referto associato all'appuntamento.</returns>
    [HttpGet("by-appointment/{appointmentId:int}")]
    [Authorize(Roles = "Admin,Doctor,Patient")]
    [SwaggerOperation(
        Summary = "Recupera il referto per appuntamento",
        Description = "Restituisce il referto medico associato a uno specifico appuntamento. Admin può accedere a qualsiasi referto, Doctor solo ai referti dei propri appuntamenti, Patient solo ai referti dei propri appuntamenti.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Referto recuperato correttamente.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Utente non autenticato.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Utente non autorizzato a leggere questo referto.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Nessun referto trovato per l'appuntamento indicato.")]
    [ProducesResponseType(typeof(MedicalReportReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MedicalReportReadDto>> GetByAppointment(int appointmentId)
    {
        var reportData = await _context.MedicalReports
            .Include(r => r.Appointment)
            .Where(r => r.AppointmentId == appointmentId)
            .Select(r => new
            {
                Report = new MedicalReportReadDto
                {
                    Id = r.Id,
                    AppointmentId = r.AppointmentId,
                    Diagnosis = r.Diagnosis,
                    Therapy = r.Therapy,
                    Notes = r.Notes,
                    CreatedAt = r.CreatedAt
                },
                DoctorId = r.Appointment.DoctorId,
                PatientId = r.Appointment.PatientId
            })
            .FirstOrDefaultAsync();

        if (reportData is null)
        {
            return NotFound("Nessun referto trovato per l'appuntamento indicato.");
        }

        if (User.IsInRole("Doctor") && GetCurrentDoctorId() != reportData.DoctorId)
        {
            return Forbid();
        }

        if (User.IsInRole("Patient") && GetCurrentPatientId() != reportData.PatientId)
        {
            return Forbid();
        }

        return Ok(reportData.Report);
    }

    /// <summary>
    /// Crea un nuovo referto medico per un appuntamento completato.
    /// Admin può creare referti per qualsiasi appuntamento.
    /// Doctor può creare referti solo per i propri appuntamenti completati.
    /// </summary>
    /// <param name="dto">Dati del referto da creare.</param>
    /// <returns>Referto appena creato.</returns>
    [HttpPost]
    [Authorize(Roles = "Admin,Doctor")]
    [SwaggerOperation(
        Summary = "Crea un nuovo referto medico",
        Description = "Crea un referto medico solo se l'appuntamento associato risulta completato e non ha già un referto. Il ruolo Doctor può creare referti solo per i propri appuntamenti.")]
    [SwaggerResponse(StatusCodes.Status201Created, "Referto creato correttamente.")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Appuntamento non valido o non completato.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Utente non autenticato.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Utente non autorizzato a creare un referto per questo appuntamento.")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Esiste già un referto associato all'appuntamento.")]
    [ProducesResponseType(typeof(MedicalReportReadDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<MedicalReportReadDto>> Create([FromBody] MedicalReportCreateDto dto)
    {
        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == dto.AppointmentId);

        if (appointment is null)
        {
            return BadRequest("L'appuntamento indicato non esiste.");
        }

        if (User.IsInRole("Doctor") && GetCurrentDoctorId() != appointment.DoctorId)
        {
            return Forbid();
        }

        if (appointment.Status != AppointmentStatus.Completed)
        {
            return BadRequest("È possibile creare un referto solo per un appuntamento completato.");
        }

        var reportAlreadyExists = await _context.MedicalReports
            .AnyAsync(r => r.AppointmentId == dto.AppointmentId);

        if (reportAlreadyExists)
        {
            return Conflict("Esiste già un referto associato a questo appuntamento.");
        }

        var report = new MedicalReport
        {
            AppointmentId = dto.AppointmentId,
            Diagnosis = dto.Diagnosis,
            Therapy = dto.Therapy,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow
        };

        _context.MedicalReports.Add(report);
        await _context.SaveChangesAsync();

        var result = new MedicalReportReadDto
        {
            Id = report.Id,
            AppointmentId = report.AppointmentId,
            Diagnosis = report.Diagnosis,
            Therapy = report.Therapy,
            Notes = report.Notes,
            CreatedAt = report.CreatedAt
        };

        return CreatedAtAction(nameof(GetById), new { id = report.Id }, result);
    }

    /// <summary>
    /// Aggiorna un referto medico esistente.
    /// Admin può aggiornare qualsiasi referto.
    /// Doctor può aggiornare solo i referti dei propri appuntamenti.
    /// Non è consentito modificare l'appuntamento associato.
    /// </summary>
    /// <param name="id">Identificativo del referto da aggiornare.</param>
    /// <param name="dto">Nuovi dati del referto.</param>
    /// <returns>Esito dell'operazione di aggiornamento.</returns>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Doctor")]
    [SwaggerOperation(
        Summary = "Aggiorna un referto medico",
        Description = "Aggiorna i contenuti di un referto esistente senza consentire la modifica dell'appuntamento associato. Il ruolo Doctor può aggiornare solo i referti relativi ai propri appuntamenti.")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Referto aggiornato correttamente.")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Non è consentito modificare l'appuntamento associato.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Utente non autenticato.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Utente non autorizzato a modificare questo referto.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Referto non trovato.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] MedicalReportCreateDto dto)
    {
        var report = await _context.MedicalReports
            .Include(r => r.Appointment)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (report is null)
        {
            return NotFound("Referto medico non trovato.");
        }

        if (User.IsInRole("Doctor") && GetCurrentDoctorId() != report.Appointment.DoctorId)
        {
            return Forbid();
        }

        if (report.AppointmentId != dto.AppointmentId)
        {
            return BadRequest("Non è consentito modificare l'appuntamento associato al referto.");
        }

        report.Diagnosis = dto.Diagnosis;
        report.Therapy = dto.Therapy;
        report.Notes = dto.Notes;

        await _context.SaveChangesAsync();

        return NoContent();
    }
}