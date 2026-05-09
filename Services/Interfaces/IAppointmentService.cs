using SchiperkeWebApp.Models.Database;

namespace SchiperkeWebApp.Services.Interfaces;

public interface IAppointmentService
{
    IReadOnlyList<string> GetAllowedServiceTypes();
    IReadOnlyList<string> GetAllowedStatuses();
    IReadOnlyList<TimeOnly> GetAllowedAppointmentTimes();
    Task<List<Appointment>> GetAllAsync();
    Task<Appointment?> GetByIdAsync(int id);
    Task<Appointment?> GetPublicStatusAsync(string appointmentCode, string patientIdentifier);
    Task<List<Appointment>> GetByPetIdAsync(int petId);
    Task<List<Appointment>> GetByDateAsync(DateOnly appointmentDate);
    Task<List<Appointment>> GetByStatusAsync(string status);
    Task<Pet> RegisterPatientAsync(int appointmentId);
    Task CreateAsync(Appointment appointment);
    Task UpdateAsync(Appointment appointment);
    Task DeleteAsync(int id);
}
