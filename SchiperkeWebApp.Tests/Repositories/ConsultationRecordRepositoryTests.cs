using SchiperkeWebApp.Repositories.Implementations;
using SchiperkeWebApp.Tests.TestDoubles;

namespace SchiperkeWebApp.Tests.Repositories;

public class ConsultationRecordRepositoryTests
{
    [Fact]
    public async Task GetByDateRangeAsync_ShouldExcludeDeletedRecords()
    {
        using var helper = new SqliteRepositoryTestHelper();
        using var context = helper.CreateContext();

        context.Users.Add(EntityFactory.CreateUser());
        context.Pets.Add(EntityFactory.CreatePet());
        context.ConsultationRecords.AddRange(
            EntityFactory.CreateConsultationRecord(consultationId: 1, isDeleted: false),
            EntityFactory.CreateConsultationRecord(consultationId: 2, isDeleted: true));
        await context.SaveChangesAsync();

        var repository = new ConsultationRecordRepository(context);

        var result = await repository.GetByDateRangeAsync(new DateTime(2026, 5, 1), new DateTime(2026, 5, 2));

        Assert.Single(result);
        Assert.Equal(1, result[0].ConsultationId);
    }
}
