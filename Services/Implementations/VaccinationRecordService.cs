using SchiperkeWebApp.Models.Database;
using SchiperkeWebApp.Repositories.Interfaces;
using SchiperkeWebApp.Services.Interfaces;

namespace SchiperkeWebApp.Services.Implementations;

public class VaccinationRecordService : IVaccinationRecordService
{
    private readonly IVaccinationRecordRepository _vaccinationRecordRepository;
    private readonly IPetRepository _petRepository;
    private readonly IUserRepository _userRepository;
    private readonly IAppointmentRepository _appointmentRepository;

    public VaccinationRecordService(
        IVaccinationRecordRepository vaccinationRecordRepository,
        IPetRepository petRepository,
        IUserRepository userRepository,
        IAppointmentRepository appointmentRepository)
    {
        _vaccinationRecordRepository = vaccinationRecordRepository;
        _petRepository = petRepository;
        _userRepository = userRepository;
        _appointmentRepository = appointmentRepository;
    }

    public async Task<List<VaccinationRecord>> GetAllAsync()
    {
        return await _vaccinationRecordRepository.GetAllAsync();
    }

    public async Task<VaccinationRecord?> GetByIdAsync(int id)
    {
        return await _vaccinationRecordRepository.GetByIdAsync(id);
    }

    public async Task<List<VaccinationRecord>> GetByPetIdAsync(int petId)
    {
        return await _vaccinationRecordRepository.GetByPetIdAsync(petId);
    }

    public async Task<List<VaccinationRecord>> GetUpcomingDueAsync()
    {
        return await _vaccinationRecordRepository.GetUpcomingDueAsync();
    }

    public async Task<List<VaccinationRecord>> GetOverdueAsync()
    {
        return await _vaccinationRecordRepository.GetOverdueAsync();
    }

    public async Task CreateAsync(VaccinationRecord vaccinationRecord)
    {
        await ValidateVaccinationRecordAsync(vaccinationRecord);
        NormalizeVaccinationRecord(vaccinationRecord);

        if (vaccinationRecord.CreatedAt == default)
        {
            vaccinationRecord.CreatedAt = DateTime.Now;
        }

        vaccinationRecord.IsDeleted = false;

        await _vaccinationRecordRepository.AddAsync(vaccinationRecord);
        await _vaccinationRecordRepository.SaveAsync();
    }

    public async Task UpdateAsync(VaccinationRecord vaccinationRecord)
    {
        var existingVaccinationRecord = await _vaccinationRecordRepository.GetByIdAsync(vaccinationRecord.VaccinationId);
        if (existingVaccinationRecord is null)
        {
            throw new InvalidOperationException("Vaccination record was not found.");
        }

        await ValidateVaccinationRecordAsync(vaccinationRecord);
        NormalizeVaccinationRecord(vaccinationRecord);

        existingVaccinationRecord.PetId = vaccinationRecord.PetId;
        existingVaccinationRecord.AppointmentId = vaccinationRecord.AppointmentId;
        existingVaccinationRecord.VaccineName = vaccinationRecord.VaccineName;
        existingVaccinationRecord.DateGiven = vaccinationRecord.DateGiven;
        existingVaccinationRecord.NextDueDate = vaccinationRecord.NextDueDate;
        existingVaccinationRecord.Dose = vaccinationRecord.Dose;
        existingVaccinationRecord.Route = vaccinationRecord.Route;
        existingVaccinationRecord.Manufacturer = vaccinationRecord.Manufacturer;
        existingVaccinationRecord.LotNumber = vaccinationRecord.LotNumber;
        existingVaccinationRecord.Remarks = vaccinationRecord.Remarks;
        existingVaccinationRecord.RecordedByUserId = vaccinationRecord.RecordedByUserId;

        _vaccinationRecordRepository.Update(existingVaccinationRecord);
        await _vaccinationRecordRepository.SaveAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var vaccinationRecord = await _vaccinationRecordRepository.GetByIdAsync(id);
        if (vaccinationRecord is null)
        {
            return;
        }

        // Soft delete only: do not physically remove records from the database.
        _vaccinationRecordRepository.Delete(vaccinationRecord);
        await _vaccinationRecordRepository.SaveAsync();
    }

    private async Task ValidateVaccinationRecordAsync(VaccinationRecord vaccinationRecord)
    {
        if (string.IsNullOrWhiteSpace(vaccinationRecord.VaccineName))
        {
            throw new ArgumentException("Vaccine name is required.");
        }

        if (vaccinationRecord.DateGiven == default)
        {
            throw new ArgumentException("Date given is required.");
        }

        var pet = await _petRepository.GetByIdAsync(vaccinationRecord.PetId);
        if (pet is null)
        {
            throw new InvalidOperationException("Vaccination record must reference an active pet.");
        }

        var user = await _userRepository.GetByIdAsync(vaccinationRecord.RecordedByUserId);
        if (user is null)
        {
            throw new InvalidOperationException("Vaccination record must reference an active user.");
        }

        if (vaccinationRecord.AppointmentId.HasValue)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(vaccinationRecord.AppointmentId.Value);
            if (appointment is null)
            {
                throw new InvalidOperationException("Vaccination appointment was not found.");
            }

            if (!appointment.PetId.HasValue)
            {
                throw new InvalidOperationException("Public booking appointments are not linked directly to vaccination records.");
            }

            if (appointment.PetId.Value != vaccinationRecord.PetId)
            {
                throw new InvalidOperationException("Vaccination appointment does not belong to the selected pet.");
            }
        }

        if (vaccinationRecord.NextDueDate.HasValue &&
            vaccinationRecord.NextDueDate.Value < vaccinationRecord.DateGiven)
        {
            throw new ArgumentException("Next due date cannot be earlier than the date given.");
        }
    }

    private static void NormalizeVaccinationRecord(VaccinationRecord vaccinationRecord)
    {
        vaccinationRecord.VaccineName = vaccinationRecord.VaccineName.Trim();
        vaccinationRecord.Dose = CleanText(vaccinationRecord.Dose);
        vaccinationRecord.Route = CleanText(vaccinationRecord.Route);
        vaccinationRecord.Manufacturer = CleanText(vaccinationRecord.Manufacturer);
        vaccinationRecord.LotNumber = CleanText(vaccinationRecord.LotNumber);
        vaccinationRecord.Remarks = CleanText(vaccinationRecord.Remarks);
    }

    private static string? CleanText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
