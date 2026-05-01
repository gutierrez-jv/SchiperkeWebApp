using System.ComponentModel.DataAnnotations;

namespace SchiperkeWebApp.Models.ViewModels;

public class UserFormViewModel
{
    public int UserId { get; set; }

    [Required]
    [Display(Name = "Username")]
    public string Username { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Password Hash")]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = string.Empty;
}
