using SchiperkeWebApp.Models.Database;

namespace SchiperkeWebApp.Services.Interfaces;

public interface IConsultationRecordService
{
    Task<List<ConsultationRecord>> GetAllAsync();
    Task<ConsultationRecord?> GetByIdAsync(int id);
    Task<List<ConsultationRecord>> GetByPetIdAsync(int petId);
    Task<List<ConsultationRecord>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task CreateAsync(ConsultationRecord consultationRecord);
    Task UpdateAsync(ConsultationRecord consultationRecord);
    Task DeleteAsync(int id);
}
