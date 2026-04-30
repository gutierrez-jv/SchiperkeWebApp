using SchiperkeWebApp.Services.Implementations;
using SchiperkeWebApp.Tests.TestDoubles;

namespace SchiperkeWebApp.Tests.Services;

public class PetServiceTests
{
    [Fact]
    public async Task CreateAsync_ShouldTrimFields_SetPetActive_AndCalculateAgeYears()
    {
        var repository = new FakePetRepository();
        var service = new PetService(repository);
        var pet = EntityFactory.CreatePet(
            patientNo: "  P-001  ",
            petName: "  Buddy  ",
            species: "  Dog  ",
            sex: "  Male  ",
            isActive: false);

        pet.BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-5).AddDays(-10));
        pet.Breed = "  Aspin  ";
        pet.Color = "  Brown  ";
        pet.Notes = "  Good appetite  ";
        pet.Weight = 12.5m;

        await service.CreateAsync(pet);

        Assert.True(repository.SaveCalled);
        Assert.True(pet.IsActive);
        Assert.Equal("P-001", pet.PatientNo);
        Assert.Equal("Buddy", pet.PetName);
        Assert.Equal("Dog", pet.Species);
        Assert.Equal("Male", pet.Sex);
        Assert.Equal("Aspin", pet.Breed);
        Assert.Equal("Brown", pet.Color);
        Assert.Equal("Good appetite", pet.Notes);
        Assert.Equal(5, pet.AgeYears);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenPatientNumberAlreadyExists()
    {
        var repository = new FakePetRepository([EntityFactory.CreatePet(patientNo: "P-001")]);
        var service = new PetService(repository);
        var pet = EntityFactory.CreatePet(petId: 2, patientNo: "P-001");

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(pet));
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenBirthDateIsInTheFuture()
    {
        var repository = new FakePetRepository();
        var service = new PetService(repository);
        var pet = EntityFactory.CreatePet();
        pet.BirthDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1));

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(pet));
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnAllActivePets_WhenSearchTermIsBlank()
    {
        var repository = new FakePetRepository([
            EntityFactory.CreatePet(petId: 1, petName: "Buddy", isActive: true),
            EntityFactory.CreatePet(petId: 2, petName: "Charlie", patientNo: "P-002", isActive: false)
        ]);
        var service = new PetService(repository);

        var result = await service.SearchAsync("   ");

        Assert.Single(result);
        Assert.Equal("Buddy", result[0].PetName);
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeletePet_AndSave()
    {
        var repository = new FakePetRepository([EntityFactory.CreatePet()]);
        var service = new PetService(repository);

        await service.DeleteAsync(1);

        Assert.True(repository.SaveCalled);
        Assert.False(repository.Pets[0].IsActive);
    }
}
