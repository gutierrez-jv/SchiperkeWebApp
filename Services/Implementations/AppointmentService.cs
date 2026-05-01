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

    public async Task CreateAsync(Appointment appointment)
    {
        await ValidateAppointmentAsync(appointment);

        NormalizeAppointment(appointment);

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
