using ClinicaFlow.Api.Application.DTOs;
using ClinicaFlow.Api.Application.Helpers;
using ClinicaFlow.Api.Controllers.Base;
using ClinicaFlow.Api.Domain.Entities;
using ClinicaFlow.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
    /// Componente utilizzato per salvare le password in forma hashata.
    /// </summary>
    private readonly PasswordHasher<UserAccount> _passwordHasher = new();

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
            .Select(p => new PatientReadDto
            {
                Id = p.Id,
                FirstName = p.FirstName,
                LastName = p.LastName,
                TaxCode = p.TaxCode,
                BirthDate = p.BirthDate,
                Phone = p.Phone,
                Email = p.Email,
                Username = _context.UserAccounts
                    .Where(u => u.PatientId == p.Id && u.Role == "Patient")
                    .Select(u => u.Username)
                    .FirstOrDefault()
            })
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
            .Select(p => new PatientReadDto
            {
                Id = p.Id,
                FirstName = p.FirstName,
                LastName = p.LastName,
                TaxCode = p.TaxCode,
                BirthDate = p.BirthDate,
                Phone = p.Phone,
                Email = p.Email,
                Username = _context.UserAccounts
                    .Where(u => u.PatientId == p.Id && u.Role == "Patient")
                    .Select(u => u.Username)
                    .FirstOrDefault()
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
            .Select(p => new PatientReadDto
            {
                Id = p.Id,
                FirstName = p.FirstName,
                LastName = p.LastName,
                TaxCode = p.TaxCode,
                BirthDate = p.BirthDate,
                Phone = p.Phone,
                Email = p.Email,
                Username = _context.UserAccounts
                    .Where(u => u.PatientId == p.Id && u.Role == "Patient")
                    .Select(u => u.Username)
                    .FirstOrDefault()
            })
            .FirstOrDefaultAsync();

        if (patient is null)
        {
            return NotFound("Paziente non trovato.");
        }

        return Ok(patient);
    }

    /// <summary>
    /// Crea un nuovo paziente e, se fornite le credenziali, crea anche l'account applicativo Patient.
    /// Operazione riservata al Back Office.
    /// </summary>
    /// <param name="dto">Dati del paziente da creare.</param>
    /// <returns>Paziente appena creato.</returns>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(
        Summary = "Crea un nuovo paziente",
        Description = "Inserisce un nuovo paziente nel sistema e consente al Back Office di creare contestualmente le credenziali di accesso del paziente.")]
    [SwaggerResponse(StatusCodes.Status201Created, "Paziente creato correttamente.")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Dati di input non validi.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Utente non autenticato.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Utente non autorizzato.")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Esiste già un paziente o un account con gli stessi dati identificativi.")]
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

        var normalizedUsername = NormalizeUsername(dto.Username);
        var password = NormalizePassword(dto.Password);
        var hasCredentials = HasCredentialInput(normalizedUsername, password);

        if (hasCredentials && !HasCompleteCredentials(normalizedUsername, password))
        {
            return BadRequest("Per creare l'account del paziente sono obbligatori sia username sia password.");
        }

        var taxCodeExists = await _context.Patients
            .AnyAsync(p => p.TaxCode == normalizedTaxCode);

        if (taxCodeExists)
        {
            return Conflict("Esiste già un paziente con lo stesso codice fiscale.");
        }

        if (!string.IsNullOrWhiteSpace(normalizedUsername))
        {
            var usernameExists = await _context.UserAccounts
                .AnyAsync(u => u.Username == normalizedUsername);

            if (usernameExists)
            {
                return Conflict("Esiste già un account con lo stesso username.");
            }
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

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

        if (hasCredentials && normalizedUsername is not null && password is not null)
        {
            CreatePatientAccount(patient.Id, normalizedUsername, password);
            await _context.SaveChangesAsync();
        }

        await transaction.CommitAsync();

        var result = MapToReadDto(patient, normalizedUsername);

        return CreatedAtAction(nameof(GetById), new { id = patient.Id }, result);
    }

    /// <summary>
    /// Aggiorna un paziente esistente e consente al Back Office di aggiornare o creare l'account di accesso associato.
    /// Operazione riservata al Back Office.
    /// </summary>
    /// <param name="id">Identificativo del paziente da aggiornare.</param>
    /// <param name="dto">Nuovi dati del paziente.</param>
    /// <returns>Esito dell'operazione di aggiornamento.</returns>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(
        Summary = "Aggiorna un paziente",
        Description = "Aggiorna i dati anagrafici e di contatto di un paziente esistente e consente la modifica delle credenziali di accesso associate.")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Paziente aggiornato correttamente.")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Dati di input non validi.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Utente non autenticato.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Utente non autorizzato.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Paziente non trovato.")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Esiste già un altro paziente o account con gli stessi dati identificativi.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

        var normalizedUsername = NormalizeUsername(dto.Username);
        var password = NormalizePassword(dto.Password);

        var existingAccount = await _context.UserAccounts
            .FirstOrDefaultAsync(u => u.PatientId == id && u.Role == "Patient");

        if (!string.IsNullOrWhiteSpace(normalizedUsername))
        {
            var duplicateUsername = await _context.UserAccounts
                .AnyAsync(u => u.Id != (existingAccount == null ? 0 : existingAccount.Id) && u.Username == normalizedUsername);

            if (duplicateUsername)
            {
                return Conflict("Esiste già un account con lo stesso username.");
            }
        }

        if (existingAccount is null && HasCredentialInput(normalizedUsername, password) && !HasCompleteCredentials(normalizedUsername, password))
        {
            return BadRequest("Per creare l'account del paziente sono obbligatori sia username sia password.");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        patient.FirstName = dto.FirstName;
        patient.LastName = dto.LastName;
        patient.TaxCode = normalizedTaxCode;
        patient.BirthDate = dto.BirthDate;
        patient.Phone = dto.Phone;
        patient.Email = dto.Email;

        UpsertPatientAccount(id, existingAccount, normalizedUsername, password);

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

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
    /// Crea un account di tipo Patient collegato al paziente indicato.
    /// </summary>
    private void CreatePatientAccount(int patientId, string username, string password)
    {
        var account = new UserAccount
        {
            Username = username,
            Role = "Patient",
            PatientId = patientId,
            IsActive = true
        };

        account.PasswordHash = _passwordHasher.HashPassword(account, password);
        _context.UserAccounts.Add(account);
    }

    /// <summary>
    /// Aggiorna o crea l'account applicativo collegato al paziente.
    /// </summary>
    private void UpsertPatientAccount(int patientId, UserAccount? existingAccount, string? username, string? password)
    {
        if (existingAccount is null)
        {
            if (!HasCompleteCredentials(username, password))
            {
                return;
            }

            CreatePatientAccount(patientId, username!, password!);
            return;
        }

        if (!string.IsNullOrWhiteSpace(username))
        {
            existingAccount.Username = username;
        }

        if (!string.IsNullOrWhiteSpace(password))
        {
            existingAccount.PasswordHash = _passwordHasher.HashPassword(existingAccount, password);
        }

        existingAccount.IsActive = true;
    }

    /// <summary>
    /// Normalizza lo username rimuovendo gli spazi non significativi.
    /// </summary>
    private static string? NormalizeUsername(string? username)
    {
        return string.IsNullOrWhiteSpace(username) ? null : username.Trim();
    }

    /// <summary>
    /// Normalizza la password solo per verificare la presenza del valore.
    /// </summary>
    private static string? NormalizePassword(string? password)
    {
        return string.IsNullOrWhiteSpace(password) ? null : password;
    }

    /// <summary>
    /// Verifica se almeno uno tra username e password è stato valorizzato.
    /// </summary>
    private static bool HasCredentialInput(string? username, string? password)
    {
        return !string.IsNullOrWhiteSpace(username) || !string.IsNullOrWhiteSpace(password);
    }

    /// <summary>
    /// Verifica se username e password sono entrambi valorizzati.
    /// </summary>
    private static bool HasCompleteCredentials(string? username, string? password)
    {
        return !string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password);
    }

    /// <summary>
    /// Mappa un'entità Patient nel relativo DTO di lettura.
    /// </summary>
    /// <param name="patient">Entità paziente da convertire.</param>
    /// <param name="username">Username dell'account associato, se presente.</param>
    /// <returns>DTO di lettura del paziente.</returns>
    private static PatientReadDto MapToReadDto(Patient patient, string? username)
    {
        return new PatientReadDto
        {
            Id = patient.Id,
            FirstName = patient.FirstName,
            LastName = patient.LastName,
            TaxCode = patient.TaxCode,
            BirthDate = patient.BirthDate,
            Phone = patient.Phone,
            Email = patient.Email,
            Username = username
        };
    }
}
