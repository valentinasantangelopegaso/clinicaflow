using ClinicaFlow.Api.Application.DTOs;
using ClinicaFlow.Api.Domain.Entities;
using ClinicaFlow.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using ClinicaFlow.Api.Application.Helpers;
namespace ClinicaFlow.Api.Controllers;

/// <summary>
/// Controller per la gestione dei pazienti.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PatientsController : ControllerBase
{
    /// <summary>
    /// Contesto Entity Framework utilizzato per l'accesso ai dati.
    /// </summary>
    private readonly ClinicaFlowDbContext _context;

    /// <summary>
    /// Inizializza una nuova istanza del controller dei pazienti.
    /// </summary>
    /// <param name="context">Contesto Entity Framework dell'applicazione.</param>
    public PatientsController(ClinicaFlowDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Restituisce l'elenco completo dei pazienti.
    /// </summary>
    /// <returns>Lista ordinata dei pazienti.</returns>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Recupera tutti i pazienti",
        Description = "Restituisce l'elenco completo dei pazienti registrati nel sistema.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Elenco dei pazienti recuperato correttamente.")]
    [ProducesResponseType(typeof(IEnumerable<PatientReadDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PatientReadDto>>> GetAll()
    {
        var patients = await _context.Patients
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .Select(p => new PatientReadDto
            {
                Id = p.Id,
                FirstName = p.FirstName,
                LastName = p.LastName,
                TaxCode = p.TaxCode,
                BirthDate = p.BirthDate,
                Phone = p.Phone,
                Email = p.Email
            })
            .ToListAsync();

        return Ok(patients);
    }

    /// <summary>
    /// Restituisce un paziente tramite identificativo.
    /// </summary>
    /// <param name="id">Identificativo del paziente.</param>
    /// <returns>Dati del paziente richiesto.</returns>
    [HttpGet("{id:int}")]
    [SwaggerOperation(
        Summary = "Recupera un paziente per id",
        Description = "Restituisce i dati di un singolo paziente.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Paziente recuperato correttamente.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Paziente non trovato.")]
    [ProducesResponseType(typeof(PatientReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PatientReadDto>> GetById(int id)
    {
        var patient = await _context.Patients
            .Where(p => p.Id == id)
            .Select(p => new PatientReadDto
            {
                Id = p.Id,
                FirstName = p.FirstName,
                LastName = p.LastName,
                TaxCode = p.TaxCode,
                BirthDate = p.BirthDate,
                Phone = p.Phone,
                Email = p.Email
            })
            .FirstOrDefaultAsync();

        if (patient is null)
        {
            return NotFound("Paziente non trovato.");
        }

        return Ok(patient);
    }

    /// <summary>
    /// Restituisce un paziente tramite codice fiscale.
    /// </summary>
    /// <param name="taxCode">Codice fiscale del paziente.</param>
    /// <returns>Dati del paziente richiesto.</returns>
    [HttpGet("by-taxcode/{taxCode}")]
    [SwaggerOperation(
        Summary = "Recupera un paziente per codice fiscale",
        Description = "Restituisce i dati del paziente associato al codice fiscale indicato. Questo endpoint è usato per l'accesso simulato all'area paziente.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Paziente recuperato correttamente.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Paziente non trovato.")]
    [ProducesResponseType(typeof(PatientReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PatientReadDto>> GetByTaxCode(string taxCode)
    {
        var normalizedTaxCode = TaxCodeHelper.Normalize(taxCode);

        var patient = await _context.Patients
            .Where(p => p.TaxCode == normalizedTaxCode)
            .Select(p => new PatientReadDto
            {
                Id = p.Id,
                FirstName = p.FirstName,
                LastName = p.LastName,
                TaxCode = p.TaxCode,
                BirthDate = p.BirthDate,
                Phone = p.Phone,
                Email = p.Email
            })
            .FirstOrDefaultAsync();

        if (patient is null)
        {
            return NotFound("Paziente non trovato.");
        }

        return Ok(patient);
    }
    /// <summary>
    /// Crea un nuovo paziente.
    /// </summary>
    /// <param name="dto">Dati del paziente da creare.</param>
    /// <returns>Paziente appena creato.</returns>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Crea un nuovo paziente",
        Description = "Inserisce un nuovo paziente nel sistema.")]
    [SwaggerResponse(StatusCodes.Status201Created, "Paziente creato correttamente.")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Dati di input non validi.")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Esiste già un paziente con lo stesso codice fiscale.")]
    [ProducesResponseType(typeof(PatientReadDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PatientReadDto>> Create([FromBody] PatientCreateDto dto)
    {
        var normalizedTaxCode = TaxCodeHelper.Normalize(dto.TaxCode);

        if (normalizedTaxCode.Length != 16)
        {
            ModelState.AddModelError("TaxCode", "Il codice fiscale deve contenere 16 caratteri.");
            return ValidationProblem(ModelState);
        }

        var taxCodeExists = await _context.Patients
            .AnyAsync(p => p.TaxCode == normalizedTaxCode);

        if (taxCodeExists)
        {
            return Conflict("Esiste già un paziente con lo stesso codice fiscale.");
        }

        var patient = new Patient
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            TaxCode = normalizedTaxCode,
            BirthDate = dto.BirthDate,
            Phone = dto.Phone,
            Email = dto.Email
        };

        _context.Patients.Add(patient);
        await _context.SaveChangesAsync();

        var result = new PatientReadDto
        {
            Id = patient.Id,
            FirstName = patient.FirstName,
            LastName = patient.LastName,
            TaxCode = patient.TaxCode,
            BirthDate = patient.BirthDate,
            Phone = patient.Phone,
            Email = patient.Email
        };

        return CreatedAtAction(nameof(GetById), new { id = patient.Id }, result);
    }
    /// <summary>
    /// Aggiorna un paziente esistente.
    /// </summary>
    /// <param name="id">Identificativo del paziente da aggiornare.</param>
    /// <param name="dto">Nuovi dati del paziente.</param>
    /// <returns>Esito dell'operazione di aggiornamento.</returns>
    [HttpPut("{id:int}")]
    [SwaggerOperation(
        Summary = "Aggiorna un paziente",
        Description = "Aggiorna i dati anagrafici e di contatto di un paziente esistente.")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Paziente aggiornato correttamente.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Paziente non trovato.")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Esiste già un altro paziente con lo stesso codice fiscale.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, [FromBody] PatientCreateDto dto)
    {
        var patient = await _context.Patients.FindAsync(id);

        if (patient is null)
        {
            return NotFound("Paziente non trovato.");
        }

        var normalizedTaxCode = TaxCodeHelper.Normalize(dto.TaxCode);

        var duplicateTaxCode = await _context.Patients
            .AnyAsync(p => p.Id != id && p.TaxCode == normalizedTaxCode);

        if (duplicateTaxCode)
        {
            return Conflict("Esiste già un altro paziente con lo stesso codice fiscale.");
        }

        patient.FirstName = dto.FirstName;
        patient.LastName = dto.LastName;
        patient.TaxCode = normalizedTaxCode;
        patient.BirthDate = dto.BirthDate;
        patient.Phone = dto.Phone;
        patient.Email = dto.Email;

        await _context.SaveChangesAsync();

        return NoContent();
    }
    /// <summary>
    /// Elimina un paziente se non ha appuntamenti associati.
    /// </summary>
    /// <param name="id">Identificativo del paziente da eliminare.</param>
    /// <returns>Esito dell'operazione di eliminazione.</returns>
    [HttpDelete("{id:int}")]
    [SwaggerOperation(
        Summary = "Elimina un paziente",
        Description = "Elimina un paziente solo se non risulta collegato ad appuntamenti.")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Paziente eliminato correttamente.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Paziente non trovato.")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Il paziente è collegato ad appuntamenti.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id)
    {
        var patient = await _context.Patients.FindAsync(id);

        if (patient is null)
        {
            return NotFound("Paziente non trovato.");
        }

        var hasAppointments = await _context.Appointments
            .AnyAsync(a => a.PatientId == id);

        if (hasAppointments)
        {
            return Conflict("Non è possibile eliminare un paziente collegato ad appuntamenti.");
        }

        _context.Patients.Remove(patient);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}