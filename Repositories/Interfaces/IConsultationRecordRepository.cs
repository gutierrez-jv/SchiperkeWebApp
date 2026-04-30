using SchiperkeWebApp.Models.Database;

namespace SchiperkeWebApp.Repositories.Interfaces;

public interface IConsultationRecordRepository
{
    Task<List<ConsultationRecord>> GetAllAsync();
    Task<ConsultationRecord?> GetByIdAsync(int id);
    Task<List<ConsultationRecord>> GetByPetIdAsync(int petId);
    Task<List<ConsultationRecord>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task AddAsync(ConsultationRecord consultationRecord);
    void Update(ConsultationRecord consultationRecord);
    void Delete(ConsultationRecord consultationRecord);
    Task SaveAsync();
}
