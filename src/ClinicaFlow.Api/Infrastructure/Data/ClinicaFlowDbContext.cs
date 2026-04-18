using ClinicaFlow.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicaFlow.Api.Infrastructure.Data;

public class ClinicaFlowDbContext : DbContext
{
    public ClinicaFlowDbContext(DbContextOptions<ClinicaFlowDbContext> options)
        : base(options)
    {
    }
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Specialty> Specialties => Set<Specialty>();
    public DbSet<AvailabilitySlot> AvailabilitySlots => Set<AvailabilitySlot>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<MedicalReport> MedicalReports => Set<MedicalReport>();
    public DbSet<UserAccount> UserAccounts => Set<UserAccount>();

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    modelBuilder.ApplyConfigurationsFromAssembly(typeof(ClinicaFlowDbContext).Assembly);

    modelBuilder.Entity<UserAccount>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<UserAccount>()
            .HasIndex(u => u.DoctorId)
            .IsUnique()
            .HasFilter("[DoctorId] IS NOT NULL");

        modelBuilder.Entity<UserAccount>()
            .HasIndex(u => u.PatientId)
            .IsUnique()
            .HasFilter("[PatientId] IS NOT NULL");
}
}