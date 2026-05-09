using SchiperkeWebApp.Models.Database;
using SchiperkeWebApp.Repositories.Interfaces;
using SchiperkeWebApp.Services.Interfaces;

namespace SchiperkeWebApp.Services.Implementations;

public class AppointmentService : IAppointmentService
{
    private static readonly string[] AllowedServiceTypes =
    [
        "Consultation",
        "Vaccination",
        "Deworming",
        "Internal Antiparasitic",
        "External Antiparasitic",
        "General Wellness"
    ];

    private static readonly string[] AllowedStatuses =
    [
        "Pending",
        "Confirmed",
        "Completed",
        "Cancelled",
        "No-Show"
    ];

    private static readonly TimeOnly[] AllowedAppointmentTimes = BuildAllowedAppointmentTimes();

    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IPetRepository _petRepository;
    private readonly IUserRepository _userRepository;

    public AppointmentService(
        IAppointmentRepository appointmentRepository,
        IPetRepository petRepository,
        IUserRepository userRepository)
    {
        _appointmentRepository = appointmentRepository;
        _petRepository = petRepository;
        _userRepository = userRepository;
    }

    public IReadOnlyList<string> GetAllowedServiceTypes()
    {
        return AllowedServiceTypes;
    }

    public IReadOnlyList<string> GetAllowedStatuses()
    {
        return AllowedStatuses;
    }

    public IReadOnlyList<TimeOnly> GetAllowedAppointmentTimes()
    {
        return AllowedAppointmentTimes;
    }

    public async Task<List<Appointment>> GetAllAsync()
    {
        return await _appointmentRepository.GetAllAsync();
    }

    public async Task<Appointment?> GetByIdAsync(int id)
    {
        return await _appointmentRepository.GetByIdAsync(id);
    }

    public async Task<Appointment?> GetPublicStatusAsync(string appointmentCode, string patientIdentifier)
    {
        if (string.IsNullOrWhiteSpace(appointmentCode))
        {
            throw new ArgumentException("Appointment code is required.");
        }

        if (string.IsNullOrWhiteSpace(patientIdentifier))
        {
            throw new ArgumentException("Patient number or pet name is required.");
        }

        var appointment = await FindAppointmentByPublicCodeAsync(appointmentCode);
        if (appointment is null)
        {
            return null;
        }

        return MatchesPublicIdentifier(appointment, patientIdentifier) ? appointment : null;
    }

    public async Task<List<Appointment>> GetByPetIdAsync(int petId)
    {
        return await _appointmentRepository.GetByPetIdAsync(petId);
    }

    public async Task<List<Appointment>> GetByDateAsync(DateOnly appointmentDate)
    {
        return await _appointmentRepository.GetByDateAsync(appointmentDate);
    }

    public async Task<List<Appointment>> GetByStatusAsync(string status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            throw new ArgumentException("Status is required.");
        }

