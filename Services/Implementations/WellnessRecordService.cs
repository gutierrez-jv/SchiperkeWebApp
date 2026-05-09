using SchiperkeWebApp.Models.Database;
using SchiperkeWebApp.Repositories.Interfaces;
using SchiperkeWebApp.Services.Interfaces;

namespace SchiperkeWebApp.Services.Implementations;

public class WellnessRecordService : IWellnessRecordService
{
    private static readonly string[] AllowedWellnessTypes =
    [
        "Deworming",
        "Internal Antiparasitic",
        "External Antiparasitic",
        "General Wellness"
    ];

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

        var normalizedWellnessType = NormalizeAllowedWellnessType(wellnessType);
        return await _wellnessRecordRepository.GetByWellnessTypeAsync(normalizedWellnessType);
    }

    public IReadOnlyList<string> GetAllowedWellnessTypes()
    {
        return AllowedWellnessTypes;
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

        if (wellnessRecord.CreatedAt == default)
        {
            wellnessRecord.CreatedAt = DateTime.Now;
        }

        wellnessRecord.IsDeleted = false;

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

        existingWellnessRecord.PetId = wellnessRecord.PetId;
        existingWellnessRecord.AppointmentId = wellnessRecord.AppointmentId;
        existingWellnessRecord.WellnessType = wellnessRecord.WellnessType;
        existingWellnessRecord.ProductOrMedication = wellnessRecord.ProductOrMedication;
        existingWellnessRecord.DateGiven = wellnessRecord.DateGiven;
        existingWellnessRecord.NextDueDate = wellnessRecord.NextDueDate;
        existingWellnessRecord.Dose = wellnessRecord.Dose;
        existingWellnessRecord.Route = wellnessRecord.Route;
        existingWellnessRecord.Remarks = wellnessRecord.Remarks;
        existingWellnessRecord.RecordedByUserId = wellnessRecord.RecordedByUserId;

        _wellnessRecordRepository.Update(existingWellnessRecord);
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

        _ = NormalizeAllowedWellnessType(wellnessRecord.WellnessType);

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

            if (!appointment.PetId.HasValue)
            {
                throw new InvalidOperationException("Public booking appointments are not linked directly to wellness records.");
            }

            if (appointment.PetId.Value != wellnessRecord.PetId)
            {
                throw new InvalidOperationException("Wellness appointment does not belong to the selected pet.");
            }

            if (!appointment.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Wellness records can only be linked to completed appointments.");
            }

            if (!IsWellnessServiceType(appointment.ServiceType))
            {
                throw new InvalidOperationException("Wellness records can only be linked to wellness, deworming, or antiparasitic appointments.");
            }

            var hasExistingRecord = await _wellnessRecordRepository.ExistsByAppointmentIdAsync(
                appointment.AppointmentId,
                wellnessRecord.WellnessId == 0 ? null : wellnessRecord.WellnessId);

            if (hasExistingRecord)
            {
                throw new InvalidOperationException("This appointment already has a wellness record.");
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
        wellnessRecord.WellnessType = NormalizeAllowedWellnessType(wellnessRecord.WellnessType);
        wellnessRecord.ProductOrMedication = wellnessRecord.ProductOrMedication.Trim();
        wellnessRecord.Dose = CleanText(wellnessRecord.Dose);
        wellnessRecord.Route = CleanText(wellnessRecord.Route);
        wellnessRecord.Remarks = CleanText(wellnessRecord.Remarks);
    }

    private static string NormalizeAllowedWellnessType(string wellnessType)
    {
        var value = wellnessType.Trim();
        var normalizedValue = AllowedWellnessTypes.FirstOrDefault(type => type.Equals(value, StringComparison.OrdinalIgnoreCase));
        if (normalizedValue is null)
        {
            throw new ArgumentException("Wellness type must be Deworming, Internal Antiparasitic, External Antiparasitic, or General Wellness.");
        }

        return normalizedValue;
    }

    private static bool IsWellnessServiceType(string serviceType)
    {
        return serviceType.Equals("Deworming", StringComparison.OrdinalIgnoreCase)
            || serviceType.Equals("Internal Antiparasitic", StringComparison.OrdinalIgnoreCase)
            || serviceType.Equals("External Antiparasitic", StringComparison.OrdinalIgnoreCase)
            || serviceType.Equals("General Wellness", StringComparison.OrdinalIgnoreCase);
    }

    private static string? CleanText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
