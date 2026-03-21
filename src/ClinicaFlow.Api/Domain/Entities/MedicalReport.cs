namespace ClinicaFlow.Api.Domain.Entities;

public class MedicalReport
{
    public int Id { get; set; }

    public int AppointmentId { get; set; }
    public Appointment Appointment { get; set; } = null!;

    public string Diagnosis { get; set; } = null!;
    public string? Therapy { get; set; }
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}