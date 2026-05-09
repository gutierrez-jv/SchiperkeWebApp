using SchiperkeWebApp.Models.Database;

namespace SchiperkeWebApp.Repositories.Interfaces;

public interface IVaccinationRecordRepository
{
    Task<List<VaccinationRecord>> GetAllAsync();
    Task<VaccinationRecord?> GetByIdAsync(int id);
    Task<List<VaccinationRecord>> GetByPetIdAsync(int petId);
    Task<List<VaccinationRecord>> GetUpcomingDueAsync();
    Task<List<VaccinationRecord>> GetOverdueAsync();
    Task<bool> ExistsByAppointmentIdAsync(int appointmentId, int? excludingVaccinationId = null);
    Task AddAsync(VaccinationRecord vaccinationRecord);
    void Update(VaccinationRecord vaccinationRecord);
    void Delete(VaccinationRecord vaccinationRecord);
    Task SaveAsync();
}
