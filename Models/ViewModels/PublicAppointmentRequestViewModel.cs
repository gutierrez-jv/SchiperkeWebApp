using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SchiperkeWebApp.Models.ViewModels;

public class PublicAppointmentRequestViewModel
{
    [Display(Name = "Has this pet visited the clinic before?")]
    public bool IsExistingPatient { get; set; }

    [Display(Name = "Patient Number")]
    [StringLength(50)]
    public string? PatientNoInput { get; set; }

    [Required]
    [Display(Name = "Pet Name")]
    [StringLength(100)]
    public string PetName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Species { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Breed { get; set; }

    [Required]
    [StringLength(20)]
    public string Sex { get; set; } = string.Empty;

    [StringLength(50)]
    public string? Color { get; set; }

    [Required]
    [Display(Name = "Appointment Date")]
    [DataType(DataType.Date)]
    public DateOnly AppointmentDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Required]
    [Display(Name = "Appointment Time")]
    public TimeOnly AppointmentTime { get; set; } = TimeOnly.FromDateTime(DateTime.Now);

    [Required]
    [Display(Name = "Service Type")]
    public string ServiceType { get; set; } = string.Empty;

    [Display(Name = "Reason for Visit")]
    public string? ReasonForVisit { get; set; }

    [Display(Name = "Remarks / Further Details")]
    public string? Remarks { get; set; }

    public IEnumerable<SelectListItem> TimeOptions { get; set; } = [];
}
