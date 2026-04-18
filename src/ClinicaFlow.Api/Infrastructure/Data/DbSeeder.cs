using ClinicaFlow.Api.Domain.Entities;
using ClinicaFlow.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ClinicaFlow.Api.Infrastructure.Data;

/// <summary>
/// Seeder iniziale dei dati applicativi.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(ClinicaFlowDbContext context)
    {
        if (await context.UserAccounts.AnyAsync())
        {
            return;
        }

        var hasher = new PasswordHasher<UserAccount>();

        var admin = new UserAccount
        {
            Username = "admin",
            Role = "Admin",
            IsActive = true
        };
        admin.PasswordHash = hasher.HashPassword(admin, "Admin123!");

        context.UserAccounts.Add(admin);

        var firstDoctor = await context.Doctors.OrderBy(d => d.Id).FirstOrDefaultAsync();
        if (firstDoctor is not null)
        {
            var doctorUser = new UserAccount
            {
                Username = "doctor1",
                Role = "Doctor",
                DoctorId = firstDoctor.Id,
                IsActive = true
            };
            doctorUser.PasswordHash = hasher.HashPassword(doctorUser, "Doctor123!");
            context.UserAccounts.Add(doctorUser);
        }

        var firstPatient = await context.Patients.OrderBy(p => p.Id).FirstOrDefaultAsync();
        if (firstPatient is not null)
        {
            var patientUser = new UserAccount
            {
                Username = "patient1",
                Role = "Patient",
                PatientId = firstPatient.Id,
                IsActive = true
            };
            patientUser.PasswordHash = hasher.HashPassword(patientUser, "Patient123!");
            context.UserAccounts.Add(patientUser);
        }

        await context.SaveChangesAsync();
    }
}