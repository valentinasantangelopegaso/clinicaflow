using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ClinicaFlow.Api.Application.DTOs;
using ClinicaFlow.Api.Controllers.Base;
using ClinicaFlow.Api.Domain.Entities;
using ClinicaFlow.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ClinicaFlow.Api.Controllers
{
    /// <summary>
    /// Controller che gestisce l'autenticazione degli utenti e la generazione dei token JWT.
    /// Espone un endpoint di login che accetta le credenziali, verifica la password
    /// hashata e restituisce un token con i claim del ruolo e degli identificativi legati al dominio.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : AuthenticatedControllerBase
    {
        private readonly ClinicaFlowDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly PasswordHasher<UserAccount> _passwordHasher;

        public AuthController(ClinicaFlowDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _passwordHasher = new PasswordHasher<UserAccount>();
        }

        /// <summary>
        /// Endpoint di login che autentica l'utente e genera un token JWT.
        /// </summary>
        /// <param name="dto">Le credenziali dell'utente.</param>
        /// <returns>Un oggetto con il token e le informazioni di ruolo/id associate.</returns>
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest("Credenziali non valide.");
            }

            var user = await _context.UserAccounts.FirstOrDefaultAsync(u => u.Username == dto.Username);
            if (user == null)
            {
                return Unauthorized("Username o password errati.");
            }

            var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
            if (verificationResult != PasswordVerificationResult.Success)
            {
                return Unauthorized("Username o password errati.");
            }

            var secretKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("La chiave JWT non è configurata.");

            var issuer = _configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("L'issuer JWT non è configurato.");


            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Username),
                new(ClaimTypes.Role, user.Role)
            };

            if (user.DoctorId.HasValue)
            {
                claims.Add(new Claim("doctorId", user.DoctorId.Value.ToString()));
            }
            if (user.PatientId.HasValue)
            {
                claims.Add(new Claim("patientId", user.PatientId.Value.ToString()));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(2),
                Issuer = issuer,
                SigningCredentials = credentials
            };
            
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            var response = new AuthResponseDto
            {
                Token = tokenString,
                Role = user.Role,
                DoctorId = user.DoctorId,
                PatientId = user.PatientId
            };

            return Ok(response);
        }
    }
}