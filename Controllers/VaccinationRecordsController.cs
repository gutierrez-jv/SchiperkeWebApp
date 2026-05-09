using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SchiperkeWebApp.Models.Database;
using SchiperkeWebApp.Models.ViewModels;
using SchiperkeWebApp.Services.Interfaces;

namespace SchiperkeWebApp.Controllers;

public class VaccinationRecordsController : Controller
{
    private readonly IVaccinationRecordService _vaccinationRecordService;
    private readonly IAppointmentService _appointmentService;
    private readonly IPetService _petService;
    private readonly IUserService _userService;

    public VaccinationRecordsController(
        IVaccinationRecordService vaccinationRecordService,
        IAppointmentService appointmentService,
        IPetService petService,
        IUserService userService)
    {
        _vaccinationRecordService = vaccinationRecordService;
        _appointmentService = appointmentService;
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

    public async Task<IActionResult> Create(int? appointmentId)
    {
        return View(await BuildFormAsync(new VaccinationRecordFormViewModel
        {
            AppointmentId = appointmentId
        }));
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
        await ApplyAppointmentContextAsync(model);

        var currentUserId = await ResolveCurrentUserIdAsync();
        if (currentUserId.HasValue && model.RecordedByUserId == 0)
        {
            model.RecordedByUserId = currentUserId.Value;
        }

        var pets = await _petService.GetAllAsync();
        var users = await _userService.GetAllAsync();
        var appointments = await GetCompletedAppointmentOptionsAsync("Vaccination", model.AppointmentId);

        model.PetOptions = pets.Select(p => new SelectListItem($"{p.PatientNo} - {p.PetName}", p.PetId.ToString(), p.PetId == model.PetId));
        model.AppointmentOptions = appointments.Select(a => new SelectListItem(BuildAppointmentLabel(a), a.AppointmentId.ToString(), a.AppointmentId == model.AppointmentId));
        model.UserOptions = users.Select(u => new SelectListItem($"{u.FullName} ({u.Role})", u.UserId.ToString(), u.UserId == model.RecordedByUserId));

        return model;
    }

    private async Task ApplyAppointmentContextAsync(VaccinationRecordFormViewModel model)
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

        if (model.VaccinationId == 0)
        {
            model.DateGiven = appointment.AppointmentDate;
        }
    }

    private async Task<IEnumerable<Appointment>> GetCompletedAppointmentOptionsAsync(string serviceType, int? selectedAppointmentId)
    {
        var appointments = await _appointmentService.GetAllAsync();
        return appointments
            .Where(a =>
                a.PetId.HasValue &&
                a.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase) &&
                (a.ServiceType.Equals(serviceType, StringComparison.OrdinalIgnoreCase) ||
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

    private static string BuildAppointmentLabel(Appointment appointment)
    {
        var appointmentCode = appointment.AppointmentCode ?? $"APT-{appointment.AppointmentId:0000}";
        var patientNo = appointment.PatientNoInput ?? appointment.Pet?.PatientNo ?? "No patient no";
        var petName = appointment.PetName ?? appointment.Pet?.PetName ?? "Unnamed pet";
        return $"{appointmentCode} - {patientNo} {petName} - {appointment.AppointmentDate:yyyy-MM-dd} {appointment.AppointmentTime:hh\\:mm tt}";
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
