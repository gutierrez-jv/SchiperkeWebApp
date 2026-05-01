using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SchiperkeWebApp.Models.Database;
using SchiperkeWebApp.Models.ViewModels;
using SchiperkeWebApp.Services.Interfaces;

namespace SchiperkeWebApp.Controllers;

public class WellnessRecordsController : Controller
{
    private readonly IWellnessRecordService _wellnessRecordService;
    private readonly IPetService _petService;
    private readonly IUserService _userService;

    public WellnessRecordsController(
        IWellnessRecordService wellnessRecordService,
        IPetService petService,
        IUserService userService)
    {
        _wellnessRecordService = wellnessRecordService;
        _petService = petService;
        _userService = userService;
    }

    public async Task<IActionResult> Index(int? petId, string? dueFilter, string? wellnessType)
    {
        var records = dueFilter switch
        {
            "upcoming" => await _wellnessRecordService.GetUpcomingDueAsync(),
            "overdue" => await _wellnessRecordService.GetOverdueAsync(),
            _ => string.IsNullOrWhiteSpace(wellnessType)
                ? await _wellnessRecordService.GetAllAsync()
                : await _wellnessRecordService.GetByWellnessTypeAsync(wellnessType.Trim())
        };

        if (petId.HasValue)
        {
            records = records.Where(r => r.PetId == petId.Value).ToList();
        }

        if (!string.IsNullOrWhiteSpace(wellnessType) && dueFilter is "upcoming" or "overdue")
        {
            records = records
                .Where(r => r.WellnessType.Contains(wellnessType.Trim(), StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        ViewData["PetId"] = petId?.ToString();
        ViewData["DueFilter"] = dueFilter;
        ViewData["WellnessType"] = wellnessType;
        ViewData["PetOptions"] = (await _petService.GetAllAsync())
            .Select(p => new SelectListItem($"{p.PatientNo} - {p.PetName}", p.PetId.ToString(), p.PetId == petId));
        ViewData["WellnessTypeOptions"] = BuildValueOptions(_wellnessRecordService.GetAllowedWellnessTypes(), wellnessType);

        return View(records);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var record = await _wellnessRecordService.GetByIdAsync(id.Value);
        return record is null ? NotFound() : View(record);
    }

    public async Task<IActionResult> Create()
    {
        return View(await BuildFormAsync(new WellnessRecordFormViewModel()));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(WellnessRecordFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(await BuildFormAsync(model));
        }

        try
        {
            await _wellnessRecordService.CreateAsync(MapToEntity(model));
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

        var record = await _wellnessRecordService.GetByIdAsync(id.Value);
        if (record is null)
        {
            return NotFound();
        }

        return View(await BuildFormAsync(MapToFormModel(record)));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, WellnessRecordFormViewModel model)
    {
        if (id != model.WellnessId)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(await BuildFormAsync(model));
        }

        try
        {
            await _wellnessRecordService.UpdateAsync(MapToEntity(model));
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

        var record = await _wellnessRecordService.GetByIdAsync(id.Value);
        return record is null ? NotFound() : View(record);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _wellnessRecordService.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    private async Task<WellnessRecordFormViewModel> BuildFormAsync(WellnessRecordFormViewModel model)
    {
        var pets = await _petService.GetAllAsync();
        var users = await _userService.GetAllAsync();

        model.PetOptions = pets.Select(p => new SelectListItem($"{p.PatientNo} - {p.PetName}", p.PetId.ToString(), p.PetId == model.PetId));
        model.UserOptions = users.Select(u => new SelectListItem($"{u.FullName} ({u.Role})", u.UserId.ToString(), u.UserId == model.RecordedByUserId));
        model.WellnessTypeOptions = BuildValueOptions(_wellnessRecordService.GetAllowedWellnessTypes(), model.WellnessType);

        return model;
    }

    private static IEnumerable<SelectListItem> BuildValueOptions(IEnumerable<string> values, string? selectedValue)
    {
        return values.Select(value => new SelectListItem(value, value, value.Equals(selectedValue, StringComparison.OrdinalIgnoreCase)));
    }

    private static WellnessRecordFormViewModel MapToFormModel(WellnessRecord record)
    {
        return new WellnessRecordFormViewModel
        {
            WellnessId = record.WellnessId,
            PetId = record.PetId,
            AppointmentId = record.AppointmentId,
            WellnessType = record.WellnessType,
            ProductOrMedication = record.ProductOrMedication,
            DateGiven = record.DateGiven,
            NextDueDate = record.NextDueDate,
            Dose = record.Dose,
            Route = record.Route,
            Remarks = record.Remarks,
            RecordedByUserId = record.RecordedByUserId
        };
    }

    private static WellnessRecord MapToEntity(WellnessRecordFormViewModel model)
    {
        return new WellnessRecord
        {
            WellnessId = model.WellnessId,
            PetId = model.PetId,
            AppointmentId = model.AppointmentId,
            WellnessType = model.WellnessType,
            ProductOrMedication = model.ProductOrMedication,
            DateGiven = model.DateGiven,
            NextDueDate = model.NextDueDate,
            Dose = model.Dose,
            Route = model.Route,
            Remarks = model.Remarks,
            RecordedByUserId = model.RecordedByUserId
        };
    }
}
