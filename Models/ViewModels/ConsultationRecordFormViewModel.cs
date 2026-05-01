using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SchiperkeWebApp.Models.ViewModels;

public class ConsultationRecordFormViewModel
{
    public int ConsultationId { get; set; }

    [Required]
    [Display(Name = "Pet")]
    public int PetId { get; set; }

    [Display(Name = "Appointment")]
    public int? AppointmentId { get; set; }

    [Required]
    [Display(Name = "Consultation Date")]
    [DataType(DataType.DateTime)]
    public DateTime ConsultationDate { get; set; } = DateTime.Now;

    [Display(Name = "Chief Complaint")]
    public string? ChiefComplaint { get; set; }

    public string? History { get; set; }

    public string? Vitals { get; set; }

    [Display(Name = "Physical Examination")]
    public string? PhysicalExamination { get; set; }

    [Display(Name = "Lab Exam")]
    public string? LabExam { get; set; }

    public string? Assessment { get; set; }

    public string? Treatment { get; set; }

    public string? Notes { get; set; }

    [Required]
    [Display(Name = "Recorded By")]
    public int RecordedByUserId { get; set; }

    public IEnumerable<SelectListItem> PetOptions { get; set; } = [];

    public IEnumerable<SelectListItem> UserOptions { get; set; } = [];
}
