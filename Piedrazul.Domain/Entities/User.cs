using Piedrazul.Domain.Enums;

namespace Piedrazul.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Username { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public string FullName { get; private set; } = null!;
    public string? Email { get; private set; }
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private User() { }

    public static User Create(
        string username,
        string passwordHash,
        string fullName,
        UserRole role,
        string? email = null)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("El nombre de usuario es obligatorio.");
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("La contraseña no puede estar vacía.");

        return new User
        {
            Id = Guid.NewGuid(),
            Username = username.Trim().ToLower(),
            PasswordHash = passwordHash,
            FullName = fullName.Trim(),
            Email = email?.Trim(),
            Role = role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
    public void UpdatePassword(string newHash) => PasswordHash = newHash;
}