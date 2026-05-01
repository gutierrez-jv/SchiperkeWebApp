using SchiperkeWebApp.Models.Database;
using SchiperkeWebApp.Repositories.Interfaces;
using SchiperkeWebApp.Services.Interfaces;

namespace SchiperkeWebApp.Services.Implementations;

public class PetService : IPetService
{
    private readonly IPetRepository _petRepository;

    public PetService(IPetRepository petRepository)
    {
        _petRepository = petRepository;
    }

    public async Task<List<Pet>> GetAllAsync()
    {
        return await _petRepository.GetAllAsync();
    }

    public async Task<Pet?> GetByIdAsync(int id)
    {
        return await _petRepository.GetByIdAsync(id);
    }

    public async Task<Pet?> GetByPatientNoAsync(string patientNo)
    {
        if (string.IsNullOrWhiteSpace(patientNo))
        {
            throw new ArgumentException("Patient number is required.");
        }

        return await _petRepository.GetByPatientNoAsync(patientNo.Trim());
    }

    public async Task<List<Pet>> SearchAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return await _petRepository.GetAllAsync();
        }

        return await _petRepository.SearchAsync(searchTerm.Trim());
    }

    public async Task<string> GeneratePatientNoAsync()
    {
        var activePets = await _petRepository.GetAllAsync();
        var nextNumber = activePets
            .Select(p => TryParsePatientNumber(p.PatientNo))
            .DefaultIfEmpty(0)
            .Max() + 1;

        string patientNo;
        do
        {
            patientNo = $"PET-{nextNumber:0000}";
            nextNumber++;
        }
        while (await _petRepository.GetByPatientNoAsync(patientNo) is not null);

        return patientNo;
    }

    public async Task CreateAsync(Pet pet)
    {
        PreparePetForSave(pet);

        var existingPet = await _petRepository.GetByPatientNoAsync(pet.PatientNo);
        if (existingPet is not null)
        {
            throw new InvalidOperationException("Patient number already exists.");
        }

        pet.IsActive = true;
        await _petRepository.AddAsync(pet);
        await _petRepository.SaveAsync();
    }

    public async Task UpdateAsync(Pet pet)
    {
        PreparePetForSave(pet);

        var existingPet = await _petRepository.GetByIdAsync(pet.PetId);
        if (existingPet is null)
        {
            throw new InvalidOperationException("Active pet record was not found.");
        }

        var duplicatePet = await _petRepository.GetByPatientNoAsync(pet.PatientNo);
        if (duplicatePet is not null && duplicatePet.PetId != pet.PetId)
        {
            throw new InvalidOperationException("Patient number already exists.");
        }

        pet.IsActive = existingPet.IsActive;
        pet.CreatedAt = existingPet.CreatedAt;

        _petRepository.Update(pet);
        await _petRepository.SaveAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var pet = await _petRepository.GetByIdAsync(id);
        if (pet is null)
        {
            return;
        }

        // Soft delete only: do not physically remove records from the database.
        _petRepository.Delete(pet);
        await _petRepository.SaveAsync();
    }

    private static void PreparePetForSave(Pet pet)
    {
        if (string.IsNullOrWhiteSpace(pet.PatientNo))
        {
            throw new ArgumentException("Patient number is required.");
        }

        if (string.IsNullOrWhiteSpace(pet.PetName))
        {
            throw new ArgumentException("Pet name is required.");
        }

        if (string.IsNullOrWhiteSpace(pet.Species))
        {
            throw new ArgumentException("Species is required.");
        }

        if (string.IsNullOrWhiteSpace(pet.Sex))
        {
            throw new ArgumentException("Sex is required.");
        }

        pet.PatientNo = pet.PatientNo.Trim();
        pet.PetName = pet.PetName.Trim();
        pet.Species = pet.Species.Trim();
        pet.Sex = pet.Sex.Trim();
        pet.Breed = string.IsNullOrWhiteSpace(pet.Breed) ? null : pet.Breed.Trim();
        pet.Color = string.IsNullOrWhiteSpace(pet.Color) ? null : pet.Color.Trim();
        pet.Notes = string.IsNullOrWhiteSpace(pet.Notes) ? null : pet.Notes.Trim();

        if (pet.Weight.HasValue && pet.Weight.Value <= 0)
        {
            throw new ArgumentException("Weight must be greater than zero when provided.");
        }

        if (pet.BirthDate.HasValue)
        {
            if (pet.BirthDate.Value > DateOnly.FromDateTime(DateTime.Today))
            {
                throw new ArgumentException("Birth date cannot be in the future.");
            }

            pet.AgeYears = CalculateAgeYears(pet.BirthDate.Value);
        }
        else if (pet.AgeYears.HasValue && pet.AgeYears.Value < 0)
        {
            throw new ArgumentException("Age cannot be negative.");
        }
    }

    private static int CalculateAgeYears(DateOnly birthDate)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var age = today.Year - birthDate.Year;

        if (birthDate > today.AddYears(-age))
        {
            age--;
        }

        return Math.Max(age, 0);
    }

    private static int TryParsePatientNumber(string patientNo)
    {
        var digits = new string(patientNo.Where(char.IsDigit).ToArray());
        return int.TryParse(digits, out var number) ? number : 0;
    }
}
