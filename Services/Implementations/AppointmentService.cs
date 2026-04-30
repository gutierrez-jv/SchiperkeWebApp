using SchiperkeWebApp.Models.Database;
using SchiperkeWebApp.Repositories.Interfaces;
using SchiperkeWebApp.Services.Interfaces;

namespace SchiperkeWebApp.Services.Implementations;

public class AppointmentService : IAppointmentService
{
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

        return await _appointmentRepository.GetByStatusAsync(status.Trim());
    }

    public async Task CreateAsync(Appointment appointment)
    {
        await ValidateAppointmentAsync(appointment);

        appointment.ServiceType = appointment.ServiceType.Trim();
        appointment.ReasonForVisit = string.IsNullOrWhiteSpace(appointment.ReasonForVisit)
            ? null
            : appointment.ReasonForVisit.Trim();
        appointment.Remarks = string.IsNullOrWhiteSpace(appointment.Remarks)
            ? null
            : appointment.Remarks.Trim();
        appointment.Status = string.IsNullOrWhiteSpace(appointment.Status)
            ? "Pending"
            : appointment.Status.Trim();

        await _appointmentRepository.AddAsync(appointment);
        await _appointmentRepository.SaveAsync();
    }

    public async Task UpdateAsync(Appointment appointment)
    {
        var existingAppointment = await _appointmentRepository.GetByIdAsync(appointment.AppointmentId);
        if (existingAppointment is null)
        {
            throw new InvalidOperationException("Appointment record was not found.");
        }

        await ValidateAppointmentAsync(appointment);

        appointment.ServiceType = appointment.ServiceType.Trim();
        appointment.ReasonForVisit = string.IsNullOrWhiteSpace(appointment.ReasonForVisit)
            ? null
            : appointment.ReasonForVisit.Trim();
        appointment.Remarks = string.IsNullOrWhiteSpace(appointment.Remarks)
            ? null
            : appointment.Remarks.Trim();
        appointment.Status = string.IsNullOrWhiteSpace(appointment.Status)
            ? existingAppointment.Status
            : appointment.Status.Trim();
        appointment.CreatedAt = existingAppointment.CreatedAt;
        appointment.IsDeleted = existingAppointment.IsDeleted;

        _appointmentRepository.Update(appointment);
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
        if (string.IsNullOrWhiteSpace(appointment.ServiceType))
        {
            throw new ArgumentException("Service type is required.");
        }

        if (appointment.AppointmentDate == default)
        {
            throw new ArgumentException("Appointment date is required.");
        }

        var pet = await _petRepository.GetByIdAsync(appointment.PetId);
        if (pet is null)
        {
            throw new InvalidOperationException("Appointment must reference an active pet.");
        }

        var user = await _userRepository.GetByIdAsync(appointment.CreatedByUserId);
        if (user is null)
        {
            throw new InvalidOperationException("Appointment must reference an active user.");
        }
    }
}
