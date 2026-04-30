using Microsoft.EntityFrameworkCore;
using SchiperkeWebApp.Models.Database;
using SchiperkeWebApp.Repositories.Interfaces;

namespace SchiperkeWebApp.Repositories.Implementations;

public class VaccinationRecordRepository : IVaccinationRecordRepository
{
    private readonly SchiperkeDbContext _context;

    public VaccinationRecordRepository(SchiperkeDbContext context)
    {
        _context = context;
    }

    public async Task<List<VaccinationRecord>> GetAllAsync()
    {
        return await _context.VaccinationRecords
            .Include(v => v.Pet)
            .Include(v => v.Appointment)
            .Include(v => v.RecordedByUser)
            .Where(v => !v.IsDeleted)
            .OrderByDescending(v => v.DateGiven)
            .ToListAsync();
    }

    public async Task<VaccinationRecord?> GetByIdAsync(int id)
    {
        return await _context.VaccinationRecords
            .Include(v => v.Pet)
            .Include(v => v.Appointment)
            .Include(v => v.RecordedByUser)
            .FirstOrDefaultAsync(v => v.VaccinationId == id && !v.IsDeleted);
    }

    public async Task<List<VaccinationRecord>> GetByPetIdAsync(int petId)
    {
        return await _context.VaccinationRecords
            .Include(v => v.Pet)
            .Include(v => v.Appointment)
            .Include(v => v.RecordedByUser)
            .Where(v => v.PetId == petId && !v.IsDeleted)
            .OrderByDescending(v => v.DateGiven)
            .ToListAsync();
    }

    public async Task<List<VaccinationRecord>> GetUpcomingDueAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var thirtyDaysFromToday = today.AddDays(30);

        return await _context.VaccinationRecords
            .Include(v => v.Pet)
            .Include(v => v.Appointment)
            .Include(v => v.RecordedByUser)
            .Where(v => !v.IsDeleted &&
                        v.NextDueDate.HasValue &&
                        v.NextDueDate.Value >= today &&
                        v.NextDueDate.Value <= thirtyDaysFromToday)
            .OrderBy(v => v.NextDueDate)
            .ToListAsync();
    }

    public async Task<List<VaccinationRecord>> GetOverdueAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        return await _context.VaccinationRecords
            .Include(v => v.Pet)
            .Include(v => v.Appointment)
            .Include(v => v.RecordedByUser)
            .Where(v => !v.IsDeleted &&
                        v.NextDueDate.HasValue &&
                        v.NextDueDate.Value < today)
            .OrderBy(v => v.NextDueDate)
            .ToListAsync();
    }

    public async Task AddAsync(VaccinationRecord vaccinationRecord)
    {
        await _context.VaccinationRecords.AddAsync(vaccinationRecord);
    }

    public void Update(VaccinationRecord vaccinationRecord)
    {
        _context.VaccinationRecords.Update(vaccinationRecord);
    }

    public void Delete(VaccinationRecord vaccinationRecord)
    {
        vaccinationRecord.IsDeleted = true;
        _context.VaccinationRecords.Update(vaccinationRecord);
    }

    public async Task SaveAsync()
    {
        await _context.SaveChangesAsync();
    }
}
