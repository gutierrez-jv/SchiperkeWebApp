using SchiperkeWebApp.Models.Database;
using SchiperkeWebApp.Repositories.Interfaces;
using SchiperkeWebApp.Services.Interfaces;

namespace SchiperkeWebApp.Services.Implementations;

public class WellnessRecordService : IWellnessRecordService
{
    private readonly IWellnessRecordRepository _wellnessRecordRepository;
    private readonly IPetRepository _petRepository;
    private readonly IUserRepository _userRepository;
    private readonly IAppointmentRepository _appointmentRepository;

    public WellnessRecordService(
        IWellnessRecordRepository wellnessRecordRepository,
        IPetRepository petRepository,
        IUserRepository userRepository,
        IAppointmentRepository appointmentRepository)
    {
        _wellnessRecordRepository = wellnessRecordRepository;
        _petRepository = petRepository;
        _userRepository = userRepository;
        _appointmentRepository = appointmentRepository;
    }

    public async Task<List<WellnessRecord>> GetAllAsync()
    {
        return await _wellnessRecordRepository.GetAllAsync();
    }

    public async Task<WellnessRecord?> GetByIdAsync(int id)
    {
        return await _wellnessRecordRepository.GetByIdAsync(id);
    }

    public async Task<List<WellnessRecord>> GetByPetIdAsync(int petId)
    {
        return await _wellnessRecordRepository.GetByPetIdAsync(petId);
    }

    public async Task<List<WellnessRecord>> GetByWellnessTypeAsync(string wellnessType)
    {
        if (string.IsNullOrWhiteSpace(wellnessType))
        {
            throw new ArgumentException("Wellness type is required.");
        }

        return await _wellnessRecordRepository.GetByWellnessTypeAsync(wellnessType.Trim());
    }

    public async Task<List<WellnessRecord>> GetUpcomingDueAsync()
    {
        return await _wellnessRecordRepository.GetUpcomingDueAsync();
    }

    public async Task<List<WellnessRecord>> GetOverdueAsync()
    {
        return await _wellnessRecordRepository.GetOverdueAsync();
    }

    public async Task CreateAsync(WellnessRecord wellnessRecord)
    {
        await ValidateWellnessRecordAsync(wellnessRecord);
        NormalizeWellnessRecord(wellnessRecord);

        await _wellnessRecordRepository.AddAsync(wellnessRecord);
        await _wellnessRecordRepository.SaveAsync();
    }

    public async Task UpdateAsync(WellnessRecord wellnessRecord)
    {
        var existingWellnessRecord = await _wellnessRecordRepository.GetByIdAsync(wellnessRecord.WellnessId);
        if (existingWellnessRecord is null)
        {
            throw new InvalidOperationException("Wellness record was not found.");
        }

        await ValidateWellnessRecordAsync(wellnessRecord);
        NormalizeWellnessRecord(wellnessRecord);
        wellnessRecord.CreatedAt = existingWellnessRecord.CreatedAt;
        wellnessRecord.IsDeleted = existingWellnessRecord.IsDeleted;

        _wellnessRecordRepository.Update(wellnessRecord);
        await _wellnessRecordRepository.SaveAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var wellnessRecord = await _wellnessRecordRepository.GetByIdAsync(id);
        if (wellnessRecord is null)
        {
            return;
        }

        // Soft delete only: do not physically remove records from the database.
        _wellnessRecordRepository.Delete(wellnessRecord);
        await _wellnessRecordRepository.SaveAsync();
    }

    private async Task ValidateWellnessRecordAsync(WellnessRecord wellnessRecord)
    {
        if (string.IsNullOrWhiteSpace(wellnessRecord.WellnessType))
        {
            throw new ArgumentException("Wellness type is required.");
        }

        if (string.IsNullOrWhiteSpace(wellnessRecord.ProductOrMedication))
        {
            throw new ArgumentException("Product or medication is required.");
        }

        if (wellnessRecord.DateGiven == default)
        {
            throw new ArgumentException("Date given is required.");
        }

        var normalizedType = wellnessRecord.WellnessType.Trim();
        if (normalizedType.Contains("vaccin", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "Vaccination entries should be saved in Vaccination Records, not Wellness Records.");
        }

        var pet = await _petRepository.GetByIdAsync(wellnessRecord.PetId);
        if (pet is null)
        {
            throw new InvalidOperationException("Wellness record must reference an active pet.");
        }

        var user = await _userRepository.GetByIdAsync(wellnessRecord.RecordedByUserId);
        if (user is null)
        {
            throw new InvalidOperationException("Wellness record must reference an active user.");
        }

        if (wellnessRecord.AppointmentId.HasValue)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(wellnessRecord.AppointmentId.Value);
            if (appointment is null)
            {
                throw new InvalidOperationException("Wellness appointment was not found.");
            }

            if (appointment.PetId != wellnessRecord.PetId)
            {
                throw new InvalidOperationException("Wellness appointment does not belong to the selected pet.");
            }
        }

        if (wellnessRecord.NextDueDate.HasValue &&
            wellnessRecord.NextDueDate.Value < wellnessRecord.DateGiven)
        {
            throw new ArgumentException("Next due date cannot be earlier than the date given.");
        }
    }

    private static void NormalizeWellnessRecord(WellnessRecord wellnessRecord)
    {
        wellnessRecord.WellnessType = wellnessRecord.WellnessType.Trim();
        wellnessRecord.ProductOrMedication = wellnessRecord.ProductOrMedication.Trim();
        wellnessRecord.Dose = CleanText(wellnessRecord.Dose);
        wellnessRecord.Route = CleanText(wellnessRecord.Route);
        wellnessRecord.Remarks = CleanText(wellnessRecord.Remarks);
    }

    private static string? CleanText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
