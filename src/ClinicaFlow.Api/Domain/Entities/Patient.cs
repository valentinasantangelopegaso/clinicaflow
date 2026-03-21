namespace ClinicaFlow.Api.Domain.Entities;

public class Patient
{
    public int Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string TaxCode { get; set; } = null!;
    public DateTime BirthDate { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}