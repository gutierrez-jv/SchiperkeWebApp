using SchiperkeWebApp.Models.Database;
using SchiperkeWebApp.Repositories.Implementations;
using SchiperkeWebApp.Tests.TestDoubles;

namespace SchiperkeWebApp.Tests.Repositories;

public class UserRepositoryTests
{
    [Fact]
    public async Task GetAllAsync_ShouldReturnOnlyActiveUsersOrderedByFullName()
    {
        using var helper = new SqliteRepositoryTestHelper();
        using var context = helper.CreateContext();

        context.Users.AddRange(
            EntityFactory.CreateUser(userId: 1, fullName: "Charlie Vet", isActive: true),
            EntityFactory.CreateUser(userId: 2, fullName: "Alpha Vet", username: "alpha", isActive: true),
            EntityFactory.CreateUser(userId: 3, fullName: "Inactive Vet", username: "inactive", isActive: false));
        await context.SaveChangesAsync();

        var repository = new UserRepository(context);

        var result = await repository.GetAllAsync();

        Assert.Equal(2, result.Count);
        Assert.Collection(result,
            user => Assert.Equal("Alpha Vet", user.FullName),
            user => Assert.Equal("Charlie Vet", user.FullName));
    }

    [Fact]
    public async Task Delete_ShouldSoftDeleteUser()
    {
        using var helper = new SqliteRepositoryTestHelper();
        using var context = helper.CreateContext();

        var user = EntityFactory.CreateUser();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var repository = new UserRepository(context);
        repository.Delete(user);
        await repository.SaveAsync();

        Assert.False(user.IsActive);
        Assert.Empty(await repository.GetAllAsync());
    }
}
