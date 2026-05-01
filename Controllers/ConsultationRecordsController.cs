using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SchiperkeWebApp.Models.Database;
using SchiperkeWebApp.Models.ViewModels;
using SchiperkeWebApp.Services.Interfaces;

namespace SchiperkeWebApp.Controllers;

public class ConsultationRecordsController : Controller
{
    private readonly IConsultationRecordService _consultationRecordService;
    private readonly IPetService _petService;
    private readonly IUserService _userService;

    public ConsultationRecordsController(
        IConsultationRecordService consultationRecordService,
        IPetService petService,
        IUserService userService)
    {
        _consultationRecordService = consultationRecordService;
        _petService = petService;
        _userService = userService;
    }

    public async Task<IActionResult> Index(int? petId, DateTime? startDate, DateTime? endDate)
    {
        var records = startDate.HasValue && endDate.HasValue
            ? await _consultationRecordService.GetByDateRangeAsync(startDate.Value, endDate.Value)
            : await _consultationRecordService.GetAllAsync();

        if (petId.HasValue)
        {
            records = records.Where(r => r.PetId == petId.Value).ToList();
        }

        ViewData["PetId"] = petId?.ToString();
        ViewData["StartDate"] = startDate?.ToString("yyyy-MM-dd");
        ViewData["EndDate"] = endDate?.ToString("yyyy-MM-dd");
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

        var record = await _consultationRecordService.GetByIdAsync(id.Value);
        return record is null ? NotFound() : View(record);
    }

    public async Task<IActionResult> Create()
    {
        return View(await BuildFormAsync(new ConsultationRecordFormViewModel()));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ConsultationRecordFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(await BuildFormAsync(model));
        }

        try
        {
            await _consultationRecordService.CreateAsync(MapToEntity(model));
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

        var record = await _consultationRecordService.GetByIdAsync(id.Value);
        if (record is null)
        {
            return NotFound();
        }

        return View(await BuildFormAsync(MapToFormModel(record)));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ConsultationRecordFormViewModel model)
    {
        if (id != model.ConsultationId)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(await BuildFormAsync(model));
        }

        try
        {
            await _consultationRecordService.UpdateAsync(MapToEntity(model));
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

        var record = await _consultationRecordService.GetByIdAsync(id.Value);
        return record is null ? NotFound() : View(record);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _consultationRecordService.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    private async Task<ConsultationRecordFormViewModel> BuildFormAsync(ConsultationRecordFormViewModel model)
    {
        var pets = await _petService.GetAllAsync();
        var users = await _userService.GetAllAsync();

        model.PetOptions = pets.Select(p => new SelectListItem($"{p.PatientNo} - {p.PetName}", p.PetId.ToString(), p.PetId == model.PetId));
        model.UserOptions = users.Select(u => new SelectListItem($"{u.FullName} ({u.Role})", u.UserId.ToString(), u.UserId == model.RecordedByUserId));

        return model;
    }

    private static ConsultationRecordFormViewModel MapToFormModel(ConsultationRecord record)
    {
        return new ConsultationRecordFormViewModel
        {
            ConsultationId = record.ConsultationId,
            PetId = record.PetId,
            AppointmentId = record.AppointmentId,
            ConsultationDate = record.ConsultationDate,
            ChiefComplaint = record.ChiefComplaint,
            History = record.History,
            Vitals = record.Vitals,
            PhysicalExamination = record.PhysicalExamination,
            LabExam = record.LabExam,
            Assessment = record.Assessment,
            Treatment = record.Treatment,
            Notes = record.Notes,
            RecordedByUserId = record.RecordedByUserId
        };
    }

    private static ConsultationRecord MapToEntity(ConsultationRecordFormViewModel model)
    {
        return new ConsultationRecord
        {
            ConsultationId = model.ConsultationId,
            PetId = model.PetId,
            AppointmentId = model.AppointmentId,
            ConsultationDate = model.ConsultationDate,
            ChiefComplaint = model.ChiefComplaint,
            History = model.History,
            Vitals = model.Vitals,
            PhysicalExamination = model.PhysicalExamination,
            LabExam = model.LabExam,
            Assessment = model.Assessment,
            Treatment = model.Treatment,
            Notes = model.Notes,
            RecordedByUserId = model.RecordedByUserId
        };
    }
}
