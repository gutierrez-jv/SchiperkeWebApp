using Microsoft.AspNetCore.Mvc;
using SchiperkeWebApp.Repositories.Interfaces;

namespace SchiperkeWebApp.Controllers
{
    public class RepositoryTestController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IPetRepository _petRepository;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IConsultationRecordRepository _consultationRecordRepository;
        private readonly IVaccinationRecordRepository _vaccinationRecordRepository;
        private readonly IWellnessRecordRepository _wellnessRecordRepository;

        public RepositoryTestController(
            IUserRepository userRepository,
            IPetRepository petRepository,
            IAppointmentRepository appointmentRepository,
            IConsultationRecordRepository consultationRecordRepository,
            IVaccinationRecordRepository vaccinationRecordRepository,
            IWellnessRecordRepository wellnessRecordRepository)
        {
            _userRepository = userRepository;
            _petRepository = petRepository;
            _appointmentRepository = appointmentRepository;
            _consultationRecordRepository = consultationRecordRepository;
            _vaccinationRecordRepository = vaccinationRecordRepository;
            _wellnessRecordRepository = wellnessRecordRepository;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userRepository.GetAllAsync();
            var pets = await _petRepository.GetAllAsync();
            var appointments = await _appointmentRepository.GetAllAsync();
            var consultations = await _consultationRecordRepository.GetAllAsync();
            var vaccinations = await _vaccinationRecordRepository.GetAllAsync();
            var wellnessRecords = await _wellnessRecordRepository.GetAllAsync();

            var result = new
            {
                Message = "Repository layer is working.",
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
