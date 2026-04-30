using SchiperkeWebApp.Repositories.Implementations;
using SchiperkeWebApp.Tests.TestDoubles;

namespace SchiperkeWebApp.Tests.Repositories;

public class VaccinationRecordRepositoryTests
{
    [Fact]
    public async Task GetUpcomingDueAsync_ShouldExcludeDeletedRecords()
    {
        using var helper = new SqliteRepositoryTestHelper();
        using var context = helper.CreateContext();

        context.Users.Add(EntityFactory.CreateUser());
        context.Pets.Add(EntityFactory.CreatePet());
        var activeRecord = EntityFactory.CreateVaccinationRecord(vaccinationId: 1, isDeleted: false);
        activeRecord.NextDueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(10));

        var deletedRecord = EntityFactory.CreateVaccinationRecord(vaccinationId: 2, isDeleted: true);
        deletedRecord.NextDueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(10));

        context.VaccinationRecords.AddRange(activeRecord, deletedRecord);
        await context.SaveChangesAsync();

        var repository = new VaccinationRecordRepository(context);

        var result = await repository.GetUpcomingDueAsync();

        Assert.Single(result);
        Assert.Equal(1, result[0].VaccinationId);
    }

    [Fact]
    public async Task Delete_ShouldSoftDeleteVaccinationRecord()
    {
        using var helper = new SqliteRepositoryTestHelper();
        using var context = helper.CreateContext();

        context.Users.Add(EntityFactory.CreateUser());
        context.Pets.Add(EntityFactory.CreatePet());
        var vaccinationRecord = EntityFactory.CreateVaccinationRecord();
        context.VaccinationRecords.Add(vaccinationRecord);
        await context.SaveChangesAsync();

        var repository = new VaccinationRecordRepository(context);
        repository.Delete(vaccinationRecord);
        await repository.SaveAsync();

        Assert.True(vaccinationRecord.IsDeleted);
        Assert.Null(await repository.GetByIdAsync(vaccinationRecord.VaccinationId));
    }
}
