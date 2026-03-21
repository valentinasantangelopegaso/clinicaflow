using ClinicaFlow.Api.Application.DTOs;
using ClinicaFlow.Api.Domain.Entities;
using ClinicaFlow.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace ClinicaFlow.Api.Controllers;

/// <summary>
/// Controller per la gestione dei medici.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DoctorsController : ControllerBase
{
    /// <summary>
    /// Contesto Entity Framework utilizzato per l'accesso ai dati.
    /// </summary>
    private readonly ClinicaFlowDbContext _context;

    /// <summary>
    /// Inizializza una nuova istanza del controller dei medici.
    /// </summary>
    /// <param name="context">Contesto Entity Framework dell'applicazione.</param>
    public DoctorsController(ClinicaFlowDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Restituisce l'elenco completo dei medici.
    /// </summary>
    /// <returns>Lista ordinata dei medici con relativa specializzazione.</returns>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Recupera tutti i medici",
        Description = "Restituisce l'elenco completo dei medici con l'indicazione della specializzazione associata.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Elenco dei medici recuperato correttamente.")]
    [ProducesResponseType(typeof(IEnumerable<DoctorReadDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DoctorReadDto>>> GetAll()
    {
        var doctors = await _context.Doctors
            .Include(d => d.Specialty)
            .OrderBy(d => d.LastName)
            .ThenBy(d => d.FirstName)
            .Select(d => new DoctorReadDto
            {
                Id = d.Id,
                FirstName = d.FirstName,
                LastName = d.LastName,
                SpecialtyId = d.SpecialtyId,
                SpecialtyName = d.Specialty.Name
            })
            .ToListAsync();

        return Ok(doctors);
    }

    /// <summary>
    /// Restituisce un medico tramite identificativo.
    /// </summary>
    /// <param name="id">Identificativo del medico.</param>
    /// <returns>Dati del medico richiesto.</returns>
    [HttpGet("{id:int}")]
    [SwaggerOperation(
        Summary = "Recupera un medico per id",
        Description = "Restituisce i dati di un medico specifico con la relativa specializzazione.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Medico recuperato correttamente.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Medico non trovato.")]
    [ProducesResponseType(typeof(DoctorReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DoctorReadDto>> GetById(int id)
    {
        var doctor = await _context.Doctors
            .Include(d => d.Specialty)
            .Where(d => d.Id == id)
            .Select(d => new DoctorReadDto
            {
                Id = d.Id,
                FirstName = d.FirstName,
                LastName = d.LastName,
                SpecialtyId = d.SpecialtyId,
                SpecialtyName = d.Specialty.Name
            })
            .FirstOrDefaultAsync();

        if (doctor is null)
        {
            return NotFound("Medico non trovato.");
        }

        return Ok(doctor);
    }

    /// <summary>
    /// Crea un nuovo medico.
    /// </summary>
    /// <param name="dto">Dati del medico da creare.</param>
    /// <returns>Medico appena creato.</returns>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Crea un nuovo medico",
        Description = "Inserisce un nuovo medico nel sistema associandolo a una specializzazione esistente.")]
    [SwaggerResponse(StatusCodes.Status201Created, "Medico creato correttamente.")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Specializzazione non valida.")]
    [ProducesResponseType(typeof(DoctorReadDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DoctorReadDto>> Create([FromBody] DoctorCreateDto dto)
    {
        var specialtyExists = await _context.Specialties
            .AnyAsync(s => s.Id == dto.SpecialtyId);

        if (!specialtyExists)
        {
            return BadRequest("La specializzazione indicata non esiste.");
        }

        var doctor = new Doctor
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            SpecialtyId = dto.SpecialtyId
        };

        _context.Doctors.Add(doctor);
        await _context.SaveChangesAsync();

        var result = await _context.Doctors
            .Include(d => d.Specialty)
            .Where(d => d.Id == doctor.Id)
            .Select(d => new DoctorReadDto
            {
                Id = d.Id,
                FirstName = d.FirstName,
                LastName = d.LastName,
                SpecialtyId = d.SpecialtyId,
                SpecialtyName = d.Specialty.Name
            })
            .FirstAsync();

        return CreatedAtAction(nameof(GetById), new { id = doctor.Id }, result);
    }

    /// <summary>
    /// Aggiorna un medico esistente.
    /// </summary>
    /// <param name="id">Identificativo del medico da aggiornare.</param>
    /// <param name="dto">Nuovi dati del medico.</param>
    /// <returns>Esito dell'operazione di aggiornamento.</returns>
    [HttpPut("{id:int}")]
    [SwaggerOperation(
        Summary = "Aggiorna un medico",
        Description = "Aggiorna i dati anagrafici e la specializzazione di un medico esistente.")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Medico aggiornato correttamente.")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Specializzazione non valida.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Medico non trovato.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] DoctorCreateDto dto)
    {
        var doctor = await _context.Doctors.FindAsync(id);

        if (doctor is null)
        {
            return NotFound("Medico non trovato.");
        }

        var specialtyExists = await _context.Specialties
            .AnyAsync(s => s.Id == dto.SpecialtyId);

        if (!specialtyExists)
        {
            return BadRequest("La specializzazione indicata non esiste.");
        }

        doctor.FirstName = dto.FirstName;
        doctor.LastName = dto.LastName;
        doctor.SpecialtyId = dto.SpecialtyId;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Elimina un medico se non ha slot o appuntamenti associati.
    /// </summary>
    /// <param name="id">Identificativo del medico da eliminare.</param>
    /// <returns>Esito dell'operazione di eliminazione.</returns>
    [HttpDelete("{id:int}")]
    [SwaggerOperation(
        Summary = "Elimina un medico",
        Description = "Elimina un medico solo se non risulta collegato a slot di disponibilità o appuntamenti.")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Medico eliminato correttamente.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Medico non trovato.")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Il medico è collegato a slot o appuntamenti.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id)
    {
        var doctor = await _context.Doctors.FindAsync(id);

        if (doctor is null)
        {
            return NotFound("Medico non trovato.");
        }

        var hasSlots = await _context.AvailabilitySlots
            .AnyAsync(s => s.DoctorId == id);

        var hasAppointments = await _context.Appointments
            .AnyAsync(a => a.DoctorId == id);

        if (hasSlots || hasAppointments)
        {
            return Conflict("Non è possibile eliminare un medico collegato a slot di disponibilità o appuntamenti.");
        }

        _context.Doctors.Remove(doctor);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
