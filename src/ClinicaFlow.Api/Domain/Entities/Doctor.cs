namespace ClinicaFlow.Api.Domain.Entities;

public class Doctor
{
    public int Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? Email { get; set; }

    public int SpecialtyId { get; set; }
    public Specialty Specialty { get; set; } = null!;

    public ICollection<AvailabilitySlot> AvailabilitySlots { get; set; } = new List<AvailabilitySlot>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}