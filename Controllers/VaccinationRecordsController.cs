using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SchiperkeWebApp.Models.Database;
using SchiperkeWebApp.Models.ViewModels;
using SchiperkeWebApp.Services.Interfaces;

namespace SchiperkeWebApp.Controllers;

public class VaccinationRecordsController : Controller
{
    private readonly IVaccinationRecordService _vaccinationRecordService;
    private readonly IPetService _petService;
    private readonly IUserService _userService;

    public VaccinationRecordsController(
        IVaccinationRecordService vaccinationRecordService,
        IPetService petService,
        IUserService userService)
    {
        _vaccinationRecordService = vaccinationRecordService;
        _petService = petService;
        _userService = userService;
    }

    public async Task<IActionResult> Index(int? petId, string? dueFilter)
    {
        var records = dueFilter switch
        {
            "upcoming" => await _vaccinationRecordService.GetUpcomingDueAsync(),
            "overdue" => await _vaccinationRecordService.GetOverdueAsync(),
            _ => await _vaccinationRecordService.GetAllAsync()
        };

        if (petId.HasValue)
        {
            records = records.Where(r => r.PetId == petId.Value).ToList();
        }

        ViewData["PetId"] = petId?.ToString();
        ViewData["DueFilter"] = dueFilter;
        ViewData["PetOptions"] = (await _petService.GetAllAsync())
            .Select(p => new SelectListItem($"{p.PatientNo} - {p.PetName}", p.PetId.ToString(), p.PetId == petId));

        return View(records);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var record = await _vaccinationRecordService.GetByIdAsync(id.Value);
        return record is null ? NotFound() : View(record);
    }

    public async Task<IActionResult> Create()
    {
        return View(await BuildFormAsync(new VaccinationRecordFormViewModel()));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(VaccinationRecordFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(await BuildFormAsync(model));
        }

        try
        {
            await _vaccinationRecordService.CreateAsync(MapToEntity(model));
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(await BuildFormAsync(model));
        }
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var record = await _vaccinationRecordService.GetByIdAsync(id.Value);
        if (record is null)
        {
            return NotFound();
        }

        return View(await BuildFormAsync(MapToFormModel(record)));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, VaccinationRecordFormViewModel model)
    {
        if (id != model.VaccinationId)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(await BuildFormAsync(model));
        }

        try
        {
            await _vaccinationRecordService.UpdateAsync(MapToEntity(model));
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(await BuildFormAsync(model));
        }
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var record = await _vaccinationRecordService.GetByIdAsync(id.Value);
        return record is null ? NotFound() : View(record);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _vaccinationRecordService.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    private async Task<VaccinationRecordFormViewModel> BuildFormAsync(VaccinationRecordFormViewModel model)
    {
        var pets = await _petService.GetAllAsync();
        var users = await _userService.GetAllAsync();

        model.PetOptions = pets.Select(p => new SelectListItem($"{p.PatientNo} - {p.PetName}", p.PetId.ToString(), p.PetId == model.PetId));
        model.UserOptions = users.Select(u => new SelectListItem($"{u.FullName} ({u.Role})", u.UserId.ToString(), u.UserId == model.RecordedByUserId));

        return model;
    }

    private static VaccinationRecordFormViewModel MapToFormModel(VaccinationRecord record)
    {
        return new VaccinationRecordFormViewModel
        {
            VaccinationId = record.VaccinationId,
            PetId = record.PetId,
            AppointmentId = record.AppointmentId,
            VaccineName = record.VaccineName,
            DateGiven = record.DateGiven,
            NextDueDate = record.NextDueDate,
            Dose = record.Dose,
            Route = record.Route,
            Manufacturer = record.Manufacturer,
            LotNumber = record.LotNumber,
            Remarks = record.Remarks,
            RecordedByUserId = record.RecordedByUserId
        };
    }

    private static VaccinationRecord MapToEntity(VaccinationRecordFormViewModel model)
    {
        return new VaccinationRecord
        {
            VaccinationId = model.VaccinationId,
            PetId = model.PetId,
            AppointmentId = model.AppointmentId,
            VaccineName = model.VaccineName,
            DateGiven = model.DateGiven,
            NextDueDate = model.NextDueDate,
            Dose = model.Dose,
            Route = model.Route,
            Manufacturer = model.Manufacturer,
            LotNumber = model.LotNumber,
            Remarks = model.Remarks,
            RecordedByUserId = model.RecordedByUserId
        };
    }
}
