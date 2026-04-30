using SchiperkeWebApp.Services.Implementations;
using SchiperkeWebApp.Tests.TestDoubles;
using Xunit;

namespace SchiperkeWebApp.Tests.Services;

public class AppointmentServiceTests
{
    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenPetDoesNotExist()
    {
        var appointmentRepository = new FakeAppointmentRepository();
        var petRepository = new FakePetRepository();
        var userRepository = new FakeUserRepository([EntityFactory.CreateUser()]);
        var service = new AppointmentService(appointmentRepository, petRepository, userRepository);
        var appointment = EntityFactory.CreateAppointment();

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(appointment));
    }

    [Fact]
    public async Task CreateAsync_ShouldDefaultStatus_TrimText_AndSave()
    {
        var appointmentRepository = new FakeAppointmentRepository();
        var petRepository = new FakePetRepository([EntityFactory.CreatePet()]);
        var userRepository = new FakeUserRepository([EntityFactory.CreateUser()]);
        var service = new AppointmentService(appointmentRepository, petRepository, userRepository);
        var appointment = EntityFactory.CreateAppointment();

        appointment.ServiceType = "  Consultation  ";
        appointment.ReasonForVisit = "  Fever  ";
        appointment.Remarks = "  Needs follow-up  ";
        appointment.Status = "   ";

        await service.CreateAsync(appointment);

        Assert.True(appointmentRepository.SaveCalled);
        Assert.Equal("Consultation", appointment.ServiceType);
        Assert.Equal("Fever", appointment.ReasonForVisit);
        Assert.Equal("Needs follow-up", appointment.Remarks);
        Assert.Equal("Pending", appointment.Status);
    }

    [Fact]
    public async Task UpdateAsync_ShouldPreserveCreatedAt_AndIsDeleted()
    {
        var existingAppointment = EntityFactory.CreateAppointment(isDeleted: false);
        var appointmentRepository = new FakeAppointmentRepository([existingAppointment]);
        var petRepository = new FakePetRepository([EntityFactory.CreatePet()]);
        var userRepository = new FakeUserRepository([EntityFactory.CreateUser()]);
        var service = new AppointmentService(appointmentRepository, petRepository, userRepository);
        var updatedAppointment = EntityFactory.CreateAppointment();

        updatedAppointment.CreatedAt = DateTime.MinValue;
        updatedAppointment.IsDeleted = true;
        updatedAppointment.ServiceType = "  Wellness  ";

        await service.UpdateAsync(updatedAppointment);

        Assert.True(appointmentRepository.SaveCalled);
        Assert.Equal(existingAppointment.CreatedAt, updatedAppointment.CreatedAt);
        Assert.False(updatedAppointment.IsDeleted);
        Assert.Equal("Wellness", updatedAppointment.ServiceType);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenAppointmentDateIsDefault()
    {
        var appointmentRepository = new FakeAppointmentRepository();
        var petRepository = new FakePetRepository([EntityFactory.CreatePet()]);
        var userRepository = new FakeUserRepository([EntityFactory.CreateUser()]);
        var service = new AppointmentService(appointmentRepository, petRepository, userRepository);
        var appointment = EntityFactory.CreateAppointment();
        appointment.AppointmentDate = default;

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(appointment));
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteAppointment_AndSave()
    {
        var appointmentRepository = new FakeAppointmentRepository([EntityFactory.CreateAppointment()]);
        var petRepository = new FakePetRepository([EntityFactory.CreatePet()]);
        var userRepository = new FakeUserRepository([EntityFactory.CreateUser()]);
        var service = new AppointmentService(appointmentRepository, petRepository, userRepository);

        await service.DeleteAsync(1);

        Assert.True(appointmentRepository.SaveCalled);
        Assert.True(appointmentRepository.Appointments[0].IsDeleted);
    }
}
