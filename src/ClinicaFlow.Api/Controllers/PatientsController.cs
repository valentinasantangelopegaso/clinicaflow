using ClinicaFlow.Api.Application.DTOs;
using ClinicaFlow.Api.Application.Helpers;
using ClinicaFlow.Api.Controllers.Base;
using ClinicaFlow.Api.Domain.Entities;
using ClinicaFlow.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace ClinicaFlow.Api.Controllers;

/// <summary>
/// Controller per la gestione dei pazienti.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class PatientsController : AuthenticatedControllerBase
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
    /// Consentito solo al Back Office.
    /// </summary>
    /// <returns>Lista ordinata dei pazienti.</returns>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(
        Summary = "Recupera tutti i pazienti",
        Description = "Restituisce l'elenco completo dei pazienti registrati nel sistema. Endpoint riservato al ruolo Admin.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Elenco dei pazienti recuperato correttamente.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Utente non autenticato.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Utente non autorizzato.")]
    [ProducesResponseType(typeof(IEnumerable<PatientReadDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<PatientReadDto>>> GetAll()
    {
        var patients = await _context.Patients
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .Select(p => MapToReadDto(p))
            .ToListAsync();

        return Ok(patients);
    }

    /// <summary>
    /// Restituisce un paziente tramite identificativo.
    /// Il paziente autenticato può leggere solo il proprio profilo.
    /// </summary>
    /// <param name="id">Identificativo del paziente.</param>
    /// <returns>Dati del paziente richiesto.</returns>
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Patient")]
    [SwaggerOperation(
        Summary = "Recupera un paziente per id",
        Description = "Restituisce i dati di un singolo paziente. Il ruolo Patient può accedere solo al proprio record; il ruolo Admin può accedere a qualsiasi paziente.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Paziente recuperato correttamente.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Utente non autenticato.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Utente non autorizzato a leggere questo record.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Paziente non trovato.")]
    [ProducesResponseType(typeof(PatientReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PatientReadDto>> GetById(int id)
    {
        if (User.IsInRole("Patient") && GetCurrentPatientId() != id)
        {
            return Forbid();
        }

        var patient = await _context.Patients
            .Where(p => p.Id == id)
            .Select(p => MapToReadDto(p))
            .FirstOrDefaultAsync();

        if (patient is null)
        {
            return NotFound("Paziente non trovato.");
        }

        return Ok(patient);
    }

    /// <summary>
    /// Restituisce un paziente tramite codice fiscale.
    /// Endpoint mantenuto come funzione di lookup amministrativa e non come login.
    /// </summary>
    /// <param name="taxCode">Codice fiscale del paziente.</param>
    /// <returns>Dati del paziente richiesto.</returns>
    [HttpGet("by-taxcode/{taxCode}")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(
        Summary = "Recupera un paziente per codice fiscale",
        Description = "Restituisce i dati del paziente associato al codice fiscale indicato. Endpoint riservato al ruolo Admin.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Paziente recuperato correttamente.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Utente non autenticato.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Utente non autorizzato.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Paziente non trovato.")]
    [ProducesResponseType(typeof(PatientReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PatientReadDto>> GetByTaxCode(string taxCode)
    {
        var normalizedTaxCode = TaxCodeHelper.Normalize(taxCode);

        var patient = await _context.Patients
            .Where(p => p.TaxCode == normalizedTaxCode)
            .Select(p => MapToReadDto(p))
            .FirstOrDefaultAsync();

        if (patient is null)
        {
            return NotFound("Paziente non trovato.");
        }

        return Ok(patient);
    }

    /// <summary>
    /// Crea un nuovo paziente.
    /// Operazione riservata al Back Office.
    /// </summary>
    /// <param name="dto">Dati del paziente da creare.</param>
    /// <returns>Paziente appena creato.</returns>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(
        Summary = "Crea un nuovo paziente",
        Description = "Inserisce un nuovo paziente nel sistema. Endpoint riservato al ruolo Admin.")]
    [SwaggerResponse(StatusCodes.Status201Created, "Paziente creato correttamente.")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Dati di input non validi.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Utente non autenticato.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Utente non autorizzato.")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Esiste già un paziente con lo stesso codice fiscale.")]
    [ProducesResponseType(typeof(PatientReadDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
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

        var result = MapToReadDto(patient);

        return CreatedAtAction(nameof(GetById), new { id = patient.Id }, result);
    }

    /// <summary>
    /// Aggiorna un paziente esistente.
    /// Operazione riservata al Back Office.
    /// </summary>
    /// <param name="id">Identificativo del paziente da aggiornare.</param>
    /// <param name="dto">Nuovi dati del paziente.</param>
    /// <returns>Esito dell'operazione di aggiornamento.</returns>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(
        Summary = "Aggiorna un paziente",
        Description = "Aggiorna i dati anagrafici e di contatto di un paziente esistente. Endpoint riservato al ruolo Admin.")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Paziente aggiornato correttamente.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Utente non autenticato.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Utente non autorizzato.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Paziente non trovato.")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Esiste già un altro paziente con lo stesso codice fiscale.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
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

        if (normalizedTaxCode.Length != 16)
        {
            ModelState.AddModelError("TaxCode", "Il codice fiscale deve contenere 16 caratteri.");
            return ValidationProblem(ModelState);
        }

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
    /// Operazione riservata al Back Office.
    /// </summary>
    /// <param name="id">Identificativo del paziente da eliminare.</param>
    /// <returns>Esito dell'operazione di eliminazione.</returns>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(
        Summary = "Elimina un paziente",
        Description = "Elimina un paziente solo se non risulta collegato ad appuntamenti. Endpoint riservato al ruolo Admin.")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Paziente eliminato correttamente.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Utente non autenticato.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Utente non autorizzato.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Paziente non trovato.")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Il paziente è collegato ad appuntamenti.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
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

    /// <summary>
    /// Mappa un'entità Patient nel relativo DTO di lettura.
    /// </summary>
    /// <param name="patient">Entità paziente da convertire.</param>
    /// <returns>DTO di lettura del paziente.</returns>
    private static PatientReadDto MapToReadDto(Patient patient)
    {
        return new PatientReadDto
        {
            Id = patient.Id,
            FirstName = patient.FirstName,
            LastName = patient.LastName,
            TaxCode = patient.TaxCode,
            BirthDate = patient.BirthDate,
            Phone = patient.Phone,
            Email = patient.Email
        };
    }
}