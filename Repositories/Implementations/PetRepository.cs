using Microsoft.EntityFrameworkCore;
using SchiperkeWebApp.Models.Database;
using SchiperkeWebApp.Repositories.Interfaces;

namespace SchiperkeWebApp.Repositories.Implementations;

public class PetRepository : IPetRepository
{
    private readonly SchiperkeDbContext _context;

    public PetRepository(SchiperkeDbContext context)
    {
        _context = context;
    }

    public async Task<List<Pet>> GetAllAsync()
    {
        return await _context.Pets
            .Where(p => p.IsActive)
            .OrderBy(p => p.PetName)
            .ToListAsync();
    }

    public async Task<Pet?> GetByIdAsync(int id)
    {
        return await _context.Pets
            .FirstOrDefaultAsync(p => p.PetId == id && p.IsActive);
    }

    public async Task<Pet?> GetByPatientNoAsync(string patientNo)
    {
        return await _context.Pets
            .FirstOrDefaultAsync(p => p.PatientNo == patientNo && p.IsActive);
    }

    public async Task<bool> PatientNoExistsAsync(string patientNo)
    {
        return await _context.Pets
            .AnyAsync(p => p.PatientNo == patientNo);
    }

    public async Task<List<Pet>> SearchAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return await GetAllAsync();
        }

        return await _context.Pets
            .Where(p => p.IsActive &&
                (p.PatientNo.Contains(searchTerm) ||
                 p.PetName.Contains(searchTerm) ||
                 p.Species.Contains(searchTerm) ||
                 (p.Breed != null && p.Breed.Contains(searchTerm))))
            .OrderBy(p => p.PetName)
            .ToListAsync();
    }

    public async Task AddAsync(Pet pet)
    {
        await _context.Pets.AddAsync(pet);
    }

    public void Update(Pet pet)
    {
        _context.Pets.Update(pet);
    }

    public void Delete(Pet pet)
    {
        pet.IsActive = false;
        _context.Pets.Update(pet);
    }

    public async Task SaveAsync()
    {
        await _context.SaveChangesAsync();
    }
}
