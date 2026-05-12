using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchiperkeWebApp.Models.Database;
using SchiperkeWebApp.Models.ViewModels;
using SchiperkeWebApp.Services.Interfaces;

namespace SchiperkeWebApp.Controllers;

[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _userService.GetAllAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
        {
            return RedirectToAction(nameof(Index));
        }

        var user = await _userService.GetByIdAsync(id.Value);
        return user is null
            ? RedirectToAction(nameof(Index))
            : View(user);
    }

    public IActionResult Create()
    {
        return View(new UserFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await _userService.CreateAsync(MapToEntity(model));
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
        {
            return RedirectToAction(nameof(Index));
        }

        var user = await _userService.GetByIdAsync(id.Value);
        if (user is null)
        {
            return RedirectToAction(nameof(Index));
        }

        return View(MapToFormModel(user));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UserFormViewModel model)
    {
        if (id != model.UserId)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await _userService.UpdateAsync(MapToEntity(model));
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
        {
            return RedirectToAction(nameof(Index));
        }

        var user = await _userService.GetByIdAsync(id.Value);
        return user is null
            ? RedirectToAction(nameof(Index))
            : View(user);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _userService.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    private static UserFormViewModel MapToFormModel(User user)
    {
        return new UserFormViewModel
        {
            UserId = user.UserId,
            Username = user.Username,
            PasswordHash = null,
            FullName = user.FullName,
            Role = user.Role
        };
    }

    private static User MapToEntity(UserFormViewModel model)
    {
        return new User
        {
            UserId = model.UserId,
            Username = model.Username,
            PasswordHash = model.PasswordHash ?? string.Empty,
            FullName = model.FullName,
            Role = model.Role
        };
    }
}
