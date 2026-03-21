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
/// Controller per la gestione dei referti medici.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class MedicalReportsController : ControllerBase
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
    /// </summary>
    /// <returns>Lista dei referti presenti nel sistema.</returns>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Recupera tutti i referti medici",
        Description = "Restituisce l'elenco completo dei referti medici presenti nel sistema.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Elenco dei referti recuperato correttamente.")]
    [ProducesResponseType(typeof(IEnumerable<MedicalReportReadDto>), StatusCodes.Status200OK)]
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
    /// </summary>
    /// <param name="id">Identificativo del referto.</param>
    /// <returns>Dati del referto richiesto.</returns>
    [HttpGet("{id:int}")]
    [SwaggerOperation(
        Summary = "Recupera un referto per id",
        Description = "Restituisce i dati di un singolo referto medico.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Referto recuperato correttamente.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Referto non trovato.")]
    [ProducesResponseType(typeof(MedicalReportReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MedicalReportReadDto>> GetById(int id)
    {
        var report = await _context.MedicalReports
            .Where(r => r.Id == id)
            .Select(r => new MedicalReportReadDto
            {
                Id = r.Id,
                AppointmentId = r.AppointmentId,
                Diagnosis = r.Diagnosis,
                Therapy = r.Therapy,
                Notes = r.Notes,
                CreatedAt = r.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (report is null)
        {
            return NotFound("Referto medico non trovato.");
        }

        return Ok(report);
    }

    /// <summary>
    /// Restituisce il referto associato a uno specifico appuntamento.
    /// </summary>
    /// <param name="appointmentId">Identificativo dell'appuntamento.</param>
    /// <returns>Referto associato all'appuntamento.</returns>
    [HttpGet("by-appointment/{appointmentId:int}")]
    [SwaggerOperation(
        Summary = "Recupera il referto per appuntamento",
        Description = "Restituisce il referto medico associato a uno specifico appuntamento.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Referto recuperato correttamente.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Nessun referto trovato per l'appuntamento indicato.")]
    [ProducesResponseType(typeof(MedicalReportReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MedicalReportReadDto>> GetByAppointment(int appointmentId)
    {
        var report = await _context.MedicalReports
            .Where(r => r.AppointmentId == appointmentId)
            .Select(r => new MedicalReportReadDto
            {
                Id = r.Id,
                AppointmentId = r.AppointmentId,
                Diagnosis = r.Diagnosis,
                Therapy = r.Therapy,
                Notes = r.Notes,
                CreatedAt = r.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (report is null)
        {
            return NotFound("Nessun referto trovato per l'appuntamento indicato.");
        }

        return Ok(report);
    }

    /// <summary>
    /// Crea un nuovo referto medico per un appuntamento completato.
    /// </summary>
    /// <param name="dto">Dati del referto da creare.</param>
    /// <returns>Referto appena creato.</returns>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Crea un nuovo referto medico",
        Description = "Crea un referto medico solo se l'appuntamento associato risulta completato e non ha già un referto.")]
    [SwaggerResponse(StatusCodes.Status201Created, "Referto creato correttamente.")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Appuntamento non valido o non completato.")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Esiste già un referto associato all'appuntamento.")]
    [ProducesResponseType(typeof(MedicalReportReadDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<MedicalReportReadDto>> Create([FromBody] MedicalReportCreateDto dto)
    {
        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == dto.AppointmentId);

        if (appointment is null)
        {
            return BadRequest("L'appuntamento indicato non esiste.");
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
    /// </summary>
    /// <param name="id">Identificativo del referto da aggiornare.</param>
    /// <param name="dto">Nuovi dati del referto.</param>
    /// <returns>Esito dell'operazione di aggiornamento.</returns>
    [HttpPut("{id:int}")]
    [SwaggerOperation(
        Summary = "Aggiorna un referto medico",
        Description = "Aggiorna i contenuti di un referto esistente senza consentire la modifica dell'appuntamento associato.")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Referto aggiornato correttamente.")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Non è consentito modificare l'appuntamento associato.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Referto non trovato.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] MedicalReportCreateDto dto)
    {
        var report = await _context.MedicalReports.FindAsync(id);

        if (report is null)
        {
            return NotFound("Referto medico non trovato.");
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
