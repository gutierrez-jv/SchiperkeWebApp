using SchiperkeWebApp.Models.Database;

namespace SchiperkeWebApp.Repositories.Interfaces;

public interface IPetRepository
{
    Task<List<Pet>> GetAllAsync();
    Task<List<Pet>> GetAllIncludingInactiveAsync();
    Task<Pet?> GetByIdAsync(int id);
    Task<Pet?> GetByPatientNoAsync(string patientNo);
    Task<bool> PatientNoExistsAsync(string patientNo);
    Task<List<Pet>> SearchAsync(string searchTerm);
    Task AddAsync(Pet pet);
    void Update(Pet pet);
    void Delete(Pet pet);
    Task SaveAsync();
}
