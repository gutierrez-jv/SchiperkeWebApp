using Microsoft.EntityFrameworkCore;
using SchiperkeWebApp.Models.Database;
using SchiperkeWebApp.Repositories.Interfaces;

namespace SchiperkeWebApp.Repositories.Implementations;

public class ConsultationRecordRepository : IConsultationRecordRepository
{
    private readonly SchiperkeDbContext _context;

    public ConsultationRecordRepository(SchiperkeDbContext context)
    {
        _context = context;
    }

    public async Task<List<ConsultationRecord>> GetAllAsync()
    {
        return await _context.ConsultationRecords
            .Include(c => c.Pet)
            .Include(c => c.Appointment)
            .Include(c => c.RecordedByUser)
            .Where(c => !c.IsDeleted)
            .OrderByDescending(c => c.ConsultationDate)
            .ToListAsync();
    }

    public async Task<ConsultationRecord?> GetByIdAsync(int id)
    {
        return await _context.ConsultationRecords
            .Include(c => c.Pet)
            .Include(c => c.Appointment)
            .Include(c => c.RecordedByUser)
            .FirstOrDefaultAsync(c => c.ConsultationId == id && !c.IsDeleted);
    }

    public async Task<List<ConsultationRecord>> GetByPetIdAsync(int petId)
    {
        return await _context.ConsultationRecords
            .Include(c => c.Pet)
            .Include(c => c.Appointment)
            .Include(c => c.RecordedByUser)
            .Where(c => c.PetId == petId && !c.IsDeleted)
            .OrderByDescending(c => c.ConsultationDate)
            .ToListAsync();
    }

    public async Task<List<ConsultationRecord>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.ConsultationRecords
            .Include(c => c.Pet)
            .Include(c => c.Appointment)
            .Include(c => c.RecordedByUser)
            .Where(c => !c.IsDeleted &&
                        c.ConsultationDate >= startDate &&
                        c.ConsultationDate <= endDate)
            .OrderByDescending(c => c.ConsultationDate)
            .ToListAsync();
    }

    public async Task AddAsync(ConsultationRecord consultationRecord)
    {
        await _context.ConsultationRecords.AddAsync(consultationRecord);
    }

    public void Update(ConsultationRecord consultationRecord)
    {
        _context.ConsultationRecords.Update(consultationRecord);
    }

    public void Delete(ConsultationRecord consultationRecord)
    {
        consultationRecord.IsDeleted = true;
        _context.ConsultationRecords.Update(consultationRecord);
    }

    public async Task SaveAsync()
    {
        await _context.SaveChangesAsync();
    }
}
