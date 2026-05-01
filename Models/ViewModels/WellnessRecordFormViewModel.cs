using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SchiperkeWebApp.Models.ViewModels;

public class WellnessRecordFormViewModel
{
    public int WellnessId { get; set; }

    [Required]
    [Display(Name = "Pet")]
    public int PetId { get; set; }

    [Display(Name = "Appointment")]
    public int? AppointmentId { get; set; }

    [Required]
    [Display(Name = "Wellness Type")]
    public string WellnessType { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Product or Medication")]
    public string ProductOrMedication { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Date Given")]
    [DataType(DataType.Date)]
    public DateOnly DateGiven { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Display(Name = "Next Due Date")]
    [DataType(DataType.Date)]
    public DateOnly? NextDueDate { get; set; }

    public string? Dose { get; set; }

    public string? Route { get; set; }

    public string? Remarks { get; set; }

    [Required]
    [Display(Name = "Recorded By")]
    public int RecordedByUserId { get; set; }

    public IEnumerable<SelectListItem> PetOptions { get; set; } = [];

    public IEnumerable<SelectListItem> UserOptions { get; set; } = [];

    public IEnumerable<SelectListItem> WellnessTypeOptions { get; set; } = [];
}
