namespace ClinicaFlow.Api.Domain.Entities;

public class AvailabilitySlot
{
    public int Id { get; set; }

    public int DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;

    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public bool IsAvailable { get; set; } = true;

    public Appointment? Appointment { get; set; }
}