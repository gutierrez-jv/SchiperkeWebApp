using SchiperkeWebApp.Models.Database;

namespace SchiperkeWebApp.Services.Interfaces;

public interface IWellnessRecordService
{
    IReadOnlyList<string> GetAllowedWellnessTypes();
    Task<List<WellnessRecord>> GetAllAsync();
    Task<WellnessRecord?> GetByIdAsync(int id);
    Task<List<WellnessRecord>> GetByPetIdAsync(int petId);
    Task<List<WellnessRecord>> GetByWellnessTypeAsync(string wellnessType);
    Task<List<WellnessRecord>> GetUpcomingDueAsync();
    Task<List<WellnessRecord>> GetOverdueAsync();
    Task CreateAsync(WellnessRecord wellnessRecord);
    Task UpdateAsync(WellnessRecord wellnessRecord);
    Task DeleteAsync(int id);
}
