using Microsoft.AspNetCore.Mvc;
using SchiperkeWebApp.Models.ViewModels;
using SchiperkeWebApp.Services.Interfaces;

namespace SchiperkeWebApp.Controllers;

public class DashboardController : Controller
{
    private readonly IPetService _petService;
    private readonly IAppointmentService _appointmentService;

    public DashboardController(
        IPetService petService,
        IAppointmentService appointmentService)
    {
        _petService = petService;
        _appointmentService = appointmentService;
    }

    public async Task<IActionResult> Index()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var pets = await _petService.GetAllAsync();
        var appointments = await _appointmentService.GetAllAsync();

        var model = new DashboardViewModel
        {
            ActivePetCount = pets.Count,
            TodayAppointmentCount = appointments.Count(a => a.AppointmentDate == today),
            PendingAppointmentCount = appointments.Count(a => a.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase)),
            ConfirmedAppointmentCount = appointments.Count(a => a.Status.Equals("Confirmed", StringComparison.OrdinalIgnoreCase)),
            CompletedAppointmentCount = appointments.Count(a => a.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase)),
            CancelledAppointmentCount = appointments.Count(a => a.Status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase)),
            NoShowAppointmentCount = appointments.Count(a => a.Status.Equals("No-Show", StringComparison.OrdinalIgnoreCase)),
            TodayAppointments = appointments
                .Where(a => a.AppointmentDate == today)
                .OrderBy(a => a.AppointmentTime)
                .Take(6)
                .ToList()
        };

        return View(model);
    }
}
