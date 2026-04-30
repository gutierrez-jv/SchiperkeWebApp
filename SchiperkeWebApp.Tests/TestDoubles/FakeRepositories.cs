using SchiperkeWebApp.Models.Database;
using SchiperkeWebApp.Repositories.Interfaces;

namespace SchiperkeWebApp.Tests.TestDoubles;

internal sealed class FakeUserRepository : IUserRepository
{
    public List<User> Users { get; }
    public bool SaveCalled { get; private set; }

    public FakeUserRepository(IEnumerable<User>? users = null)
    {
        Users = users?.ToList() ?? [];
    }

    public Task<List<User>> GetAllAsync() =>
        Task.FromResult(Users.Where(u => u.IsActive).OrderBy(u => u.FullName).ToList());

    public Task<User?> GetByIdAsync(int id) =>
        Task.FromResult(Users.FirstOrDefault(u => u.UserId == id && u.IsActive));

    public Task<User?> GetByUsernameAsync(string username) =>
        Task.FromResult(Users.FirstOrDefault(u => u.Username == username && u.IsActive));

    public Task AddAsync(User user)
    {
        Users.Add(user);
        return Task.CompletedTask;
    }

    public void Update(User user)
    {
        var index = Users.FindIndex(u => u.UserId == user.UserId);
        if (index >= 0)
        {
            Users[index] = user;
        }
    }

    public void Delete(User user)
    {
        var existing = Users.First(u => u.UserId == user.UserId);
        existing.IsActive = false;
    }

    public Task SaveAsync()
    {
        SaveCalled = true;
        return Task.CompletedTask;
    }
}

internal sealed class FakePetRepository : IPetRepository
{
    public List<Pet> Pets { get; }
    public bool SaveCalled { get; private set; }

    public FakePetRepository(IEnumerable<Pet>? pets = null)
    {
        Pets = pets?.ToList() ?? [];
    }

    public Task<List<Pet>> GetAllAsync() =>
        Task.FromResult(Pets.Where(p => p.IsActive).OrderBy(p => p.PetName).ToList());

    public Task<Pet?> GetByIdAsync(int id) =>
        Task.FromResult(Pets.FirstOrDefault(p => p.PetId == id && p.IsActive));

    public Task<Pet?> GetByPatientNoAsync(string patientNo) =>
        Task.FromResult(Pets.FirstOrDefault(p => p.PatientNo == patientNo && p.IsActive));

    public Task<List<Pet>> SearchAsync(string searchTerm) =>
        Task.FromResult(Pets
            .Where(p => p.IsActive &&
                        (p.PatientNo.Contains(searchTerm) ||
                         p.PetName.Contains(searchTerm) ||
                         p.Species.Contains(searchTerm) ||
                         (p.Breed?.Contains(searchTerm) ?? false)))
            .OrderBy(p => p.PetName)
            .ToList());

    public Task AddAsync(Pet pet)
    {
        Pets.Add(pet);
        return Task.CompletedTask;
    }

    public void Update(Pet pet)
    {
        var index = Pets.FindIndex(p => p.PetId == pet.PetId);
        if (index >= 0)
        {
            Pets[index] = pet;
        }
    }

    public void Delete(Pet pet)
    {
        var existing = Pets.First(p => p.PetId == pet.PetId);
        existing.IsActive = false;
    }

    public Task SaveAsync()
    {
        SaveCalled = true;
        return Task.CompletedTask;
    }
}

internal sealed class FakeAppointmentRepository : IAppointmentRepository
{
    public List<Appointment> Appointments { get; }
    public bool SaveCalled { get; private set; }

    public FakeAppointmentRepository(IEnumerable<Appointment>? appointments = null)
    {
        Appointments = appointments?.ToList() ?? [];
    }

    public Task<List<Appointment>> GetAllAsync() =>
        Task.FromResult(Appointments.Where(a => !a.IsDeleted).ToList());

    public Task<Appointment?> GetByIdAsync(int id) =>
        Task.FromResult(Appointments.FirstOrDefault(a => a.AppointmentId == id && !a.IsDeleted));

