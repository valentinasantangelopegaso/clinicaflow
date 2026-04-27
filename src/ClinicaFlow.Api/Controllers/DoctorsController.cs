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
/// Controller per la gestione dei medici.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class DoctorsController : AuthenticatedControllerBase
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
    /// Inizializza una nuova istanza del controller dei medici.
    /// </summary>
    /// <param name="context">Contesto Entity Framework dell'applicazione.</param>
    public DoctorsController(ClinicaFlowDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Restituisce l'elenco completo dei medici.
    /// Operazione riservata al Back Office.
    /// </summary>
    /// <returns>Lista ordinata dei medici con relativa specializzazione.</returns>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(
        Summary = "Recupera tutti i medici",
        Description = "Restituisce l'elenco completo dei medici con l'indicazione della specializzazione associata. Endpoint riservato al ruolo Admin.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Elenco dei medici recuperato correttamente.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Utente non autenticato.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Utente non autorizzato.")]
    [ProducesResponseType(typeof(IEnumerable<DoctorReadDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
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
                TaxCode = d.TaxCode,
                SpecialtyId = d.SpecialtyId,
                SpecialtyName = d.Specialty.Name,
                Username = _context.UserAccounts
                    .Where(u => u.DoctorId == d.Id && u.Role == "Doctor")
                    .Select(u => u.Username)
                    .FirstOrDefault()
            })
            .ToListAsync();

        return Ok(doctors);
    }

    /// <summary>
    /// Restituisce un medico tramite identificativo.
    /// Il medico autenticato può leggere solo il proprio profilo.
    /// </summary>
    /// <param name="id">Identificativo del medico.</param>
    /// <returns>Dati del medico richiesto.</returns>
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Doctor")]
    [SwaggerOperation(
        Summary = "Recupera un medico per id",
        Description = "Restituisce i dati di un singolo medico con la relativa specializzazione. Il ruolo Doctor può accedere solo al proprio record; il ruolo Admin può accedere a qualsiasi medico.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Medico recuperato correttamente.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Utente non autenticato.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Utente non autorizzato a leggere questo record.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Medico non trovato.")]
    [ProducesResponseType(typeof(DoctorReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DoctorReadDto>> GetById(int id)
    {
        if (User.IsInRole("Doctor") && GetCurrentDoctorId() != id)
        {
            return Forbid();
        }

        var doctor = await _context.Doctors
            .Include(d => d.Specialty)
            .Where(d => d.Id == id)
            .Select(d => new DoctorReadDto
            {
                Id = d.Id,
                FirstName = d.FirstName,
                LastName = d.LastName,
                TaxCode = d.TaxCode,
                SpecialtyId = d.SpecialtyId,
                SpecialtyName = d.Specialty.Name,
                Username = _context.UserAccounts
                    .Where(u => u.DoctorId == d.Id && u.Role == "Doctor")
                    .Select(u => u.Username)
                    .FirstOrDefault()
            })
            .FirstOrDefaultAsync();

        if (doctor is null)
        {
            return NotFound("Medico non trovato.");
        }

        return Ok(doctor);
    }

    /// <summary>
    /// Restituisce un medico tramite codice fiscale.
    /// Endpoint mantenuto come funzione di lookup amministrativa e non come login.
    /// </summary>
    /// <param name="taxCode">Codice fiscale del medico.</param>
    /// <returns>Dati del medico richiesto.</returns>
    [HttpGet("by-taxcode/{taxCode}")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(
        Summary = "Recupera un medico per codice fiscale",
        Description = "Restituisce i dati del medico associato al codice fiscale indicato. Endpoint riservato al ruolo Admin.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Medico recuperato correttamente.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Utente non autenticato.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Utente non autorizzato.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Medico non trovato.")]
    [ProducesResponseType(typeof(DoctorReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DoctorReadDto>> GetByTaxCode(string taxCode)
    {
        var normalizedTaxCode = TaxCodeHelper.Normalize(taxCode);

        var doctor = await _context.Doctors
            .Include(d => d.Specialty)
            .Where(d => d.TaxCode == normalizedTaxCode)
            .Select(d => new DoctorReadDto
            {
                Id = d.Id,
                FirstName = d.FirstName,
                LastName = d.LastName,
                TaxCode = d.TaxCode,
                SpecialtyId = d.SpecialtyId,
                SpecialtyName = d.Specialty.Name,
                Username = _context.UserAccounts
                    .Where(u => u.DoctorId == d.Id && u.Role == "Doctor")
                    .Select(u => u.Username)
                    .FirstOrDefault()
            })
            .FirstOrDefaultAsync();

        if (doctor is null)
        {
            return NotFound("Medico non trovato.");
        }

        return Ok(doctor);
    }

    /// <summary>
    /// Crea un nuovo medico e, se fornite le credenziali, crea anche l'account applicativo Doctor.
    /// Operazione riservata al Back Office.
    /// </summary>
    /// <param name="dto">Dati del medico da creare.</param>
    /// <returns>Medico appena creato.</returns>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(
        Summary = "Crea un nuovo medico",
        Description = "Inserisce un nuovo medico nel sistema e consente al Back Office di creare contestualmente le credenziali di accesso del medico.")]
    [SwaggerResponse(StatusCodes.Status201Created, "Medico creato correttamente.")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Specializzazione non valida o dati di input non validi.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Utente non autenticato.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Utente non autorizzato.")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Esiste già un medico o un account con gli stessi dati identificativi.")]
    [ProducesResponseType(typeof(DoctorReadDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DoctorReadDto>> Create([FromBody] DoctorCreateDto dto)
    {
        var specialtyExists = await _context.Specialties
            .AnyAsync(s => s.Id == dto.SpecialtyId);

        if (!specialtyExists)
        {
            return BadRequest("La specializzazione indicata non esiste.");
        }

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
            return BadRequest("Per creare l'account del medico sono obbligatori sia username sia password.");
        }

        var duplicateTaxCode = await _context.Doctors
            .AnyAsync(d => d.TaxCode == normalizedTaxCode);

        if (duplicateTaxCode)
        {
            return Conflict("Esiste già un medico con lo stesso codice fiscale.");
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

        var doctor = new Doctor
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            TaxCode = normalizedTaxCode,
            SpecialtyId = dto.SpecialtyId
        };

        _context.Doctors.Add(doctor);
        await _context.SaveChangesAsync();

        if (hasCredentials && normalizedUsername is not null && password is not null)
        {
            CreateDoctorAccount(doctor.Id, normalizedUsername, password);
            await _context.SaveChangesAsync();
        }

        await transaction.CommitAsync();

        var specialtyName = await _context.Specialties
            .Where(s => s.Id == doctor.SpecialtyId)
            .Select(s => s.Name)
            .FirstAsync();

        var result = MapToReadDto(doctor, specialtyName, normalizedUsername);

        return CreatedAtAction(nameof(GetById), new { id = doctor.Id }, result);
    }

    /// <summary>
    /// Aggiorna un medico esistente e consente al Back Office di aggiornare o creare l'account di accesso associato.
    /// Operazione riservata al Back Office.
    /// </summary>
    /// <param name="id">Identificativo del medico da aggiornare.</param>
    /// <param name="dto">Nuovi dati del medico.</param>
    /// <returns>Esito dell'operazione di aggiornamento.</returns>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(
        Summary = "Aggiorna un medico",
        Description = "Aggiorna i dati anagrafici e la specializzazione di un medico esistente e consente la modifica delle credenziali di accesso associate.")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Medico aggiornato correttamente.")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Specializzazione non valida o dati di input non validi.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Utente non autenticato.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Utente non autorizzato.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Medico non trovato.")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Esiste già un altro medico o account con gli stessi dati identificativi.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
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

        var normalizedTaxCode = TaxCodeHelper.Normalize(dto.TaxCode);

        if (normalizedTaxCode.Length != 16)
        {
            ModelState.AddModelError("TaxCode", "Il codice fiscale deve contenere 16 caratteri.");
            return ValidationProblem(ModelState);
        }

        var duplicateTaxCode = await _context.Doctors
            .AnyAsync(d => d.Id != id && d.TaxCode == normalizedTaxCode);

        if (duplicateTaxCode)
        {
            return Conflict("Esiste già un altro medico con lo stesso codice fiscale.");
        }

        var normalizedUsername = NormalizeUsername(dto.Username);
        var password = NormalizePassword(dto.Password);

        var existingAccount = await _context.UserAccounts
            .FirstOrDefaultAsync(u => u.DoctorId == id && u.Role == "Doctor");

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
            return BadRequest("Per creare l'account del medico sono obbligatori sia username sia password.");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        doctor.FirstName = dto.FirstName;
        doctor.LastName = dto.LastName;
        doctor.TaxCode = normalizedTaxCode;
        doctor.SpecialtyId = dto.SpecialtyId;

        UpsertDoctorAccount(id, existingAccount, normalizedUsername, password);

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return NoContent();
    }

    /// <summary>
    /// Elimina un medico se non ha slot o appuntamenti associati.
    /// Operazione riservata al Back Office.
    /// </summary>
    /// <param name="id">Identificativo del medico da eliminare.</param>
    /// <returns>Esito dell'operazione di eliminazione.</returns>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(
        Summary = "Elimina un medico",
        Description = "Elimina un medico solo se non risulta collegato a slot di disponibilità o appuntamenti. Endpoint riservato al ruolo Admin.")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Medico eliminato correttamente.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Utente non autenticato.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Utente non autorizzato.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Medico non trovato.")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Il medico è collegato a slot o appuntamenti.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
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

    /// <summary>
    /// Crea un account di tipo Doctor collegato al medico indicato.
    /// </summary>
    private void CreateDoctorAccount(int doctorId, string username, string password)
    {
        var account = new UserAccount
        {
            Username = username,
            Role = "Doctor",
            DoctorId = doctorId,
            IsActive = true
        };

        account.PasswordHash = _passwordHasher.HashPassword(account, password);
        _context.UserAccounts.Add(account);
    }

    /// <summary>
    /// Aggiorna o crea l'account applicativo collegato al medico.
    /// </summary>
    private void UpsertDoctorAccount(int doctorId, UserAccount? existingAccount, string? username, string? password)
    {
        if (existingAccount is null)
        {
            if (!HasCompleteCredentials(username, password))
            {
                return;
            }

            CreateDoctorAccount(doctorId, username!, password!);
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
    /// Mappa un'entità Doctor nel relativo DTO di lettura.
    /// </summary>
    /// <param name="doctor">Entità medico da convertire.</param>
    /// <param name="specialtyName">Nome della specializzazione associata.</param>
    /// <param name="username">Username dell'account associato, se presente.</param>
    /// <returns>DTO di lettura del medico.</returns>
    private static DoctorReadDto MapToReadDto(Doctor doctor, string specialtyName, string? username)
    {
        return new DoctorReadDto
        {
            Id = doctor.Id,
            FirstName = doctor.FirstName,
            LastName = doctor.LastName,
            TaxCode = doctor.TaxCode,
            SpecialtyId = doctor.SpecialtyId,
            SpecialtyName = specialtyName,
            Username = username
        };
    }
}
