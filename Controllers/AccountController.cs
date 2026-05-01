using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SchiperkeWebApp.Models.Database;
using SchiperkeWebApp.Models.ViewModels;
using SchiperkeWebApp.Services.Interfaces;

namespace SchiperkeWebApp.Controllers;

[AllowAnonymous]
public class AccountController : Controller
{
    private readonly IUserService _userService;
    private readonly PasswordHasher<User> _passwordHasher = new();

    public AccountController(IUserService userService)
    {
        _userService = userService;
    }

    public IActionResult Login(string? returnUrl = null)
    {
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userService.GetByUsernameAsync(model.Username);
        if (user is null || !PasswordMatches(user, model.Password))
        {
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return View(model);
        }

        var role = NormalizeRole(user.Role);
        if (role is not ("Admin" or "Staff"))
        {
            ModelState.AddModelError(string.Empty, "Only admin or staff accounts can sign in.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, role)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return Redirect(model.ReturnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Book", "Appointments");
    }

    public IActionResult AccessDenied()
    {
        return View();
    }

    private bool PasswordMatches(User user, string password)
    {
        try
        {
            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            if (result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded)
            {
                return true;
            }
        }
        catch (FormatException)
        {
        }

        // Supports existing development records that were stored before password hashing was added.
        return user.PasswordHash == password;
    }

    private static string NormalizeRole(string role)
    {
        if (role.Equals("admin", StringComparison.OrdinalIgnoreCase))
        {
            return "Admin";
        }

        if (role.Equals("staff", StringComparison.OrdinalIgnoreCase))
        {
            return "Staff";
        }

        return role.Trim();
    }
}
