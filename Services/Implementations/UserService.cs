using Microsoft.AspNetCore.Identity;
using SchiperkeWebApp.Models.Database;
using SchiperkeWebApp.Repositories.Interfaces;
using SchiperkeWebApp.Services.Interfaces;

namespace SchiperkeWebApp.Services.Implementations;

public class UserService : IUserService
{
    private static readonly string[] AllowedRoles =
    [
        "Admin",
        "Staff",
        "Veterinarian"
    ];

    private readonly IUserRepository _userRepository;
    private readonly PasswordHasher<User> _passwordHasher = new();

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<List<User>> GetAllAsync()
    {
        return await _userRepository.GetAllAsync();
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _userRepository.GetByIdAsync(id);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentException("Username is required.");
        }

        return await _userRepository.GetByUsernameAsync(username.Trim());
    }

    public async Task CreateAsync(User user)
    {
        ValidateUser(user, requirePassword: true);

        user.Username = user.Username.Trim();
        user.FullName = user.FullName.Trim();
        user.Role = NormalizeRole(user.Role);
        user.PasswordHash = _passwordHasher.HashPassword(user, user.PasswordHash.Trim());
        user.IsActive = true;

        var existingUser = await _userRepository.GetByUsernameAsync(user.Username);
        if (existingUser is not null)
        {
            throw new InvalidOperationException("Username already exists.");
        }

        await _userRepository.AddAsync(user);
        await _userRepository.SaveAsync();
    }

    public async Task UpdateAsync(User user)
    {
        ValidateUser(user, requirePassword: false);

        var existingUser = await _userRepository.GetByIdAsync(user.UserId);
        if (existingUser is null)
        {
            throw new InvalidOperationException("Active user record was not found.");
        }

        user.Username = user.Username.Trim();
        user.FullName = user.FullName.Trim();
        user.Role = NormalizeRole(user.Role);
        user.PasswordHash = string.IsNullOrWhiteSpace(user.PasswordHash)
            ? existingUser.PasswordHash
            : _passwordHasher.HashPassword(user, user.PasswordHash.Trim());
        user.IsActive = existingUser.IsActive;
        user.CreatedAt = existingUser.CreatedAt;

        var duplicateUser = await _userRepository.GetByUsernameAsync(user.Username);
        if (duplicateUser is not null && duplicateUser.UserId != user.UserId)
        {
            throw new InvalidOperationException("Username already exists.");
        }

        _userRepository.Update(user);
        await _userRepository.SaveAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user is null)
        {
            return;
        }

        // Soft delete only: do not physically remove records from the database.
        _userRepository.Delete(user);
        await _userRepository.SaveAsync();
    }

    private static void ValidateUser(User user, bool requirePassword)
    {
        if (string.IsNullOrWhiteSpace(user.Username))
        {
            throw new ArgumentException("Username is required.");
        }

        if (string.IsNullOrWhiteSpace(user.FullName))
        {
            throw new ArgumentException("Full name is required.");
        }

        if (string.IsNullOrWhiteSpace(user.Role))
        {
            throw new ArgumentException("Role is required.");
        }

        if (requirePassword && string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            throw new ArgumentException("Password is required.");
        }

        _ = NormalizeRole(user.Role);
    }

    private static string NormalizeRole(string role)
    {
        var value = role.Trim();
        var normalizedRole = AllowedRoles.FirstOrDefault(allowedRole => allowedRole.Equals(value, StringComparison.OrdinalIgnoreCase));
        if (normalizedRole is null)
        {
            throw new ArgumentException("Role must be Admin, Staff, or Veterinarian.");
        }

        return normalizedRole;
    }
}
