using SchiperkeWebApp.Services.Implementations;
using SchiperkeWebApp.Tests.TestDoubles;

namespace SchiperkeWebApp.Tests.Services;

public class UserServiceTests
{
    [Fact]
    public async Task CreateAsync_ShouldTrimFields_SetUserActive_AndSave()
    {
        var repository = new FakeUserRepository();
        var service = new UserService(repository);
        var user = EntityFactory.CreateUser(
            username: "  doctor1  ",
            fullName: "  Dr. Vet  ",
            role: "  Veterinarian  ",
            passwordHash: "  hash123  ",
            isActive: false);

        await service.CreateAsync(user);

        Assert.True(repository.SaveCalled);
        Assert.Single(repository.Users);
        Assert.Equal("doctor1", user.Username);
        Assert.Equal("Dr. Vet", user.FullName);
        Assert.Equal("Veterinarian", user.Role);
        Assert.Equal("hash123", user.PasswordHash);
        Assert.True(user.IsActive);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenUsernameAlreadyExists()
    {
        var repository = new FakeUserRepository([EntityFactory.CreateUser(username: "doctor1")]);
        var service = new UserService(repository);
        var user = EntityFactory.CreateUser(userId: 2, username: "doctor1");

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(user));
    }

    [Fact]
    public async Task UpdateAsync_ShouldPreserveCreatedAt_AndIsActive()
    {
        var existingUser = EntityFactory.CreateUser();
        var repository = new FakeUserRepository([existingUser]);
        var service = new UserService(repository);
        var updatedUser = EntityFactory.CreateUser(
            fullName: "  Updated Name  ",
            role: "  Staff  ",
            passwordHash: "  newhash  ");

        updatedUser.IsActive = false;
        updatedUser.CreatedAt = DateTime.MinValue;

        await service.UpdateAsync(updatedUser);

        Assert.True(repository.SaveCalled);
        Assert.Equal(existingUser.CreatedAt, updatedUser.CreatedAt);
        Assert.True(updatedUser.IsActive);
        Assert.Equal("Updated Name", updatedUser.FullName);
        Assert.Equal("Staff", updatedUser.Role);
        Assert.Equal("newhash", updatedUser.PasswordHash);
    }

    [Fact]
    public async Task GetByUsernameAsync_ShouldThrow_WhenUsernameIsBlank()
    {
        var repository = new FakeUserRepository();
        var service = new UserService(repository);

        await Assert.ThrowsAsync<ArgumentException>(() => service.GetByUsernameAsync("   "));
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteUser_AndSave()
    {
        var repository = new FakeUserRepository([EntityFactory.CreateUser()]);
        var service = new UserService(repository);

        await service.DeleteAsync(1);

        Assert.True(repository.SaveCalled);
        Assert.False(repository.Users[0].IsActive);
    }
}
