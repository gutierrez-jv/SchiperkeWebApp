using Microsoft.AspNetCore.Mvc;
using SchiperkeWebApp.Services.Interfaces;

namespace SchiperkeWebApp.Controllers
{
    public class ServiceTestController : Controller
    {
        private readonly IUserService _userService;
        private readonly IPetService _petService;
        private readonly IAppointmentService _appointmentService;
        private readonly IConsultationRecordService _consultationRecordService;
        private readonly IVaccinationRecordService _vaccinationRecordService;
        private readonly IWellnessRecordService _wellnessRecordService;

        public ServiceTestController(
            IUserService userService,
            IPetService petService,
            IAppointmentService appointmentService,
            IConsultationRecordService consultationRecordService,
            IVaccinationRecordService vaccinationRecordService,
            IWellnessRecordService wellnessRecordService)
        {
            _userService = userService;
            _petService = petService;
            _appointmentService = appointmentService;
            _consultationRecordService = consultationRecordService;
            _vaccinationRecordService = vaccinationRecordService;
            _wellnessRecordService = wellnessRecordService;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userService.GetAllAsync();
            var pets = await _petService.GetAllAsync();
            var appointments = await _appointmentService.GetAllAsync();
            var consultations = await _consultationRecordService.GetAllAsync();
            var vaccinations = await _vaccinationRecordService.GetAllAsync();
            var wellnessRecords = await _wellnessRecordService.GetAllAsync();

            var result = new
            {
                Message = "Service layer is working.",
                UsersCount = users.Count,
                PetsCount = pets.Count,
                AppointmentsCount = appointments.Count,
                ConsultationsCount = consultations.Count,
                VaccinationsCount = vaccinations.Count,
                WellnessRecordsCount = wellnessRecords.Count
            };

            return Json(result);
        }
    }
}
