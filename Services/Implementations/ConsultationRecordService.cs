using SchiperkeWebApp.Models.Database;
using SchiperkeWebApp.Repositories.Interfaces;
using SchiperkeWebApp.Services.Interfaces;

namespace SchiperkeWebApp.Services.Implementations;

public class ConsultationRecordService : IConsultationRecordService
{
    private readonly IConsultationRecordRepository _consultationRecordRepository;
    private readonly IPetRepository _petRepository;
    private readonly IUserRepository _userRepository;
    private readonly IAppointmentRepository _appointmentRepository;

    public ConsultationRecordService(
        IConsultationRecordRepository consultationRecordRepository,
        IPetRepository petRepository,
        IUserRepository userRepository,
        IAppointmentRepository appointmentRepository)
    {
        _consultationRecordRepository = consultationRecordRepository;
        _petRepository = petRepository;
        _userRepository = userRepository;
        _appointmentRepository = appointmentRepository;
    }

    public async Task<List<ConsultationRecord>> GetAllAsync()
    {
        return await _consultationRecordRepository.GetAllAsync();
    }

    public async Task<ConsultationRecord?> GetByIdAsync(int id)
    {
        return await _consultationRecordRepository.GetByIdAsync(id);
    }

    public async Task<List<ConsultationRecord>> GetByPetIdAsync(int petId)
    {
        return await _consultationRecordRepository.GetByPetIdAsync(petId);
    }

    public async Task<List<ConsultationRecord>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        if (startDate > endDate)
        {
            throw new ArgumentException("Start date cannot be later than end date.");
        }

        return await _consultationRecordRepository.GetByDateRangeAsync(startDate, endDate);
    }

    public async Task CreateAsync(ConsultationRecord consultationRecord)
    {
        await ValidateConsultationRecordAsync(consultationRecord);
        NormalizeConsultationRecord(consultationRecord);

        if (consultationRecord.CreatedAt == default)
        {
            consultationRecord.CreatedAt = DateTime.Now;
        }

        consultationRecord.IsDeleted = false;

        await _consultationRecordRepository.AddAsync(consultationRecord);
        await _consultationRecordRepository.SaveAsync();
    }

    public async Task UpdateAsync(ConsultationRecord consultationRecord)
    {
        var existingConsultationRecord = await _consultationRecordRepository.GetByIdAsync(consultationRecord.ConsultationId);
        if (existingConsultationRecord is null)
        {
            throw new InvalidOperationException("Consultation record was not found.");
        }

        await ValidateConsultationRecordAsync(consultationRecord);
        NormalizeConsultationRecord(consultationRecord);

        existingConsultationRecord.PetId = consultationRecord.PetId;
        existingConsultationRecord.AppointmentId = consultationRecord.AppointmentId;
        existingConsultationRecord.ConsultationDate = consultationRecord.ConsultationDate;
        existingConsultationRecord.ChiefComplaint = consultationRecord.ChiefComplaint;
        existingConsultationRecord.History = consultationRecord.History;
        existingConsultationRecord.Vitals = consultationRecord.Vitals;
        existingConsultationRecord.PhysicalExamination = consultationRecord.PhysicalExamination;
        existingConsultationRecord.LabExam = consultationRecord.LabExam;
        existingConsultationRecord.Assessment = consultationRecord.Assessment;
        existingConsultationRecord.Treatment = consultationRecord.Treatment;
        existingConsultationRecord.Notes = consultationRecord.Notes;
        existingConsultationRecord.RecordedByUserId = consultationRecord.RecordedByUserId;

        _consultationRecordRepository.Update(existingConsultationRecord);
        await _consultationRecordRepository.SaveAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var consultationRecord = await _consultationRecordRepository.GetByIdAsync(id);
        if (consultationRecord is null)
        {
            return;
        }

        // Soft delete only: do not physically remove records from the database.
        _consultationRecordRepository.Delete(consultationRecord);
        await _consultationRecordRepository.SaveAsync();
    }

    private async Task ValidateConsultationRecordAsync(ConsultationRecord consultationRecord)
    {
        if (consultationRecord.ConsultationDate == default)
        {
            throw new ArgumentException("Consultation date is required.");
        }

        var pet = await _petRepository.GetByIdAsync(consultationRecord.PetId);
        if (pet is null)
        {
            throw new InvalidOperationException("Consultation record must reference an active pet.");
        }

        var user = await _userRepository.GetByIdAsync(consultationRecord.RecordedByUserId);
        if (user is null)
        {
            throw new InvalidOperationException("Consultation record must reference an active user.");
        }

        if (consultationRecord.AppointmentId.HasValue)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(consultationRecord.AppointmentId.Value);
            if (appointment is null)
            {
                throw new InvalidOperationException("Consultation record appointment was not found.");
            }

            if (!appointment.PetId.HasValue)
            {
                throw new InvalidOperationException("Public booking appointments are not linked directly to consultation records.");
            }

            if (appointment.PetId.Value != consultationRecord.PetId)
            {
                throw new InvalidOperationException("Consultation appointment does not belong to the selected pet.");
            }
        }

        if (!HasConsultationContent(consultationRecord))
        {
            throw new ArgumentException(
                "Consultation record must include at least one consultation detail such as chief complaint, history, vitals, physical examination, lab exam, assessment, treatment, or notes.");
        }
    }

    private static void NormalizeConsultationRecord(ConsultationRecord consultationRecord)
    {
        consultationRecord.ChiefComplaint = CleanText(consultationRecord.ChiefComplaint);
        consultationRecord.History = CleanText(consultationRecord.History);
        consultationRecord.Vitals = CleanText(consultationRecord.Vitals);
        consultationRecord.PhysicalExamination = CleanText(consultationRecord.PhysicalExamination);
        consultationRecord.LabExam = CleanText(consultationRecord.LabExam);
        consultationRecord.Assessment = CleanText(consultationRecord.Assessment);
        consultationRecord.Treatment = CleanText(consultationRecord.Treatment);
        consultationRecord.Notes = CleanText(consultationRecord.Notes);
    }

    private static bool HasConsultationContent(ConsultationRecord consultationRecord)
    {
        return !string.IsNullOrWhiteSpace(consultationRecord.ChiefComplaint) ||
               !string.IsNullOrWhiteSpace(consultationRecord.History) ||
               !string.IsNullOrWhiteSpace(consultationRecord.Vitals) ||
               !string.IsNullOrWhiteSpace(consultationRecord.PhysicalExamination) ||
               !string.IsNullOrWhiteSpace(consultationRecord.LabExam) ||
               !string.IsNullOrWhiteSpace(consultationRecord.Assessment) ||
               !string.IsNullOrWhiteSpace(consultationRecord.Treatment) ||
               !string.IsNullOrWhiteSpace(consultationRecord.Notes);
    }

    private static string? CleanText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
