using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SchiperkeWebApp.Models.Database;
using SchiperkeWebApp.Models.ViewModels;
using SchiperkeWebApp.Services.Interfaces;

namespace SchiperkeWebApp.Controllers;

public class WellnessRecordsController : Controller
{
    private readonly IWellnessRecordService _wellnessRecordService;
    private readonly IAppointmentService _appointmentService;
    private readonly IPetService _petService;
    private readonly IUserService _userService;

    public WellnessRecordsController(
        IWellnessRecordService wellnessRecordService,
        IAppointmentService appointmentService,
        IPetService petService,
        IUserService userService)
    {
        _wellnessRecordService = wellnessRecordService;
        _appointmentService = appointmentService;
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
            return RedirectToAction(nameof(Index));
        }

        var record = await _wellnessRecordService.GetByIdAsync(id.Value);
        return record is null
            ? RedirectToAction(nameof(Index))
            : View(record);
    }

    public async Task<IActionResult> Create(int? appointmentId)
    {
        return View(await BuildFormAsync(new WellnessRecordFormViewModel
        {
            AppointmentId = appointmentId
        }));
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
            return RedirectToAction(nameof(Index));
        }

        var record = await _wellnessRecordService.GetByIdAsync(id.Value);
        if (record is null)
        {
            return RedirectToAction(nameof(Index));
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
            return RedirectToAction(nameof(Index));
        }

        var record = await _wellnessRecordService.GetByIdAsync(id.Value);
        return record is null
            ? RedirectToAction(nameof(Index))
            : View(record);
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
        await ApplyAppointmentContextAsync(model);

        var currentUserId = await ResolveCurrentUserIdAsync();
        if (currentUserId.HasValue && model.RecordedByUserId == 0)
        {
            model.RecordedByUserId = currentUserId.Value;
        }

        var pets = await _petService.GetAllAsync();
        var users = await _userService.GetAllAsync();
        var appointments = await GetCompletedAppointmentOptionsAsync(model.AppointmentId);

        model.PetOptions = pets.Select(p => new SelectListItem($"{p.PatientNo} - {p.PetName}", p.PetId.ToString(), p.PetId == model.PetId));
        model.AppointmentOptions = appointments.Select(a => new SelectListItem(BuildAppointmentLabel(a), a.AppointmentId.ToString(), a.AppointmentId == model.AppointmentId));
        model.UserOptions = users.Select(u => new SelectListItem($"{u.FullName} ({u.Role})", u.UserId.ToString(), u.UserId == model.RecordedByUserId));
        model.WellnessTypeOptions = BuildValueOptions(_wellnessRecordService.GetAllowedWellnessTypes(), model.WellnessType);

        return model;
    }

    private async Task ApplyAppointmentContextAsync(WellnessRecordFormViewModel model)
    {
        if (!model.AppointmentId.HasValue)
        {
            return;
        }

        var appointment = await _appointmentService.GetByIdAsync(model.AppointmentId.Value);
        if (appointment?.PetId is null)
        {
            return;
        }

        if (model.PetId == 0)
        {
            model.PetId = appointment.PetId.Value;
        }

        if (model.WellnessId == 0)
        {
            model.DateGiven = appointment.AppointmentDate;
            if (string.IsNullOrWhiteSpace(model.WellnessType) && IsWellnessServiceType(appointment.ServiceType))
            {
                model.WellnessType = MapAppointmentServiceToWellnessType(appointment.ServiceType);
            }
        }
    }

    private async Task<IEnumerable<Appointment>> GetCompletedAppointmentOptionsAsync(int? selectedAppointmentId)
    {
        var appointments = await _appointmentService.GetAllAsync();
        return appointments
            .Where(a =>
                a.PetId.HasValue &&
                a.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase) &&
                (IsWellnessServiceType(a.ServiceType) ||
                 a.AppointmentId == selectedAppointmentId))
            .OrderByDescending(a => a.AppointmentDate)
            .ThenBy(a => a.AppointmentTime)
            .ToList();
    }

    private async Task<int?> ResolveCurrentUserIdAsync()
    {
        var claimValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(claimValue, out var userId))
        {
            return null;
        }

        var user = await _userService.GetByIdAsync(userId);
        return user is null ? null : userId;
    }

    private static bool IsWellnessServiceType(string serviceType)
    {
        return serviceType.Equals("Deworming", StringComparison.OrdinalIgnoreCase)
            || serviceType.Equals("Internal Antiparasitic", StringComparison.OrdinalIgnoreCase)
            || serviceType.Equals("External Antiparasitic", StringComparison.OrdinalIgnoreCase)
            || serviceType.Equals("Wellness", StringComparison.OrdinalIgnoreCase)
            || serviceType.Equals("General Wellness", StringComparison.OrdinalIgnoreCase);
    }

    private static string MapAppointmentServiceToWellnessType(string serviceType)
    {
        return serviceType.Equals("General Wellness", StringComparison.OrdinalIgnoreCase)
            ? "Wellness"
            : serviceType;
    }

    private static string BuildAppointmentLabel(Appointment appointment)
    {
        var appointmentCode = appointment.AppointmentCode ?? $"APT-{appointment.AppointmentId:0000}";
        var patientNo = appointment.PatientNoInput ?? appointment.Pet?.PatientNo ?? "No patient no";
        var petName = appointment.PetName ?? appointment.Pet?.PetName ?? "Unnamed pet";
        return $"{appointmentCode} - {patientNo} {petName} - {appointment.AppointmentDate:yyyy-MM-dd} {appointment.AppointmentTime:hh\\:mm tt}";
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