    public Task<List<Appointment>> GetByPetIdAsync(int petId) =>
        Task.FromResult(Appointments.Where(a => a.PetId == petId && !a.IsDeleted).ToList());

    public Task<List<Appointment>> GetByDateAsync(DateOnly appointmentDate) =>
        Task.FromResult(Appointments.Where(a => a.AppointmentDate == appointmentDate && !a.IsDeleted).ToList());

    public Task<List<Appointment>> GetByStatusAsync(string status) =>
        Task.FromResult(Appointments.Where(a => a.Status == status && !a.IsDeleted).ToList());

    public Task AddAsync(Appointment appointment)
    {
        Appointments.Add(appointment);
        return Task.CompletedTask;
    }

    public void Update(Appointment appointment)
    {
        var index = Appointments.FindIndex(a => a.AppointmentId == appointment.AppointmentId);
        if (index >= 0)
        {
            Appointments[index] = appointment;
        }
    }

    public void Delete(Appointment appointment)
    {
        var existing = Appointments.First(a => a.AppointmentId == appointment.AppointmentId);
        existing.IsDeleted = true;
    }

    public Task SaveAsync()
    {
        SaveCalled = true;
        return Task.CompletedTask;
    }
}

internal sealed class FakeConsultationRecordRepository : IConsultationRecordRepository
{
    public List<ConsultationRecord> ConsultationRecords { get; }
    public bool SaveCalled { get; private set; }

    public FakeConsultationRecordRepository(IEnumerable<ConsultationRecord>? consultationRecords = null)
    {
        ConsultationRecords = consultationRecords?.ToList() ?? [];
    }

    public Task<List<ConsultationRecord>> GetAllAsync() =>
        Task.FromResult(ConsultationRecords.Where(c => !c.IsDeleted).ToList());

    public Task<ConsultationRecord?> GetByIdAsync(int id) =>
        Task.FromResult(ConsultationRecords.FirstOrDefault(c => c.ConsultationId == id && !c.IsDeleted));

    public Task<List<ConsultationRecord>> GetByPetIdAsync(int petId) =>
        Task.FromResult(ConsultationRecords.Where(c => c.PetId == petId && !c.IsDeleted).ToList());

    public Task<List<ConsultationRecord>> GetByDateRangeAsync(DateTime startDate, DateTime endDate) =>
        Task.FromResult(ConsultationRecords
            .Where(c => !c.IsDeleted && c.ConsultationDate >= startDate && c.ConsultationDate <= endDate)
            .ToList());

    public Task AddAsync(ConsultationRecord consultationRecord)
    {
        ConsultationRecords.Add(consultationRecord);
        return Task.CompletedTask;
    }

    public void Update(ConsultationRecord consultationRecord)
    {
        var index = ConsultationRecords.FindIndex(c => c.ConsultationId == consultationRecord.ConsultationId);
        if (index >= 0)
        {
            ConsultationRecords[index] = consultationRecord;
        }
    }

    public void Delete(ConsultationRecord consultationRecord)
    {
        var existing = ConsultationRecords.First(c => c.ConsultationId == consultationRecord.ConsultationId);
        existing.IsDeleted = true;
    }

    public Task SaveAsync()
    {
        SaveCalled = true;
        return Task.CompletedTask;
    }
}

internal sealed class FakeVaccinationRecordRepository : IVaccinationRecordRepository
{
    public List<VaccinationRecord> VaccinationRecords { get; }
    public bool SaveCalled { get; private set; }

    public FakeVaccinationRecordRepository(IEnumerable<VaccinationRecord>? vaccinationRecords = null)
    {
        VaccinationRecords = vaccinationRecords?.ToList() ?? [];
    }

    public Task<List<VaccinationRecord>> GetAllAsync() =>
        Task.FromResult(VaccinationRecords.Where(v => !v.IsDeleted).ToList());

    public Task<VaccinationRecord?> GetByIdAsync(int id) =>
        Task.FromResult(VaccinationRecords.FirstOrDefault(v => v.VaccinationId == id && !v.IsDeleted));

