using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SchiperkeWebApp.Models.Database;
using SchiperkeWebApp.Models.ViewModels;
using SchiperkeWebApp.Services.Interfaces;

namespace SchiperkeWebApp.Controllers;

public class AppointmentsController : Controller
{
    private readonly IAppointmentService _appointmentService;
    private readonly IPetService _petService;
    private readonly IUserService _userService;

    public AppointmentsController(
        IAppointmentService appointmentService,
        IPetService petService,
        IUserService userService)
    {
        _appointmentService = appointmentService;
        _petService = petService;
        _userService = userService;
    }

    public async Task<IActionResult> Index(
        string? searchTerm,
        int? petId,
        DateOnly? appointmentDate,
        string? status,
        string? serviceType)
    {
        var appointments = await _appointmentService.GetAllAsync();

        if (petId.HasValue)
        {
            appointments = appointments.Where(a => a.PetId == petId.Value).ToList();
        }

        if (appointmentDate.HasValue)
        {
            appointments = appointments.Where(a => a.AppointmentDate == appointmentDate.Value).ToList();
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            appointments = appointments
                .Where(a => a.Status.Equals(status.Trim(), StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        if (!string.IsNullOrWhiteSpace(serviceType))
        {
            appointments = appointments
                .Where(a => a.ServiceType.Equals(serviceType.Trim(), StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();
            appointments = appointments
                .Where(a =>
                    Contains(a.AppointmentCode, term) ||
                    Contains(a.PatientNoInput, term) ||
                    Contains(a.PetName, term) ||
                    Contains(a.Species, term) ||
                    Contains(a.Breed, term) ||
                    Contains(a.Color, term) ||
                    Contains(a.Pet?.PatientNo, term) ||
                    Contains(a.Pet?.PetName, term))
                .ToList();
        }

        ViewData["SearchTerm"] = searchTerm;
        ViewData["AppointmentDate"] = appointmentDate?.ToString("yyyy-MM-dd");
        ViewData["Status"] = status;
        ViewData["ServiceType"] = serviceType;
        ViewData["StatusOptions"] = BuildValueOptions(_appointmentService.GetAllowedStatuses(), status);
        ViewData["ServiceTypeOptions"] = BuildValueOptions(_appointmentService.GetAllowedServiceTypes(), serviceType);

        return View(appointments);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var appointment = await _appointmentService.GetByIdAsync(id.Value);
        return appointment is null ? NotFound() : View(appointment);
    }

    public async Task<IActionResult> Create()
    {
        var model = await BuildAppointmentFormAsync(new AppointmentFormViewModel
        {
            Status = "Pending",
            AppointmentTime = new TimeOnly(8, 0)
        });

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AppointmentFormViewModel model)
    {
        var currentUserId = await ResolveCurrentUserIdAsync();
        if (currentUserId.HasValue && !model.CreatedByUserId.HasValue)
        {
            model.CreatedByUserId = currentUserId.Value;
        }

        if (string.IsNullOrWhiteSpace(model.Status))
        {
            model.Status = "Pending";
        }

        if (!ModelState.IsValid)
        {
            return View(await BuildAppointmentFormAsync(model));
        }

        try
        {
            await _appointmentService.CreateAsync(MapToEntity(model));
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(await BuildAppointmentFormAsync(model));
        }
    }

    [AllowAnonymous]
    public IActionResult Book()
    {
        return View(BuildPublicAppointmentRequestModel(new PublicAppointmentRequestViewModel
        {
            AppointmentTime = new TimeOnly(8, 0)
        }));
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> VerifyPatientNumber(string? patientNo)
    {
        if (string.IsNullOrWhiteSpace(patientNo))
        {
            return BadRequest(new
            {
                found = false,
                message = "Enter a patient number first."
            });
        }

        var pet = await _petService.GetByPatientNoAsync(patientNo.Trim());
        if (pet is null)
        {
            return Ok(new
            {
                found = false,
                message = "No active patient record was found for this number. Check the code or book as a new patient."
            });
        }

        return Ok(new
        {
            found = true,
            message = "Patient number verified. Continue booking with this code."
        });
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Book(PublicAppointmentRequestViewModel model)
    {
        model = BuildPublicAppointmentRequestModel(model);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var appointment = new Appointment
            {
                IsExistingPatient = model.IsExistingPatient,
                PatientNoInput = model.PatientNoInput,
                PetName = model.PetName,
                Species = model.Species,
                Breed = model.Breed,
                Sex = model.Sex,
                Color = model.Color,
                AppointmentDate = model.AppointmentDate,
                AppointmentTime = model.AppointmentTime,
                ServiceType = model.ServiceType,
                ReasonForVisit = model.ReasonForVisit,
                Status = "Pending",
                Remarks = model.Remarks
            };

            await _appointmentService.CreateAsync(appointment);

            TempData["AppointmentCode"] = appointment.AppointmentCode;
            TempData["PatientNoInput"] = appointment.PatientNoInput;
            TempData["PetName"] = appointment.IsExistingPatient ? "Existing patient record" : appointment.PetName;
            TempData["AppointmentDate"] = appointment.AppointmentDate.ToString("yyyy-MM-dd");
            TempData["AppointmentTime"] = appointment.AppointmentTime.ToString("hh:mm tt");
            TempData["ServiceType"] = appointment.ServiceType;

            return RedirectToAction(nameof(Confirmation));
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [AllowAnonymous]
    public IActionResult Confirmation()
    {
        return View();
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> CheckAppointmentStatus(string? appointmentCode, string? patientIdentifier)
    {
        if (string.IsNullOrWhiteSpace(appointmentCode) || string.IsNullOrWhiteSpace(patientIdentifier))
        {
            return BadRequest(new
            {
                found = false,
                message = "Enter both the appointment code and patient number or pet name."
            });
        }

        var appointment = await _appointmentService.GetPublicStatusAsync(appointmentCode, patientIdentifier);
        if (appointment is null)
        {
            return Ok(new
            {
                found = false,
                message = "No matching appointment was found. Check the appointment code and patient identifier."
            });
        }

        return Ok(new
        {
            found = true,
            appointmentCode = GetAppointmentCode(appointment),
            status = appointment.Status,
            schedule = $"{appointment.AppointmentDate:MMM dd, yyyy} {appointment.AppointmentTime:hh\\:mm tt}",
            serviceType = appointment.ServiceType,
            message = BuildPublicStatusMessage(appointment),
            cancellationReason = appointment.CancellationReason,
            cancelledBy = appointment.CancelledBy,
            cancelledAt = appointment.CancelledAt?.ToString("MMM dd, yyyy hh:mm tt"),
            isPast = IsPastAppointment(appointment)
        });
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var appointment = await _appointmentService.GetByIdAsync(id.Value);
        if (appointment is null)
        {
            return NotFound();
        }

        return View(await BuildAppointmentFormAsync(MapToFormModel(appointment)));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AppointmentFormViewModel model)
    {
        if (id != model.AppointmentId)
        {
            return NotFound();
        }

        var currentUserId = await ResolveCurrentUserIdAsync();
        if (currentUserId.HasValue && !model.CreatedByUserId.HasValue)
        {
            model.CreatedByUserId = currentUserId.Value;
        }

        if (!ModelState.IsValid)
        {
            return View(await BuildAppointmentFormAsync(model));
        }

        try
        {
            await _appointmentService.UpdateAsync(MapToEntity(model));
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(await BuildAppointmentFormAsync(model));
        }
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var appointment = await _appointmentService.GetByIdAsync(id.Value);
        return appointment is null ? NotFound() : View(appointment);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _appointmentService.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegisterPatient(int id)
    {
        try
        {
            var pet = await _appointmentService.RegisterPatientAsync(id);
            TempData["SuccessMessage"] = $"Patient registered successfully with patient number {pet.PatientNo}.";
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task<AppointmentFormViewModel> BuildAppointmentFormAsync(AppointmentFormViewModel model)
    {
        var currentUserId = await ResolveCurrentUserIdAsync();
        if (currentUserId.HasValue && !model.CreatedByUserId.HasValue)
        {
            model.CreatedByUserId = currentUserId.Value;
        }

        if (model.CreatedByUserId.HasValue && string.IsNullOrWhiteSpace(model.CreatedByDisplayName))
        {
            var createdByUser = await _userService.GetByIdAsync(model.CreatedByUserId.Value);
            model.CreatedByDisplayName = createdByUser?.FullName
                ?? User.Identity?.Name
                ?? $"User #{model.CreatedByUserId.Value}";
        }

        if (string.IsNullOrWhiteSpace(model.Status))
        {
            model.Status = "Pending";
        }

        model.ServiceTypeOptions = BuildValueOptions(_appointmentService.GetAllowedServiceTypes(), model.ServiceType);
        model.StatusOptions = BuildValueOptions(_appointmentService.GetAllowedStatuses(), model.Status);
        model.SexOptions = BuildValueOptions(["Male", "Female", "Unknown"], model.Sex);
        model.TimeOptions = BuildTimeOptions(model.AppointmentTime);

        return model;
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

    private static IEnumerable<SelectListItem> BuildValueOptions(IEnumerable<string> values, string? selectedValue)
    {
        return values.Select(value => new SelectListItem(value, value, value.Equals(selectedValue, StringComparison.OrdinalIgnoreCase)));
    }

    private PublicAppointmentRequestViewModel BuildPublicAppointmentRequestModel(PublicAppointmentRequestViewModel model)
    {
        ViewData["ServiceTypeOptions"] = BuildValueOptions(_appointmentService.GetAllowedServiceTypes(), model.ServiceType);
        ViewData["SexOptions"] = BuildValueOptions(["Male", "Female", "Unknown"], model.Sex);
        model.TimeOptions = BuildTimeOptions(model.AppointmentTime);
        return model;
    }

    private IEnumerable<SelectListItem> BuildTimeOptions(TimeOnly selectedTime)
    {
        return _appointmentService.GetAllowedAppointmentTimes()
            .Select(time => new SelectListItem(time.ToString("hh:mm tt"), time.ToString("HH\\:mm"), time == selectedTime))
            .ToList();
    }

    private static bool Contains(string? value, string term)
    {
        return !string.IsNullOrWhiteSpace(value)
            && value.Contains(term, StringComparison.OrdinalIgnoreCase);
    }

    private static AppointmentFormViewModel MapToFormModel(Appointment appointment)
    {
        return new AppointmentFormViewModel
        {
            AppointmentId = appointment.AppointmentId,
            PetId = appointment.PetId,
            AppointmentCode = appointment.AppointmentCode,
            IsExistingPatient = appointment.IsExistingPatient,
            PatientNoInput = appointment.PatientNoInput,
            PetName = appointment.PetName ?? appointment.Pet?.PetName ?? string.Empty,
            Species = appointment.Species ?? appointment.Pet?.Species ?? string.Empty,
            Breed = appointment.Breed ?? appointment.Pet?.Breed,
            Sex = appointment.Sex ?? appointment.Pet?.Sex ?? string.Empty,
            Color = appointment.Color ?? appointment.Pet?.Color,
            AppointmentDate = appointment.AppointmentDate,
            AppointmentTime = appointment.AppointmentTime,
            ServiceType = appointment.ServiceType,
            ReasonForVisit = appointment.ReasonForVisit,
            Status = appointment.Status,
            Remarks = appointment.Remarks,
            CancellationReason = appointment.CancellationReason,
            CancelledBy = appointment.CancelledBy,
            CancelledAt = appointment.CancelledAt,
            CreatedByUserId = appointment.CreatedByUserId,
            CreatedByDisplayName = appointment.CreatedByUser?.FullName
        };
    }

    private static Appointment MapToEntity(AppointmentFormViewModel model)
    {
        return new Appointment
        {
            AppointmentId = model.AppointmentId,
            PetId = model.PetId,
            AppointmentCode = model.AppointmentCode,
            IsExistingPatient = model.IsExistingPatient,
            PatientNoInput = model.PatientNoInput,
            PetName = model.PetName,
            Species = model.Species,
            Breed = model.Breed,
            Sex = model.Sex,
            Color = model.Color,
            AppointmentDate = model.AppointmentDate,
            AppointmentTime = model.AppointmentTime,
            ServiceType = model.ServiceType,
            ReasonForVisit = model.ReasonForVisit,
            Status = model.Status,
            Remarks = model.Remarks,
            CancellationReason = model.CancellationReason,
            CancelledBy = model.CancelledBy,
            CancelledAt = model.CancelledAt,
            CreatedByUserId = model.CreatedByUserId
        };
    }

    private static string GetAppointmentCode(Appointment appointment)
    {
        return appointment.AppointmentCode ?? $"APT-{appointment.AppointmentId:0000}";
    }

    private static bool IsPastAppointment(Appointment appointment)
    {
        return appointment.AppointmentDate < DateOnly.FromDateTime(DateTime.Today);
    }

    private static string BuildPublicStatusMessage(Appointment appointment)
    {
        if (appointment.Status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
        {
            return string.IsNullOrWhiteSpace(appointment.CancellationReason)
                ? "This appointment was cancelled."
                : $"This appointment was cancelled. Reason: {appointment.CancellationReason}";
        }

        if (appointment.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
        {
            return "This appointment has already been completed.";
        }

        if (appointment.Status.Equals("No-Show", StringComparison.OrdinalIgnoreCase))
        {
            return "This appointment was marked as no-show.";
        }

        if (IsPastAppointment(appointment))
        {
            return "This appointment date has already passed. Please contact the clinic or book another appointment.";
        }

        if (appointment.Status.Equals("Confirmed", StringComparison.OrdinalIgnoreCase))
        {
            return "Your appointment is confirmed. Please come at the scheduled time.";
        }

        if (appointment.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase))
        {
            return "Your appointment is waiting for clinic confirmation.";
        }

        return "Appointment status found.";
    }
}
