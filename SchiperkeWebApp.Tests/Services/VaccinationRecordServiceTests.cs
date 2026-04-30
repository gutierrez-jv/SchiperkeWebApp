using SchiperkeWebApp.Services.Implementations;
using SchiperkeWebApp.Tests.TestDoubles;

namespace SchiperkeWebApp.Tests.Services;

public class VaccinationRecordServiceTests
{
    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenNextDueDateIsEarlierThanDateGiven()
    {
        var service = CreateService();
        var vaccinationRecord = EntityFactory.CreateVaccinationRecord();
        vaccinationRecord.NextDueDate = vaccinationRecord.DateGiven.AddDays(-1);

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(vaccinationRecord));
    }

    [Fact]
    public async Task CreateAsync_ShouldTrimOptionalFields_AndSave()
    {
        var repository = new FakeVaccinationRecordRepository();
        var service = CreateService(vaccinationRecordRepository: repository);
        var vaccinationRecord = EntityFactory.CreateVaccinationRecord();

        vaccinationRecord.VaccineName = "  5-in-1  ";
        vaccinationRecord.Dose = "  1 mL  ";
        vaccinationRecord.Route = "  SC  ";
        vaccinationRecord.Manufacturer = "  VetPharma  ";
        vaccinationRecord.LotNumber = "  LOT-001  ";
        vaccinationRecord.Remarks = "  Booster after 1 month  ";

        await service.CreateAsync(vaccinationRecord);

        Assert.True(repository.SaveCalled);
        Assert.Equal("5-in-1", vaccinationRecord.VaccineName);
        Assert.Equal("1 mL", vaccinationRecord.Dose);
        Assert.Equal("SC", vaccinationRecord.Route);
        Assert.Equal("VetPharma", vaccinationRecord.Manufacturer);
        Assert.Equal("LOT-001", vaccinationRecord.LotNumber);
        Assert.Equal("Booster after 1 month", vaccinationRecord.Remarks);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenDateGivenIsDefault()
    {
        var service = CreateService();
        var vaccinationRecord = EntityFactory.CreateVaccinationRecord();
        vaccinationRecord.DateGiven = default;

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(vaccinationRecord));
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteVaccinationRecord_AndSave()
    {
        var repository = new FakeVaccinationRecordRepository([EntityFactory.CreateVaccinationRecord()]);
        var service = CreateService(vaccinationRecordRepository: repository);

        await service.DeleteAsync(1);

        Assert.True(repository.SaveCalled);
        Assert.True(repository.VaccinationRecords[0].IsDeleted);
    }

    private static VaccinationRecordService CreateService(
        FakeVaccinationRecordRepository? vaccinationRecordRepository = null,
        FakePetRepository? petRepository = null,
        FakeUserRepository? userRepository = null,
        FakeAppointmentRepository? appointmentRepository = null)
    {
        return new VaccinationRecordService(
            vaccinationRecordRepository ?? new FakeVaccinationRecordRepository(),
            petRepository ?? new FakePetRepository([EntityFactory.CreatePet()]),
            userRepository ?? new FakeUserRepository([EntityFactory.CreateUser()]),
            appointmentRepository ?? new FakeAppointmentRepository());
    }
}
