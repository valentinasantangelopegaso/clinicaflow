using ClinicaFlow.Api.Application.DTOs;
using ClinicaFlow.Api.Domain.Entities;
using ClinicaFlow.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace ClinicaFlow.Api.Controllers;

/// <summary>
/// Controller per la gestione degli slot di disponibilità dei medici.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AvailabilitySlotsController : ControllerBase
{
    /// <summary>
    /// Contesto Entity Framework utilizzato per l'accesso ai dati.
    /// </summary>
    private readonly ClinicaFlowDbContext _context;

    /// <summary>
    /// Inizializza una nuova istanza del controller degli slot di disponibilità.
    /// </summary>
    /// <param name="context">Contesto Entity Framework dell'applicazione.</param>
    public AvailabilitySlotsController(ClinicaFlowDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Restituisce l'elenco degli slot di disponibilità, con possibilità di filtro.
    /// </summary>
    /// <param name="doctorId">Identificativo facoltativo del medico.</param>
    /// <param name="onlyAvailable">Filtro facoltativo per visualizzare solo gli slot disponibili.</param>
    /// <returns>Lista degli slot di disponibilità.</returns>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Recupera gli slot di disponibilità",
        Description = "Restituisce l'elenco degli slot di disponibilità, con possibilità di filtrare per medico o disponibilità.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Elenco degli slot recuperato correttamente.")]
    [ProducesResponseType(typeof(IEnumerable<AvailabilitySlotReadDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AvailabilitySlotReadDto>>> GetAll(
        [FromQuery] int? doctorId,
        [FromQuery] bool? onlyAvailable)
    {
        var query = _context.AvailabilitySlots
            .Include(s => s.Doctor)
            .AsQueryable();

        if (doctorId.HasValue)
        {
            query = query.Where(s => s.DoctorId == doctorId.Value);
        }

        if (onlyAvailable == true)
        {
            query = query.Where(s => s.IsAvailable);
        }

        var slots = await query
            .OrderBy(s => s.StartDateTime)
            .Select(s => new AvailabilitySlotReadDto
            {
                Id = s.Id,
                DoctorId = s.DoctorId,
                DoctorFullName = s.Doctor.FirstName + " " + s.Doctor.LastName,
                StartTime = s.StartDateTime,
                EndTime = s.EndDateTime,
                IsAvailable = s.IsAvailable
            })
            .ToListAsync();

        return Ok(slots);
    }

    /// <summary>
    /// Restituisce uno slot di disponibilità tramite identificativo.
    /// </summary>
    /// <param name="id">Identificativo dello slot.</param>
    /// <returns>Dati dello slot richiesto.</returns>
    [HttpGet("{id:int}")]
    [SwaggerOperation(
        Summary = "Recupera uno slot per id",
        Description = "Restituisce i dati di un singolo slot di disponibilità.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Slot recuperato correttamente.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Slot non trovato.")]
    [ProducesResponseType(typeof(AvailabilitySlotReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AvailabilitySlotReadDto>> GetById(int id)
    {
        var slot = await _context.AvailabilitySlots
            .Include(s => s.Doctor)
            .Where(s => s.Id == id)
            .Select(s => new AvailabilitySlotReadDto
            {
                Id = s.Id,
                DoctorId = s.DoctorId,
                DoctorFullName = s.Doctor.FirstName + " " + s.Doctor.LastName,
                StartTime = s.StartDateTime,
                EndTime = s.EndDateTime,
                IsAvailable = s.IsAvailable
            })
            .FirstOrDefaultAsync();

        if (slot is null)
        {
            return NotFound("Slot di disponibilità non trovato.");
        }

        return Ok(slot);
    }

    /// <summary>
    /// Crea un nuovo slot di disponibilità per un medico.
    /// </summary>
    /// <param name="dto">Dati dello slot da creare.</param>
    /// <returns>Slot appena creato.</returns>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Crea uno slot di disponibilità",
        Description = "Crea un nuovo slot di disponibilità per un medico, verificando che non vi siano sovrapposizioni temporali.")]
    [SwaggerResponse(StatusCodes.Status201Created, "Slot creato correttamente.")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Dati non validi o medico inesistente.")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Esiste già uno slot sovrapposto per il medico indicato.")]
    [ProducesResponseType(typeof(AvailabilitySlotReadDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AvailabilitySlotReadDto>> Create([FromBody] AvailabilitySlotCreateDto dto)
    {
        if (dto.EndTime <= dto.StartTime)
        {
            return BadRequest("L'orario di fine deve essere successivo all'orario di inizio.");
        }

        var doctorExists = await _context.Doctors
            .AnyAsync(d => d.Id == dto.DoctorId);

        if (!doctorExists)
        {
            return BadRequest("Il medico indicato non esiste.");
        }

        var overlapExists = await _context.AvailabilitySlots
            .AnyAsync(s =>
                s.DoctorId == dto.DoctorId &&
                dto.StartTime < s.EndDateTime &&
                dto.EndTime > s.StartDateTime);

        if (overlapExists)
        {
            return Conflict("Esiste già uno slot sovrapposto per il medico indicato.");
        }

        var slot = new AvailabilitySlot
        {
            DoctorId = dto.DoctorId,
            StartDateTime = dto.StartTime,
            EndDateTime = dto.EndTime,
            IsAvailable = true
        };

        _context.AvailabilitySlots.Add(slot);
        await _context.SaveChangesAsync();

        var result = await _context.AvailabilitySlots
            .Include(s => s.Doctor)
            .Where(s => s.Id == slot.Id)
            .Select(s => new AvailabilitySlotReadDto
            {
                Id = s.Id,
                DoctorId = s.DoctorId,
                DoctorFullName = s.Doctor.FirstName + " " + s.Doctor.LastName,
                StartTime = s.StartDateTime,
                EndTime = s.EndDateTime,
                IsAvailable = s.IsAvailable
            })
            .FirstAsync();

        return CreatedAtAction(nameof(GetById), new { id = slot.Id }, result);
    }

    /// <summary>
    /// Aggiorna uno slot di disponibilità esistente.
    /// </summary>
    /// <param name="id">Identificativo dello slot da aggiornare.</param>
    /// <param name="dto">Nuovi dati dello slot.</param>
    /// <returns>Esito dell'operazione di aggiornamento.</returns>
    [HttpPut("{id:int}")]
    [SwaggerOperation(
        Summary = "Aggiorna uno slot di disponibilità",
        Description = "Aggiorna uno slot esistente solo se non è già stato prenotato.")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Slot aggiornato correttamente.")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Dati non validi o medico inesistente.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Slot non trovato.")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Lo slot non può essere modificato o si sovrappone ad altri slot.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, [FromBody] AvailabilitySlotCreateDto dto)
    {
        var slot = await _context.AvailabilitySlots.FindAsync(id);

        if (slot is null)
        {
            return NotFound("Slot di disponibilità non trovato.");
        }

        if (!slot.IsAvailable)
        {
            return Conflict("Non è possibile modificare uno slot già prenotato.");
        }

        if (dto.EndTime <= dto.StartTime)
        {
            return BadRequest("L'orario di fine deve essere successivo all'orario di inizio.");
        }

        var doctorExists = await _context.Doctors
            .AnyAsync(d => d.Id == dto.DoctorId);

        if (!doctorExists)
        {
            return BadRequest("Il medico indicato non esiste.");
        }

        var overlapExists = await _context.AvailabilitySlots
            .AnyAsync(s =>
                s.Id != id &&
                s.DoctorId == dto.DoctorId &&
                dto.StartTime < s.EndDateTime &&
                dto.EndTime > s.StartDateTime);

        if (overlapExists)
        {
            return Conflict("Esiste già uno slot sovrapposto per il medico indicato.");
        }

        slot.DoctorId = dto.DoctorId;
        slot.StartDateTime = dto.StartTime;
        slot.EndDateTime = dto.EndTime;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Elimina uno slot di disponibilità se non è già stato prenotato.
    /// </summary>
    /// <param name="id">Identificativo dello slot da eliminare.</param>
    /// <returns>Esito dell'operazione di eliminazione.</returns>
    [HttpDelete("{id:int}")]
    [SwaggerOperation(
        Summary = "Elimina uno slot di disponibilità",
        Description = "Elimina uno slot solo se non è stato ancora prenotato.")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Slot eliminato correttamente.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Slot non trovato.")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Lo slot è già stato prenotato.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id)
    {
        var slot = await _context.AvailabilitySlots.FindAsync(id);

        if (slot is null)
        {
            return NotFound("Slot di disponibilità non trovato.");
        }

        if (!slot.IsAvailable)
        {
            return Conflict("Non è possibile eliminare uno slot già prenotato.");
        }

        _context.AvailabilitySlots.Remove(slot);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
