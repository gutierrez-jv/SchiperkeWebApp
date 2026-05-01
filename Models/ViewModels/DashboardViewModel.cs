using SchiperkeWebApp.Models.Database;

namespace SchiperkeWebApp.Models.ViewModels;

public class DashboardViewModel
{
    public int ActivePetCount { get; set; }

    public int TodayAppointmentCount { get; set; }

    public int PendingAppointmentCount { get; set; }

    public int ConfirmedAppointmentCount { get; set; }

    public int CompletedAppointmentCount { get; set; }

    public int CancelledAppointmentCount { get; set; }

    public int NoShowAppointmentCount { get; set; }

    public List<Appointment> TodayAppointments { get; set; } = [];
}
