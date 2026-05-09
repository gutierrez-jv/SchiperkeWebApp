using SchiperkeWebApp.Models.Database;

namespace SchiperkeWebApp.Repositories.Interfaces;

public interface IAppointmentRepository
{
    Task<List<Appointment>> GetAllAsync();
    Task<Appointment?> GetByIdAsync(int id);
    Task<Appointment?> GetByAppointmentCodeAsync(string appointmentCode);
    Task<List<Appointment>> GetByPetIdAsync(int petId);
    Task<List<Appointment>> GetByDateAsync(DateOnly appointmentDate);
    Task<List<Appointment>> GetByStatusAsync(string status);
    Task AddAsync(Appointment appointment);
    void Update(Appointment appointment);
    void Delete(Appointment appointment);
    Task SaveAsync();
}
