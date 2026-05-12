using Microsoft.EntityFrameworkCore;
using SchiperkeWebApp.Models.Database;
using SchiperkeWebApp.Repositories.Interfaces;

namespace SchiperkeWebApp.Repositories.Implementations;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly SchiperkeDbContext _context;

    public AppointmentRepository(SchiperkeDbContext context)
    {
        _context = context;
    }

    public async Task<List<Appointment>> GetAllAsync()
    {
        return await _context.Appointments
            .Include(a => a.Pet)
            .Include(a => a.CreatedByUser)
            .Include(a => a.ConsultationRecords)
            .Include(a => a.VaccinationRecords)
            .Include(a => a.WellnessRecords)
            .Where(a => !a.IsDeleted)
            .OrderByDescending(a => a.AppointmentDate)
            .ThenBy(a => a.AppointmentTime)
            .ToListAsync();
    }

    public async Task<Appointment?> GetByIdAsync(int id)
    {
        return await _context.Appointments
            .Include(a => a.Pet)
            .Include(a => a.CreatedByUser)
            .Include(a => a.ConsultationRecords)
            .Include(a => a.VaccinationRecords)
            .Include(a => a.WellnessRecords)
            .FirstOrDefaultAsync(a => a.AppointmentId == id && !a.IsDeleted);
    }

    public async Task<Appointment?> GetByIdIncludingDeletedAsync(int id)
    {
        return await _context.Appointments
            .Include(a => a.Pet)
            .Include(a => a.CreatedByUser)
            .Include(a => a.ConsultationRecords)
            .Include(a => a.VaccinationRecords)
            .Include(a => a.WellnessRecords)
            .FirstOrDefaultAsync(a => a.AppointmentId == id);
    }

    public async Task<Appointment?> GetByAppointmentCodeAsync(string appointmentCode)
    {
        return await _context.Appointments
            .Include(a => a.Pet)
            .Include(a => a.CreatedByUser)
            .FirstOrDefaultAsync(a => a.AppointmentCode == appointmentCode && !a.IsDeleted);
    }

    public async Task<List<Appointment>> GetByPetIdAsync(int petId)
    {
        return await _context.Appointments
            .Include(a => a.Pet)
            .Include(a => a.CreatedByUser)
            .Where(a => a.PetId == petId && !a.IsDeleted)
            .OrderByDescending(a => a.AppointmentDate)
            .ThenBy(a => a.AppointmentTime)
            .ToListAsync();
    }

    public async Task<List<Appointment>> GetByDateAsync(DateOnly appointmentDate)
    {
        return await _context.Appointments
            .Include(a => a.Pet)
            .Include(a => a.CreatedByUser)
            .Where(a => a.AppointmentDate == appointmentDate && !a.IsDeleted)
            .OrderByDescending(a => a.AppointmentDate)
            .ThenBy(a => a.AppointmentTime)
            .ToListAsync();
    }

    public async Task<List<Appointment>> GetByStatusAsync(string status)
    {
        return await _context.Appointments
            .Include(a => a.Pet)
            .Include(a => a.CreatedByUser)
            .Where(a => a.Status == status && !a.IsDeleted)
            .OrderByDescending(a => a.AppointmentDate)
            .ThenBy(a => a.AppointmentTime)
            .ToListAsync();
    }

    public async Task AddAsync(Appointment appointment)
    {
        await _context.Appointments.AddAsync(appointment);
    }

    public void Update(Appointment appointment)
    {
        _context.Appointments.Update(appointment);
    }

    public void Delete(Appointment appointment)
    {
        appointment.IsDeleted = true;
        _context.Appointments.Update(appointment);
    }

    public async Task SaveAsync()
    {
        await _context.SaveChangesAsync();
    }
}
