using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SchiperkeWebApp.Models.ViewModels;

public class PublicAppointmentRequestViewModel : IValidatableObject
{
    [Display(Name = "Has this pet visited the clinic before?")]
    public bool IsExistingPatient { get; set; }

    [Display(Name = "Patient Number")]
    [StringLength(50)]
    public string? PatientNoInput { get; set; }

    [Display(Name = "Pet Name")]
    [StringLength(100)]
    public string PetName { get; set; } = string.Empty;

    [StringLength(50)]
    public string Species { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Breed { get; set; }

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

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (IsExistingPatient)
        {
            if (string.IsNullOrWhiteSpace(PatientNoInput))
            {
                yield return new ValidationResult(
                    "Patient number is required for existing patients.",
                    new[] { nameof(PatientNoInput) });
            }

            yield break;
        }

        if (string.IsNullOrWhiteSpace(PetName))
        {
            yield return new ValidationResult(
                "Pet name is required.",
                new[] { nameof(PetName) });
        }

        if (string.IsNullOrWhiteSpace(Species))
        {
            yield return new ValidationResult(
                "Species is required.",
                new[] { nameof(Species) });
        }

        if (string.IsNullOrWhiteSpace(Sex))
        {
            yield return new ValidationResult(
                "Sex is required.",
                new[] { nameof(Sex) });
        }
    }
}
