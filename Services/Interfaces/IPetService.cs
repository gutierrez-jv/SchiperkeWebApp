using SchiperkeWebApp.Models.Database;

namespace SchiperkeWebApp.Services.Interfaces;

public interface IPetService
{
    Task<List<Pet>> GetAllAsync();
    Task<Pet?> GetByIdAsync(int id);
    Task<Pet?> GetByPatientNoAsync(string patientNo);
    Task<List<Pet>> SearchAsync(string searchTerm);
    Task<string> GeneratePatientNoAsync();
    Task CreateAsync(Pet pet);
    Task UpdateAsync(Pet pet);
    Task DeleteAsync(int id);
}
