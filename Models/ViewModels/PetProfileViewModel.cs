using SchiperkeWebApp.Models.Database;

namespace SchiperkeWebApp.Models.ViewModels;

public class PetProfileViewModel
{
    public Pet Pet { get; set; } = null!;

    public List<Appointment> Appointments { get; set; } = [];

    public List<ConsultationRecord> ConsultationRecords { get; set; } = [];

    public List<VaccinationRecord> VaccinationRecords { get; set; } = [];

    public List<WellnessRecord> WellnessRecords { get; set; } = [];
}
