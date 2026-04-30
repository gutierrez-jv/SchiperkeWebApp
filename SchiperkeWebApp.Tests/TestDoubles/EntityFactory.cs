using SchiperkeWebApp.Models.Database;

namespace SchiperkeWebApp.Tests.TestDoubles;

internal static class EntityFactory
{
    public static User CreateUser(
        int userId = 1,
        string username = "vet.user",
        string fullName = "Clinic User",
        string role = "Staff",
        string passwordHash = "hash",
        bool isActive = true)
    {
        return new User
        {
            UserId = userId,
            Username = username,
            FullName = fullName,
            Role = role,
            PasswordHash = passwordHash,
            IsActive = isActive,
            CreatedAt = new DateTime(2026, 1, 1)
        };
    }

    public static Pet CreatePet(
        int petId = 1,
        string patientNo = "P-001",
        string petName = "Buddy",
        string species = "Dog",
        string sex = "Male",
        bool isActive = true)
    {
        return new Pet
        {
            PetId = petId,
            PatientNo = patientNo,
            PetName = petName,
            Species = species,
            Sex = sex,
            IsActive = isActive,
            CreatedAt = new DateTime(2026, 1, 1)
        };
    }

    public static Appointment CreateAppointment(
        int appointmentId = 1,
        int petId = 1,
        int createdByUserId = 1,
        bool isDeleted = false)
    {
        return new Appointment
        {
            AppointmentId = appointmentId,
            PetId = petId,
            CreatedByUserId = createdByUserId,
            AppointmentDate = new DateOnly(2026, 5, 1),
            AppointmentTime = new TimeOnly(9, 0),
            ServiceType = "Consultation",
            Status = "Pending",
            CreatedAt = new DateTime(2026, 1, 1),
            IsDeleted = isDeleted
        };
    }

    public static ConsultationRecord CreateConsultationRecord(
        int consultationId = 1,
        int petId = 1,
        int recordedByUserId = 1,
        int? appointmentId = null,
        bool isDeleted = false)
    {
        return new ConsultationRecord
        {
            ConsultationId = consultationId,
            PetId = petId,
            AppointmentId = appointmentId,
            RecordedByUserId = recordedByUserId,
            ConsultationDate = new DateTime(2026, 5, 1),
            ChiefComplaint = "Loss of appetite",
            CreatedAt = new DateTime(2026, 1, 1),
            IsDeleted = isDeleted
        };
    }

    public static VaccinationRecord CreateVaccinationRecord(
        int vaccinationId = 1,
        int petId = 1,
        int recordedByUserId = 1,
        int? appointmentId = null,
        bool isDeleted = false)
    {
        return new VaccinationRecord
        {
            VaccinationId = vaccinationId,
            PetId = petId,
            AppointmentId = appointmentId,
            RecordedByUserId = recordedByUserId,
            VaccineName = "5-in-1",
            DateGiven = new DateOnly(2026, 5, 1),
            CreatedAt = new DateTime(2026, 1, 1),
            IsDeleted = isDeleted
        };
    }

    public static WellnessRecord CreateWellnessRecord(
        int wellnessId = 1,
        int petId = 1,
        int recordedByUserId = 1,
        int? appointmentId = null,
        bool isDeleted = false)
    {
        return new WellnessRecord
        {
            WellnessId = wellnessId,
            PetId = petId,
            AppointmentId = appointmentId,
            RecordedByUserId = recordedByUserId,
            WellnessType = "Deworming",
            ProductOrMedication = "Dewormer",
            DateGiven = new DateOnly(2026, 5, 1),
            CreatedAt = new DateTime(2026, 1, 1),
            IsDeleted = isDeleted
        };
    }
}
