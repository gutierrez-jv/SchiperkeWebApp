using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SchiperkeWebApp.Models.Database;
using SchiperkeWebApp.Models.ViewModels;
using SchiperkeWebApp.Services.Interfaces;

namespace SchiperkeWebApp.Controllers;

public class ConsultationRecordsController : Controller
{
    private readonly IConsultationRecordService _consultationRecordService;
    private readonly IAppointmentService _appointmentService;
    private readonly IPetService _petService;
    private readonly IUserService _userService;

    public ConsultationRecordsController(
        IConsultationRecordService consultationRecordService,
        IAppointmentService appointmentService,
        IPetService petService,
        IUserService userService)
    {
        _consultationRecordService = consultationRecordService;
        _appointmentService = appointmentService;
        _petService = petService;
        _userService = userService;
    }

    public async Task<IActionResult> Index(int? petId, DateTime? startDate, DateTime? endDate)
    {
        var records = await _consultationRecordService.GetAllAsync();

        if (startDate.HasValue && endDate.HasValue)
        {
            if (startDate.Value > endDate.Value)
            {
                ModelState.AddModelError(string.Empty, "Start date cannot be later than end date.");
            }
            else
            {
                records = await _consultationRecordService.GetByDateRangeAsync(startDate.Value, endDate.Value);
            }
        }

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
            return RedirectToAction(nameof(Index));
        }

        var record = await _consultationRecordService.GetByIdAsync(id.Value);
        return record is null
            ? RedirectToAction(nameof(Index))
            : View(record);
    }

    public async Task<IActionResult> Create(int? appointmentId)
    {
        return View(await BuildFormAsync(new ConsultationRecordFormViewModel
        {
            AppointmentId = appointmentId
        }));
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
            return RedirectToAction(nameof(Index));
        }

        var record = await _consultationRecordService.GetByIdAsync(id.Value);
        if (record is null)
        {
            return RedirectToAction(nameof(Index));
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
            return RedirectToAction(nameof(Index));
        }

        var record = await _consultationRecordService.GetByIdAsync(id.Value);
        return record is null
            ? RedirectToAction(nameof(Index))
            : View(record);
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
        await ApplyAppointmentContextAsync(model);

        var currentUserId = await ResolveCurrentUserIdAsync();
        if (currentUserId.HasValue && model.RecordedByUserId == 0)
        {
            model.RecordedByUserId = currentUserId.Value;
        }

        var pets = await _petService.GetAllAsync();
        var users = await _userService.GetAllAsync();
        var appointments = await GetCompletedAppointmentOptionsAsync("Consultation", model.AppointmentId);

        model.PetOptions = pets.Select(p => new SelectListItem($"{p.PatientNo} - {p.PetName}", p.PetId.ToString(), p.PetId == model.PetId));
        model.AppointmentOptions = appointments.Select(a => new SelectListItem(BuildAppointmentLabel(a), a.AppointmentId.ToString(), a.AppointmentId == model.AppointmentId));
        model.UserOptions = users.Select(u => new SelectListItem($"{u.FullName} ({u.Role})", u.UserId.ToString(), u.UserId == model.RecordedByUserId));

        return model;
    }

    private async Task ApplyAppointmentContextAsync(ConsultationRecordFormViewModel model)
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

        if (model.ConsultationId == 0)
        {
            model.ConsultationDate = appointment.AppointmentDate.ToDateTime(appointment.AppointmentTime);
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
