using Microsoft.AspNetCore.Mvc;
using SchiperkeWebApp.Models.Database;
using SchiperkeWebApp.Models.ViewModels;
using SchiperkeWebApp.Services.Interfaces;

namespace SchiperkeWebApp.Controllers;

public class PetsController : Controller
{
    private readonly IPetService _petService;
    private readonly IAppointmentService _appointmentService;
    private readonly IConsultationRecordService _consultationRecordService;
    private readonly IVaccinationRecordService _vaccinationRecordService;
    private readonly IWellnessRecordService _wellnessRecordService;

    public PetsController(
        IPetService petService,
        IAppointmentService appointmentService,
        IConsultationRecordService consultationRecordService,
        IVaccinationRecordService vaccinationRecordService,
        IWellnessRecordService wellnessRecordService)
    {
        _petService = petService;
        _appointmentService = appointmentService;
        _consultationRecordService = consultationRecordService;
        _vaccinationRecordService = vaccinationRecordService;
        _wellnessRecordService = wellnessRecordService;
    }

    public async Task<IActionResult> Index(string? searchTerm)
    {
        var pets = string.IsNullOrWhiteSpace(searchTerm)
            ? await _petService.GetAllAsync()
            : await _petService.SearchAsync(searchTerm);

        ViewData["SearchTerm"] = searchTerm;
        return View(pets);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
        {
            return RedirectToAction(nameof(Index));
        }

        var pet = await _petService.GetByIdAsync(id.Value);
        if (pet is null)
        {
            return RedirectToAction(nameof(Index));
        }

        var model = new PetProfileViewModel
        {
            Pet = pet,
            Appointments = await _appointmentService.GetByPetIdAsync(pet.PetId),
            ConsultationRecords = await _consultationRecordService.GetByPetIdAsync(pet.PetId),
            VaccinationRecords = await _vaccinationRecordService.GetByPetIdAsync(pet.PetId),
            WellnessRecords = await _wellnessRecordService.GetByPetIdAsync(pet.PetId)
        };

        return View(model);
    }

    public async Task<IActionResult> Create()
    {
        return View(new PetFormViewModel
        {
            PatientNo = await _petService.GeneratePatientNoAsync()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PetFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await _petService.CreateAsync(MapToEntity(model));
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
        {
            return RedirectToAction(nameof(Index));
        }

        var pet = await _petService.GetByIdAsync(id.Value);
        if (pet is null)
        {
            return RedirectToAction(nameof(Index));
        }

        return View(MapToFormModel(pet));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PetFormViewModel model)
    {
        if (id != model.PetId)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await _petService.UpdateAsync(MapToEntity(model));
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
        {
            return RedirectToAction(nameof(Index));
        }

        var pet = await _petService.GetByIdAsync(id.Value);
        return pet is null
            ? RedirectToAction(nameof(Index))
            : View(pet);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _petService.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    private static PetFormViewModel MapToFormModel(Pet pet)
    {
        return new PetFormViewModel
        {
            PetId = pet.PetId,
            PatientNo = pet.PatientNo,
            PetName = pet.PetName,
            Species = pet.Species,
            Breed = pet.Breed,
            Sex = pet.Sex,
            BirthDate = pet.BirthDate,
            AgeYears = pet.AgeYears,
            Color = pet.Color,
            Weight = pet.Weight,
            Notes = pet.Notes
        };
    }

    private static Pet MapToEntity(PetFormViewModel model)
    {
        return new Pet
        {
            PetId = model.PetId,
            PatientNo = model.PatientNo ?? string.Empty,
            PetName = model.PetName,
            Species = model.Species,
            Breed = model.Breed,
            Sex = model.Sex,
            BirthDate = model.BirthDate,
            AgeYears = model.AgeYears,
            Color = model.Color,
            Weight = model.Weight,
            Notes = model.Notes
        };
    }
}
