using System.ComponentModel.DataAnnotations;

namespace SchiperkeWebApp.Models.ViewModels;

public class UserFormViewModel
{
    public int UserId { get; set; }

    [Required]
    [Display(Name = "Username")]
    public string Username { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string? PasswordHash { get; set; }

    [Required]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = string.Empty;
}
