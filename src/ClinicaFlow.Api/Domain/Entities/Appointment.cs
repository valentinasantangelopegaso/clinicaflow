using ClinicaFlow.Api.Domain.Enums;

namespace ClinicaFlow.Api.Domain.Entities;

public class Appointment
{
    public int Id { get; set; }

    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    public int DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;

    public int AvailabilitySlotId { get; set; }
    public AvailabilitySlot AvailabilitySlot { get; set; } = null!;

    public DateTime AppointmentDate { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
    public string? Notes { get; set; }

    public MedicalReport? MedicalReport { get; set; }
}