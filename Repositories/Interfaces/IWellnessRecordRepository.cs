using SchiperkeWebApp.Models.Database;

namespace SchiperkeWebApp.Repositories.Interfaces;

public interface IWellnessRecordRepository
{
    Task<List<WellnessRecord>> GetAllAsync();
    Task<WellnessRecord?> GetByIdAsync(int id);
    Task<List<WellnessRecord>> GetByPetIdAsync(int petId);
    Task<List<WellnessRecord>> GetByWellnessTypeAsync(string wellnessType);
    Task<List<WellnessRecord>> GetUpcomingDueAsync();
    Task<List<WellnessRecord>> GetOverdueAsync();
    Task AddAsync(WellnessRecord wellnessRecord);
    void Update(WellnessRecord wellnessRecord);
    void Delete(WellnessRecord wellnessRecord);
    Task SaveAsync();
}
