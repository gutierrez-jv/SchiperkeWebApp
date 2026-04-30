using SchiperkeWebApp.Services.Implementations;
using SchiperkeWebApp.Tests.TestDoubles;

namespace SchiperkeWebApp.Tests.Services;

public class WellnessRecordServiceTests
{
    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenWellnessTypeLooksLikeVaccination()
    {
        var service = CreateService();
        var wellnessRecord = EntityFactory.CreateWellnessRecord();
        wellnessRecord.WellnessType = "Vaccination";

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(wellnessRecord));
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenAppointmentBelongsToDifferentPet()
    {
        var appointmentRepository = new FakeAppointmentRepository([
            EntityFactory.CreateAppointment(appointmentId: 22, petId: 2)
        ]);
        var service = CreateService(appointmentRepository: appointmentRepository);
        var wellnessRecord = EntityFactory.CreateWellnessRecord(appointmentId: 22, petId: 1);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(wellnessRecord));
    }

    [Fact]
    public async Task CreateAsync_ShouldTrimFields_AndSave()
    {
        var repository = new FakeWellnessRecordRepository();
        var service = CreateService(wellnessRecordRepository: repository);
        var wellnessRecord = EntityFactory.CreateWellnessRecord();

        wellnessRecord.WellnessType = "  Deworming  ";
        wellnessRecord.ProductOrMedication = "  Dewormer Plus  ";
        wellnessRecord.Dose = "  1 tab  ";
        wellnessRecord.Route = "  Oral  ";
        wellnessRecord.Remarks = "  Repeat after 2 weeks  ";

        await service.CreateAsync(wellnessRecord);

        Assert.True(repository.SaveCalled);
        Assert.Equal("Deworming", wellnessRecord.WellnessType);
        Assert.Equal("Dewormer Plus", wellnessRecord.ProductOrMedication);
        Assert.Equal("1 tab", wellnessRecord.Dose);
        Assert.Equal("Oral", wellnessRecord.Route);
        Assert.Equal("Repeat after 2 weeks", wellnessRecord.Remarks);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenDateGivenIsDefault()
    {
        var service = CreateService();
        var wellnessRecord = EntityFactory.CreateWellnessRecord();
        wellnessRecord.DateGiven = default;

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(wellnessRecord));
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteWellnessRecord_AndSave()
    {
        var repository = new FakeWellnessRecordRepository([EntityFactory.CreateWellnessRecord()]);
        var service = CreateService(wellnessRecordRepository: repository);

        await service.DeleteAsync(1);

        Assert.True(repository.SaveCalled);
        Assert.True(repository.WellnessRecords[0].IsDeleted);
    }

    private static WellnessRecordService CreateService(
        FakeWellnessRecordRepository? wellnessRecordRepository = null,
        FakePetRepository? petRepository = null,
        FakeUserRepository? userRepository = null,
        FakeAppointmentRepository? appointmentRepository = null)
    {
        return new WellnessRecordService(
            wellnessRecordRepository ?? new FakeWellnessRecordRepository(),
            petRepository ?? new FakePetRepository([EntityFactory.CreatePet()]),
            userRepository ?? new FakeUserRepository([EntityFactory.CreateUser()]),
            appointmentRepository ?? new FakeAppointmentRepository());
    }
}