    public Task<List<VaccinationRecord>> GetByPetIdAsync(int petId) =>
        Task.FromResult(VaccinationRecords.Where(v => v.PetId == petId && !v.IsDeleted).ToList());

    public Task<List<VaccinationRecord>> GetUpcomingDueAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var limit = today.AddDays(30);
        return Task.FromResult(VaccinationRecords
            .Where(v => !v.IsDeleted &&
                        v.NextDueDate.HasValue &&
                        v.NextDueDate.Value >= today &&
                        v.NextDueDate.Value <= limit)
            .ToList());
    }

    public Task<List<VaccinationRecord>> GetOverdueAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        return Task.FromResult(VaccinationRecords
            .Where(v => !v.IsDeleted &&
                        v.NextDueDate.HasValue &&
                        v.NextDueDate.Value < today)
            .ToList());
    }

    public Task AddAsync(VaccinationRecord vaccinationRecord)
    {
        VaccinationRecords.Add(vaccinationRecord);
        return Task.CompletedTask;
    }

    public void Update(VaccinationRecord vaccinationRecord)
    {
        var index = VaccinationRecords.FindIndex(v => v.VaccinationId == vaccinationRecord.VaccinationId);
        if (index >= 0)
        {
            VaccinationRecords[index] = vaccinationRecord;
        }
    }

    public void Delete(VaccinationRecord vaccinationRecord)
    {
        var existing = VaccinationRecords.First(v => v.VaccinationId == vaccinationRecord.VaccinationId);
        existing.IsDeleted = true;
    }

    public Task SaveAsync()
    {
        SaveCalled = true;
        return Task.CompletedTask;
    }
}

internal sealed class FakeWellnessRecordRepository : IWellnessRecordRepository
{
    public List<WellnessRecord> WellnessRecords { get; }
    public bool SaveCalled { get; private set; }

    public FakeWellnessRecordRepository(IEnumerable<WellnessRecord>? wellnessRecords = null)
    {
        WellnessRecords = wellnessRecords?.ToList() ?? [];
    }

    public Task<List<WellnessRecord>> GetAllAsync() =>
        Task.FromResult(WellnessRecords.Where(w => !w.IsDeleted).ToList());

    public Task<WellnessRecord?> GetByIdAsync(int id) =>
        Task.FromResult(WellnessRecords.FirstOrDefault(w => w.WellnessId == id && !w.IsDeleted));

    public Task<List<WellnessRecord>> GetByPetIdAsync(int petId) =>
        Task.FromResult(WellnessRecords.Where(w => w.PetId == petId && !w.IsDeleted).ToList());

    public Task<List<WellnessRecord>> GetByWellnessTypeAsync(string wellnessType) =>
        Task.FromResult(WellnessRecords.Where(w => w.WellnessType == wellnessType && !w.IsDeleted).ToList());

    public Task<List<WellnessRecord>> GetUpcomingDueAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var limit = today.AddDays(30);
        return Task.FromResult(WellnessRecords
            .Where(w => !w.IsDeleted &&
                        w.NextDueDate.HasValue &&
                        w.NextDueDate.Value >= today &&
                        w.NextDueDate.Value <= limit)
            .ToList());
    }

    public Task<List<WellnessRecord>> GetOverdueAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        return Task.FromResult(WellnessRecords
            .Where(w => !w.IsDeleted &&
                        w.NextDueDate.HasValue &&
                        w.NextDueDate.Value < today)
            .ToList());
    }

    public Task AddAsync(WellnessRecord wellnessRecord)
    {
        WellnessRecords.Add(wellnessRecord);
        return Task.CompletedTask;
    }

    public void Update(WellnessRecord wellnessRecord)
    {
        var index = WellnessRecords.FindIndex(w => w.WellnessId == wellnessRecord.WellnessId);
        if (index >= 0)
        {
            WellnessRecords[index] = wellnessRecord;
        }
    }

    public void Delete(WellnessRecord wellnessRecord)
    {
        var existing = WellnessRecords.First(w => w.WellnessId == wellnessRecord.WellnessId);
        existing.IsDeleted = true;
    }

    public Task SaveAsync()
    {
        SaveCalled = true;
        return Task.CompletedTask;
    }
}
