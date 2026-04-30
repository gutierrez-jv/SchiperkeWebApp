using SchiperkeWebApp.Repositories.Implementations;
using SchiperkeWebApp.Tests.TestDoubles;

namespace SchiperkeWebApp.Tests.Repositories;

public class PetRepositoryTests
{
    [Fact]
    public async Task SearchAsync_ShouldExcludeInactivePets()
    {
        using var helper = new SqliteRepositoryTestHelper();
        using var context = helper.CreateContext();

        context.Pets.AddRange(
            EntityFactory.CreatePet(petId: 1, petName: "Buddy", patientNo: "P-001", isActive: true),
            EntityFactory.CreatePet(petId: 2, petName: "Buddy", patientNo: "P-002", isActive: false));
        await context.SaveChangesAsync();

        var repository = new PetRepository(context);

        var result = await repository.SearchAsync("Buddy");

        Assert.Single(result);
        Assert.Equal("P-001", result[0].PatientNo);
    }

    [Fact]
    public async Task Delete_ShouldSoftDeletePet()
    {
        using var helper = new SqliteRepositoryTestHelper();
        using var context = helper.CreateContext();

        var pet = EntityFactory.CreatePet();
        context.Pets.Add(pet);
        await context.SaveChangesAsync();

        var repository = new PetRepository(context);
        repository.Delete(pet);
        await repository.SaveAsync();

        Assert.False(pet.IsActive);
        Assert.Null(await repository.GetByIdAsync(pet.PetId));
    }
}
