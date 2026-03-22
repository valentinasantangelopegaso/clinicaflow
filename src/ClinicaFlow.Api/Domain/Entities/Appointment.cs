using ClinicaFlow.Api.Domain.Enums;

namespace ClinicaFlow.Api.Domain.Entities;

/// <summary>
/// Entità che rappresenta un appuntamento prenotato nel sistema.
/// </summary>
public class Appointment
{
    /// <summary>
    /// Identificativo univoco dell'appuntamento.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Identificativo del paziente associato all'appuntamento.
    /// </summary>
    public int PatientId { get; set; }

    /// <summary>
    /// Identificativo del medico associato all'appuntamento.
    /// </summary>
    public int DoctorId { get; set; }

    /// <summary>
    /// Identificativo dello slot di disponibilità prenotato.
    /// </summary>
    public int AvailabilitySlotId { get; set; }

    /// <summary>
    /// Data e ora dell'appuntamento derivata dallo slot prenotato.
    /// </summary>
    public DateTime AppointmentDate { get; set; }

    /// <summary>
    /// Stato corrente dell'appuntamento.
    /// </summary>
    public AppointmentStatus Status { get; set; }

    /// <summary>
    /// Eventuali note associate all'appuntamento.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Paziente associato all'appuntamento.
    /// </summary>
    public Patient Patient { get; set; } = null!;

    /// <summary>
    /// Medico associato all'appuntamento.
    /// </summary>
    public Doctor Doctor { get; set; } = null!;

    /// <summary>
    /// Slot di disponibilità associato all'appuntamento.
    /// </summary>
    public AvailabilitySlot AvailabilitySlot { get; set; } = null!;

    /// <summary>
    /// Referto medico associato all'appuntamento, se presente.
    /// </summary>
    public MedicalReport? MedicalReport { get; set; }
}