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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ClinicaFlowDbContext).Assembly);
    }
}