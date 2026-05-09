using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SchiperkeWebApp.Models.ViewModels;

public class VaccinationRecordFormViewModel
{
    public int VaccinationId { get; set; }

    [Required]
    [Display(Name = "Pet")]
    public int PetId { get; set; }

    [Display(Name = "Appointment")]
    public int? AppointmentId { get; set; }

    [Required]
    [Display(Name = "Vaccine Name")]
    public string VaccineName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Date Given")]
    [DataType(DataType.Date)]
    public DateOnly DateGiven { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Display(Name = "Next Due Date")]
    [DataType(DataType.Date)]
    public DateOnly? NextDueDate { get; set; }

    public string? Dose { get; set; }

    public string? Route { get; set; }

    public string? Manufacturer { get; set; }

    [Display(Name = "Lot Number")]
    public string? LotNumber { get; set; }

    public string? Remarks { get; set; }

    [Required]
    [Display(Name = "Recorded By")]
    public int RecordedByUserId { get; set; }

    public IEnumerable<SelectListItem> PetOptions { get; set; } = [];

    public IEnumerable<SelectListItem> AppointmentOptions { get; set; } = [];

    public IEnumerable<SelectListItem> UserOptions { get; set; } = [];
}
