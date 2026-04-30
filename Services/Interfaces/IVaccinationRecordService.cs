using SchiperkeWebApp.Models.Database;

namespace SchiperkeWebApp.Services.Interfaces;

public interface IVaccinationRecordService
{
    Task<List<VaccinationRecord>> GetAllAsync();
    Task<VaccinationRecord?> GetByIdAsync(int id);
    Task<List<VaccinationRecord>> GetByPetIdAsync(int petId);
    Task<List<VaccinationRecord>> GetUpcomingDueAsync();
    Task<List<VaccinationRecord>> GetOverdueAsync();
    Task CreateAsync(VaccinationRecord vaccinationRecord);
    Task UpdateAsync(VaccinationRecord vaccinationRecord);
    Task DeleteAsync(int id);
}
