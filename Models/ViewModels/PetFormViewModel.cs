using System.ComponentModel.DataAnnotations;

namespace SchiperkeWebApp.Models.ViewModels;

public class PetFormViewModel
{
    public int PetId { get; set; }

    [Display(Name = "Patient No.")]
    public string? PatientNo { get; set; }

    [Required]
    [Display(Name = "Pet Name")]
    public string PetName { get; set; } = string.Empty;

    [Required]
    public string Species { get; set; } = string.Empty;

    public string? Breed { get; set; }

    [Required]
    public string Sex { get; set; } = string.Empty;

    [Display(Name = "Birth Date")]
    [DataType(DataType.Date)]
    public DateOnly? BirthDate { get; set; }

    [Display(Name = "Age (Years)")]
    public int? AgeYears { get; set; }

    public string? Color { get; set; }

    [Range(typeof(decimal), "0.01", "999.99")]
    public decimal? Weight { get; set; }

    public string? Notes { get; set; }
}
