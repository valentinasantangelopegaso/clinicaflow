using ClinicaFlow.Api.Application.DTOs;
using ClinicaFlow.Api.Domain.Entities;
using ClinicaFlow.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace ClinicaFlow.Api.Controllers;

/// <summary>
/// Controller per la gestione delle specializzazioni mediche.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class SpecialtiesController : ControllerBase
{
    /// <summary>
    /// Contesto Entity Framework utilizzato per l'accesso ai dati.
    /// </summary>
    private readonly ClinicaFlowDbContext _context;

    /// <summary>
    /// Inizializza una nuova istanza del controller delle specializzazioni.
    /// </summary>
    /// <param name="context">Contesto Entity Framework dell'applicazione.</param>
    public SpecialtiesController(ClinicaFlowDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Restituisce l'elenco completo delle specializzazioni.
    /// </summary>
    /// <returns>Lista ordinata delle specializzazioni.</returns>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Recupera tutte le specializzazioni",
        Description = "Restituisce l'elenco completo delle specializzazioni mediche presenti nel sistema.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Elenco delle specializzazioni recuperato correttamente.")]
    [ProducesResponseType(typeof(IEnumerable<SpecialtyReadDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SpecialtyReadDto>>> GetAll()
    {
        var specialties = await _context.Specialties
            .OrderBy(s => s.Name)
            .Select(s => new SpecialtyReadDto
            {
                Id = s.Id,
                Name = s.Name
            })
            .ToListAsync();

        return Ok(specialties);
    }

    /// <summary>
    /// Restituisce una singola specializzazione tramite identificativo.
    /// </summary>
    /// <param name="id">Identificativo della specializzazione.</param>
    /// <returns>Dati della specializzazione richiesta.</returns>
    [HttpGet("{id:int}")]
    [SwaggerOperation(
        Summary = "Recupera una specializzazione per id",
        Description = "Restituisce i dati di una singola specializzazione medica.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Specializzazione recuperata correttamente.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Specializzazione non trovata.")]
    [ProducesResponseType(typeof(SpecialtyReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SpecialtyReadDto>> GetById(int id)
    {
        var specialty = await _context.Specialties
            .Where(s => s.Id == id)
            .Select(s => new SpecialtyReadDto
            {
                Id = s.Id,
                Name = s.Name
            })
            .FirstOrDefaultAsync();

        if (specialty is null)
        {
            return NotFound("Specializzazione non trovata.");
        }

        return Ok(specialty);
    }

    /// <summary>
    /// Crea una nuova specializzazione.
    /// </summary>
    /// <param name="dto">Dati della specializzazione da creare.</param>
    /// <returns>Specializzazione appena creata.</returns>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Crea una nuova specializzazione",
        Description = "Inserisce una nuova specializzazione medica nel sistema.")]
    [SwaggerResponse(StatusCodes.Status201Created, "Specializzazione creata correttamente.")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Dati di input non validi.")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Esiste già una specializzazione con lo stesso nome.")]
    [ProducesResponseType(typeof(SpecialtyReadDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SpecialtyReadDto>> Create([FromBody] SpecialtyCreateDto dto)
    {
        var exists = await _context.Specialties
            .AnyAsync(s => s.Name == dto.Name);

        if (exists)
        {
            return Conflict("Esiste già una specializzazione con lo stesso nome.");
        }

        var specialty = new Specialty
        {
            Name = dto.Name
        };

        _context.Specialties.Add(specialty);
        await _context.SaveChangesAsync();

        var result = new SpecialtyReadDto
        {
            Id = specialty.Id,
            Name = specialty.Name
        };

        return CreatedAtAction(nameof(GetById), new { id = specialty.Id }, result);
    }

    /// <summary>
    /// Aggiorna una specializzazione esistente.
    /// </summary>
    /// <param name="id">Identificativo della specializzazione da aggiornare.</param>
    /// <param name="dto">Nuovi dati della specializzazione.</param>
    /// <returns>Esito dell'operazione di aggiornamento.</returns>
    [HttpPut("{id:int}")]
    [SwaggerOperation(
        Summary = "Aggiorna una specializzazione",
        Description = "Aggiorna i dati di una specializzazione esistente.")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Specializzazione aggiornata correttamente.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Specializzazione non trovata.")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Esiste già una specializzazione con lo stesso nome.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, [FromBody] SpecialtyCreateDto dto)
    {
        var specialty = await _context.Specialties.FindAsync(id);

        if (specialty is null)
        {
            return NotFound("Specializzazione non trovata.");
        }

        var duplicateExists = await _context.Specialties
            .AnyAsync(s => s.Id != id && s.Name == dto.Name);

        if (duplicateExists)
        {
            return Conflict("Esiste già una specializzazione con lo stesso nome.");
        }

        specialty.Name = dto.Name;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Elimina una specializzazione se non è collegata ad alcun medico.
    /// </summary>
    /// <param name="id">Identificativo della specializzazione da eliminare.</param>
    /// <returns>Esito dell'operazione di eliminazione.</returns>
    [HttpDelete("{id:int}")]
    [SwaggerOperation(
        Summary = "Elimina una specializzazione",
        Description = "Elimina una specializzazione solo se non risulta associata ad alcun medico.")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Specializzazione eliminata correttamente.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Specializzazione non trovata.")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "La specializzazione è associata a uno o più medici.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id)
    {
        var specialty = await _context.Specialties.FindAsync(id);

        if (specialty is null)
        {
            return NotFound("Specializzazione non trovata.");
        }

        var hasDoctors = await _context.Doctors
            .AnyAsync(d => d.SpecialtyId == id);

        if (hasDoctors)
        {
            return Conflict("Non è possibile eliminare una specializzazione associata a uno o più medici.");
        }

        _context.Specialties.Remove(specialty);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
