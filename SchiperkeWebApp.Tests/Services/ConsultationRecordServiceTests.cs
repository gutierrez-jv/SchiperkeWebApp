using SchiperkeWebApp.Services.Implementations;
using SchiperkeWebApp.Tests.TestDoubles;

namespace SchiperkeWebApp.Tests.Services;

public class ConsultationRecordServiceTests
{
    [Fact]
    public async Task GetByDateRangeAsync_ShouldThrow_WhenStartDateIsAfterEndDate()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.GetByDateRangeAsync(new DateTime(2026, 5, 2), new DateTime(2026, 5, 1)));
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenConsultationContentIsEmpty()
    {
        var service = CreateService();
        var consultationRecord = EntityFactory.CreateConsultationRecord();

        consultationRecord.ChiefComplaint = null;
        consultationRecord.History = null;
        consultationRecord.Vitals = null;
        consultationRecord.PhysicalExamination = null;
        consultationRecord.LabExam = null;
        consultationRecord.Assessment = null;
        consultationRecord.Treatment = null;
        consultationRecord.Notes = null;

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(consultationRecord));
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenAppointmentBelongsToDifferentPet()
    {
        var appointmentRepository = new FakeAppointmentRepository([
            EntityFactory.CreateAppointment(appointmentId: 10, petId: 2)
        ]);
        var service = CreateService(appointmentRepository: appointmentRepository);
        var consultationRecord = EntityFactory.CreateConsultationRecord(appointmentId: 10, petId: 1);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(consultationRecord));
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenConsultationDateIsDefault()
    {
        var service = CreateService();
        var consultationRecord = EntityFactory.CreateConsultationRecord();
        consultationRecord.ConsultationDate = default;

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(consultationRecord));
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteConsultationRecord_AndSave()
    {
        var repository = new FakeConsultationRecordRepository([EntityFactory.CreateConsultationRecord()]);
        var service = CreateService(consultationRecordRepository: repository);

        await service.DeleteAsync(1);

        Assert.True(repository.SaveCalled);
        Assert.True(repository.ConsultationRecords[0].IsDeleted);
    }

    private static ConsultationRecordService CreateService(
        FakeConsultationRecordRepository? consultationRecordRepository = null,
        FakePetRepository? petRepository = null,
        FakeUserRepository? userRepository = null,
        FakeAppointmentRepository? appointmentRepository = null)
    {
        return new ConsultationRecordService(
            consultationRecordRepository ?? new FakeConsultationRecordRepository(),
            petRepository ?? new FakePetRepository([EntityFactory.CreatePet()]),
            userRepository ?? new FakeUserRepository([EntityFactory.CreateUser()]),
            appointmentRepository ?? new FakeAppointmentRepository());
    }
}