        var normalizedStatus = NormalizeAllowedValue(status, AllowedStatuses, "Status");
        return await _appointmentRepository.GetByStatusAsync(normalizedStatus);
    }

    public async Task<Pet> RegisterPatientAsync(int appointmentId)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
        if (appointment is null)
        {
            throw new InvalidOperationException("Appointment record was not found.");
        }

        if (appointment.PetId.HasValue)
        {
            throw new InvalidOperationException("This appointment is already linked to a registered patient.");
        }

        if (!CanRegisterPatient(appointment))
        {
            throw new InvalidOperationException("Only confirmed or completed new-patient appointments can be registered.");
        }

        var pet = new Pet
        {
            PatientNo = await GeneratePatientNoAsync(),
            PetName = NormalizeRequiredText(appointment.PetName),
            Species = NormalizeRequiredText(appointment.Species),
            Breed = NormalizeOptionalText(appointment.Breed),
            Sex = NormalizeRequiredText(appointment.Sex),
            Color = NormalizeOptionalText(appointment.Color),
            IsActive = true,
            CreatedAt = DateTime.Now
        };

        ValidatePetRegistration(pet);

        await _petRepository.AddAsync(pet);
        await _petRepository.SaveAsync();

        appointment.PetId = pet.PetId;
        appointment.PatientNoInput = pet.PatientNo;
        appointment.IsExistingPatient = true;
        appointment.PetName = pet.PetName;
        appointment.Species = pet.Species;
        appointment.Breed = pet.Breed;
        appointment.Sex = pet.Sex;
        appointment.Color = pet.Color;
        appointment.UpdatedAt = DateTime.Now;

        _appointmentRepository.Update(appointment);
        await _appointmentRepository.SaveAsync();

        return pet;
    }

    public async Task CreateAsync(Appointment appointment)
    {
        await ValidateAppointmentAsync(appointment);

        NormalizeAppointment(appointment);
        ApplyCancellationState(appointment);

        if (appointment.CreatedAt == default)
        {
            appointment.CreatedAt = DateTime.Now;
        }

        appointment.UpdatedAt = DateTime.Now;
        appointment.IsDeleted = false;

        await _appointmentRepository.AddAsync(appointment);
        await _appointmentRepository.SaveAsync();

        if (string.IsNullOrWhiteSpace(appointment.AppointmentCode))
        {
            appointment.AppointmentCode = BuildAppointmentCode(appointment);
            _appointmentRepository.Update(appointment);
            await _appointmentRepository.SaveAsync();
        }
    }

    public async Task UpdateAsync(Appointment appointment)
    {
        var existingAppointment = await _appointmentRepository.GetByIdAsync(appointment.AppointmentId);
        if (existingAppointment is null)
        {
            throw new InvalidOperationException("Appointment record was not found.");
        }

        await ValidateAppointmentAsync(appointment);

        NormalizeAppointment(appointment);

        existingAppointment.PetId = appointment.PetId;
        existingAppointment.AppointmentCode = string.IsNullOrWhiteSpace(existingAppointment.AppointmentCode)
            ? BuildAppointmentCode(existingAppointment)
            : existingAppointment.AppointmentCode;
        existingAppointment.IsExistingPatient = appointment.IsExistingPatient;
        existingAppointment.PatientNoInput = appointment.PatientNoInput;
        existingAppointment.PetName = appointment.PetName;
        existingAppointment.Species = appointment.Species;
        existingAppointment.Breed = appointment.Breed;
        existingAppointment.Sex = appointment.Sex;
        existingAppointment.Color = appointment.Color;
        existingAppointment.AppointmentDate = appointment.AppointmentDate;
        existingAppointment.AppointmentTime = appointment.AppointmentTime;
        existingAppointment.ServiceType = appointment.ServiceType;
        existingAppointment.ReasonForVisit = appointment.ReasonForVisit;
        existingAppointment.Status = string.IsNullOrWhiteSpace(appointment.Status)
            ? existingAppointment.Status
            : appointment.Status;
        existingAppointment.Remarks = appointment.Remarks;
        existingAppointment.CancellationReason = appointment.CancellationReason;
        existingAppointment.CancelledBy = appointment.CancelledBy;
        existingAppointment.CancelledAt = appointment.CancelledAt;
        ApplyCancellationState(existingAppointment);
        existingAppointment.CreatedByUserId = appointment.CreatedByUserId ?? existingAppointment.CreatedByUserId;
        existingAppointment.UpdatedAt = DateTime.Now;

        _appointmentRepository.Update(existingAppointment);
        await _appointmentRepository.SaveAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id);
        if (appointment is null)
        {
            return;
        }

        // Soft delete only
        _appointmentRepository.Delete(appointment);
        await _appointmentRepository.SaveAsync();
    }

    private async Task ValidateAppointmentAsync(Appointment appointment)
    {
        if (appointment.IsExistingPatient || !string.IsNullOrWhiteSpace(appointment.PatientNoInput))
        {
            var patientNo = NormalizeOptionalText(appointment.PatientNoInput);
            if (patientNo is null)
            {
                throw new ArgumentException("Patient number is required for existing patients.");
            }

            var pet = await _petRepository.GetByPatientNoAsync(patientNo);
            if (pet is null)
            {
                throw new InvalidOperationException("No active patient record was found for the provided patient number.");
            }

            ApplyExistingPatientDetails(appointment, pet);
        }
        else if (appointment.PetId.HasValue)
        {
            var pet = await _petRepository.GetByIdAsync(appointment.PetId.Value);
            if (pet is null)
            {
                throw new InvalidOperationException("Linked pet record was not found.");
            }

            appointment.IsExistingPatient = true;
            ApplyExistingPatientDetails(appointment, pet);
        }
        else
        {
            appointment.PetId = null;
            appointment.PatientNoInput = null;
        }

        if (string.IsNullOrWhiteSpace(appointment.PetName))
        {
            throw new ArgumentException("Pet name is required.");
        }

        if (string.IsNullOrWhiteSpace(appointment.Species))
        {
            throw new ArgumentException("Species is required.");
        }

        if (string.IsNullOrWhiteSpace(appointment.Sex))
        {
            throw new ArgumentException("Sex is required.");
        }

        if (string.IsNullOrWhiteSpace(appointment.ServiceType))
        {
            throw new ArgumentException("Service type is required.");
        }

        if (appointment.AppointmentDate == default)
        {
            throw new ArgumentException("Appointment date is required.");
        }

        if (appointment.AppointmentTime == default)
        {
            throw new ArgumentException("Appointment time is required.");
        }

        if (!AllowedAppointmentTimes.Contains(appointment.AppointmentTime))
        {
            throw new ArgumentException("Appointment time must use a 15-minute clinic interval.");
        }

        _ = NormalizeAllowedValue(appointment.ServiceType, AllowedServiceTypes, "Service type");

        if (!string.IsNullOrWhiteSpace(appointment.Status))
        {
            _ = NormalizeAllowedValue(appointment.Status, AllowedStatuses, "Status");
        }

        if (appointment.PetId.HasValue)
        {
            var pet = await _petRepository.GetByIdAsync(appointment.PetId.Value);
            if (pet is null)
            {
                throw new InvalidOperationException("Linked pet record was not found.");
            }
        }

        if (appointment.CreatedByUserId.HasValue)
        {
            var user = await _userRepository.GetByIdAsync(appointment.CreatedByUserId.Value);
            if (user is null)
            {
                throw new InvalidOperationException("Linked clinic user was not found.");
            }
        }
    }

    private static void NormalizeAppointment(Appointment appointment)
    {
        appointment.ServiceType = NormalizeAllowedValue(appointment.ServiceType, AllowedServiceTypes, "Service type");
        appointment.Status = string.IsNullOrWhiteSpace(appointment.Status)
            ? "Pending"
            : NormalizeAllowedValue(appointment.Status, AllowedStatuses, "Status");

        appointment.PatientNoInput = appointment.IsExistingPatient
            ? NormalizeOptionalText(appointment.PatientNoInput)
            : null;
        appointment.PetName = NormalizeRequiredText(appointment.PetName);
        appointment.Species = NormalizeRequiredText(appointment.Species);
        appointment.Breed = NormalizeOptionalText(appointment.Breed);
        appointment.Sex = NormalizeRequiredText(appointment.Sex);
        appointment.Color = NormalizeOptionalText(appointment.Color);
        appointment.ReasonForVisit = NormalizeOptionalText(appointment.ReasonForVisit);
        appointment.Remarks = NormalizeOptionalText(appointment.Remarks);
        appointment.CancellationReason = NormalizeCancellationReason(appointment.CancellationReason);
    }

    private static void ApplyExistingPatientDetails(Appointment appointment, Pet pet)
    {
        appointment.PetId = pet.PetId;
        appointment.PatientNoInput = pet.PatientNo;
        appointment.PetName = pet.PetName;
        appointment.Species = pet.Species;
        appointment.Breed = pet.Breed;
        appointment.Sex = pet.Sex;
        appointment.Color = pet.Color;
    }

    private async Task<string> GeneratePatientNoAsync()
    {
        var activePets = await _petRepository.GetAllAsync();
        var nextNumber = activePets
            .Select(p => TryParsePatientNumber(p.PatientNo))
            .DefaultIfEmpty(0)
            .Max() + 1;

        string patientNo;
        do
        {
            patientNo = $"PET-{nextNumber:0000}";
            nextNumber++;
        }
        while (await _petRepository.PatientNoExistsAsync(patientNo));

        return patientNo;
    }

    private static void ValidatePetRegistration(Pet pet)
    {
        if (string.IsNullOrWhiteSpace(pet.PetName))
        {
            throw new ArgumentException("Pet name is required before registering a patient.");
        }

        if (string.IsNullOrWhiteSpace(pet.Species))
        {
            throw new ArgumentException("Species is required before registering a patient.");
        }

        if (string.IsNullOrWhiteSpace(pet.Sex))
        {
            throw new ArgumentException("Sex is required before registering a patient.");
        }
    }

    private static string NormalizeRequiredText(string? rawValue)
    {
        return rawValue?.Trim() ?? string.Empty;
    }

    private static string? NormalizeOptionalText(string? rawValue)
    {
        return string.IsNullOrWhiteSpace(rawValue) ? null : rawValue.Trim();
    }

    private static string NormalizeAllowedValue(string rawValue, IEnumerable<string> allowedValues, string fieldName)
    {
        var value = rawValue.Trim();
        var normalizedValue = allowedValues.FirstOrDefault(item => item.Equals(value, StringComparison.OrdinalIgnoreCase));
        if (normalizedValue is null)
        {
            throw new ArgumentException($"{fieldName} must be one of the allowed values.");
        }

        return normalizedValue;
    }

    private async Task<Appointment?> FindAppointmentByPublicCodeAsync(string appointmentCode)
    {
        var normalizedCode = appointmentCode.Trim().ToUpperInvariant();
        var appointment = await _appointmentRepository.GetByAppointmentCodeAsync(normalizedCode);
        if (appointment is not null)
        {
            return appointment;
        }

        return TryGetAppointmentSequence(normalizedCode, out var appointmentId)
            ? await _appointmentRepository.GetByIdAsync(appointmentId)
            : null;
    }

    private static bool MatchesPublicIdentifier(Appointment appointment, string providedIdentifier)
    {
        var identifier = providedIdentifier.Trim();
        var allowedIdentifiers = new[]
        {
            appointment.PatientNoInput,
            appointment.Pet?.PatientNo,
            appointment.PetName,
            appointment.Pet?.PetName
        };

        return allowedIdentifiers.Any(value =>
            !string.IsNullOrWhiteSpace(value)
            && value.Trim().Equals(identifier, StringComparison.OrdinalIgnoreCase));
    }

    private static bool CanRegisterPatient(Appointment appointment)
    {
        return appointment.PetId is null
            && IsRegisterableStatus(appointment.Status)
            && !string.IsNullOrWhiteSpace(appointment.PetName)
            && !string.IsNullOrWhiteSpace(appointment.Species)
            && !string.IsNullOrWhiteSpace(appointment.Sex);
    }

    private static bool IsRegisterableStatus(string status)
    {
        return status.Equals("Confirmed", StringComparison.OrdinalIgnoreCase)
            || status.Equals("Completed", StringComparison.OrdinalIgnoreCase);
    }

    private static void ApplyCancellationState(Appointment appointment)
    {
        if (appointment.Status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
        {
            appointment.CancelledBy = string.IsNullOrWhiteSpace(appointment.CancelledBy)
                ? "Clinic"
                : appointment.CancelledBy.Trim();
            appointment.CancelledAt ??= DateTime.Now;
            return;
        }

        appointment.CancellationReason = null;
        appointment.CancelledBy = null;
        appointment.CancelledAt = null;
    }

    private static string? NormalizeCancellationReason(string? rawValue)
    {
        var value = NormalizeOptionalText(rawValue);
        if (value is not null && value.Length > 500)
        {
            throw new ArgumentException("Cancellation reason must be 500 characters or fewer.");
        }

        return value;
    }

    private static bool TryGetAppointmentSequence(string appointmentCode, out int appointmentId)
    {
        var lastDashIndex = appointmentCode.LastIndexOf('-');
        var sequenceText = lastDashIndex >= 0
            ? appointmentCode[(lastDashIndex + 1)..]
            : new string(appointmentCode.Where(char.IsDigit).ToArray());

        return int.TryParse(sequenceText, out appointmentId) && appointmentId > 0;
    }

    private static int TryParsePatientNumber(string patientNo)
    {
        var digits = new string(patientNo.Where(char.IsDigit).ToArray());
        return int.TryParse(digits, out var number) ? number : 0;
    }

    private static string BuildAppointmentCode(Appointment appointment)
    {
        var year = appointment.CreatedAt == default ? DateTime.Now.Year : appointment.CreatedAt.Year;
        return $"APT-{year}-{appointment.AppointmentId:0000}";
    }

    private static TimeOnly[] BuildAllowedAppointmentTimes()
    {
        var start = new TimeOnly(8, 0);
        var end = new TimeOnly(17, 0);
        var values = new List<TimeOnly>();

        for (var time = start; time <= end; time = time.AddMinutes(15))
        {
            values.Add(time);
        }

        return values.ToArray();
    }
}
