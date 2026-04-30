using Microsoft.EntityFrameworkCore;
using SchiperkeWebApp.Models.Database;
using SchiperkeWebApp.Repositories.Interfaces;

namespace SchiperkeWebApp.Repositories.Implementations;

public class WellnessRecordRepository : IWellnessRecordRepository
{
    private readonly SchiperkeDbContext _context;

    public WellnessRecordRepository(SchiperkeDbContext context)
    {
        _context = context;
    }

    public async Task<List<WellnessRecord>> GetAllAsync()
    {
        return await _context.WellnessRecords
            .Include(w => w.Pet)
            .Include(w => w.Appointment)
            .Include(w => w.RecordedByUser)
            .Where(w => !w.IsDeleted)
            .OrderByDescending(w => w.DateGiven)
            .ToListAsync();
    }

    public async Task<WellnessRecord?> GetByIdAsync(int id)
    {
        return await _context.WellnessRecords
            .Include(w => w.Pet)
            .Include(w => w.Appointment)
            .Include(w => w.RecordedByUser)
            .FirstOrDefaultAsync(w => w.WellnessId == id && !w.IsDeleted);
    }

    public async Task<List<WellnessRecord>> GetByPetIdAsync(int petId)
    {
        return await _context.WellnessRecords
            .Include(w => w.Pet)
            .Include(w => w.Appointment)
            .Include(w => w.RecordedByUser)
            .Where(w => w.PetId == petId && !w.IsDeleted)
            .OrderByDescending(w => w.DateGiven)
            .ToListAsync();
    }

    public async Task<List<WellnessRecord>> GetByWellnessTypeAsync(string wellnessType)
    {
        return await _context.WellnessRecords
            .Include(w => w.Pet)
            .Include(w => w.Appointment)
            .Include(w => w.RecordedByUser)
            .Where(w => w.WellnessType == wellnessType && !w.IsDeleted)
            .OrderByDescending(w => w.DateGiven)
            .ToListAsync();
    }

    public async Task<List<WellnessRecord>> GetUpcomingDueAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var thirtyDaysFromToday = today.AddDays(30);

        return await _context.WellnessRecords
            .Include(w => w.Pet)
            .Include(w => w.Appointment)
            .Include(w => w.RecordedByUser)
            .Where(w => !w.IsDeleted &&
                        w.NextDueDate.HasValue &&
                        w.NextDueDate.Value >= today &&
                        w.NextDueDate.Value <= thirtyDaysFromToday)
            .OrderBy(w => w.NextDueDate)
            .ToListAsync();
    }

    public async Task<List<WellnessRecord>> GetOverdueAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        return await _context.WellnessRecords
            .Include(w => w.Pet)
            .Include(w => w.Appointment)
            .Include(w => w.RecordedByUser)
            .Where(w => !w.IsDeleted &&
                        w.NextDueDate.HasValue &&
                        w.NextDueDate.Value < today)
            .OrderBy(w => w.NextDueDate)
            .ToListAsync();
    }

    public async Task AddAsync(WellnessRecord wellnessRecord)
    {
        await _context.WellnessRecords.AddAsync(wellnessRecord);
    }

    public void Update(WellnessRecord wellnessRecord)
    {
        _context.WellnessRecords.Update(wellnessRecord);
    }

    public void Delete(WellnessRecord wellnessRecord)
    {
        wellnessRecord.IsDeleted = true;
        _context.WellnessRecords.Update(wellnessRecord);
    }

    public async Task SaveAsync()
    {
        await _context.SaveChangesAsync();
    }
}
