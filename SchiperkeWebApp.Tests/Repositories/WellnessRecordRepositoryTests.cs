using SchiperkeWebApp.Repositories.Implementations;
using SchiperkeWebApp.Tests.TestDoubles;

namespace SchiperkeWebApp.Tests.Repositories;

public class WellnessRecordRepositoryTests
{
    [Fact]
    public async Task GetUpcomingDueAsync_ShouldExcludeDeletedRecords()
    {
        using var helper = new SqliteRepositoryTestHelper();
        using var context = helper.CreateContext();

        context.Users.Add(EntityFactory.CreateUser());
        context.Pets.Add(EntityFactory.CreatePet());

        var activeRecord = EntityFactory.CreateWellnessRecord(wellnessId: 1, isDeleted: false);
        activeRecord.NextDueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7));

        var deletedRecord = EntityFactory.CreateWellnessRecord(wellnessId: 2, isDeleted: true);
        deletedRecord.NextDueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7));

        context.WellnessRecords.AddRange(activeRecord, deletedRecord);
        await context.SaveChangesAsync();

        var repository = new WellnessRecordRepository(context);

        var result = await repository.GetUpcomingDueAsync();

        Assert.Single(result);
        Assert.Equal(1, result[0].WellnessId);
    }

    [Fact]
    public async Task Delete_ShouldSoftDeleteWellnessRecord()
    {
        using var helper = new SqliteRepositoryTestHelper();
        using var context = helper.CreateContext();

        context.Users.Add(EntityFactory.CreateUser());
        context.Pets.Add(EntityFactory.CreatePet());
        var wellnessRecord = EntityFactory.CreateWellnessRecord();
        context.WellnessRecords.Add(wellnessRecord);
        await context.SaveChangesAsync();

        var repository = new WellnessRecordRepository(context);
        repository.Delete(wellnessRecord);
        await repository.SaveAsync();

        Assert.True(wellnessRecord.IsDeleted);
        Assert.Null(await repository.GetByIdAsync(wellnessRecord.WellnessId));
    }
}
