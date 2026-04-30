using SchiperkeWebApp.Repositories.Implementations;
using SchiperkeWebApp.Tests.TestDoubles;

namespace SchiperkeWebApp.Tests.Repositories;

public class AppointmentRepositoryTests
{
    [Fact]
    public async Task GetAllAsync_ShouldExcludeDeletedAppointments()
    {
        using var helper = new SqliteRepositoryTestHelper();
        using var context = helper.CreateContext();

        context.Users.Add(EntityFactory.CreateUser());
        context.Pets.Add(EntityFactory.CreatePet());
        context.Appointments.AddRange(
            EntityFactory.CreateAppointment(appointmentId: 1, isDeleted: false),
            EntityFactory.CreateAppointment(appointmentId: 2, isDeleted: true));
        await context.SaveChangesAsync();

        var repository = new AppointmentRepository(context);

        var result = await repository.GetAllAsync();

        Assert.Single(result);
        Assert.Equal(1, result[0].AppointmentId);
    }

    [Fact]
    public async Task Delete_ShouldSoftDeleteAppointment()
    {
        using var helper = new SqliteRepositoryTestHelper();
        using var context = helper.CreateContext();

        context.Users.Add(EntityFactory.CreateUser());
        context.Pets.Add(EntityFactory.CreatePet());
        var appointment = EntityFactory.CreateAppointment();
        context.Appointments.Add(appointment);
        await context.SaveChangesAsync();

        var repository = new AppointmentRepository(context);
        repository.Delete(appointment);
        await repository.SaveAsync();

        Assert.True(appointment.IsDeleted);
        Assert.Null(await repository.GetByIdAsync(appointment.AppointmentId));
    }
}
