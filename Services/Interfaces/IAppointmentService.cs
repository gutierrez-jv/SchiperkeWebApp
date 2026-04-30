using SchiperkeWebApp.Models.Database;

namespace SchiperkeWebApp.Services.Interfaces;

public interface IAppointmentService
{
    Task<List<Appointment>> GetAllAsync();
    Task<Appointment?> GetByIdAsync(int id);
    Task<List<Appointment>> GetByPetIdAsync(int petId);
    Task<List<Appointment>> GetByDateAsync(DateOnly appointmentDate);
    Task<List<Appointment>> GetByStatusAsync(string status);
    Task CreateAsync(Appointment appointment);
    Task UpdateAsync(Appointment appointment);
    Task DeleteAsync(int id);
}
