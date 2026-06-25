using EventosVivos.Domain.Exceptions;
using System.Data;

namespace EventosVivos.Domain.Entities;

public sealed class User
{
    // Valid system roles. The "AdminOnly" policy depends on "admin".
    public static readonly string[] ValidRoles = ["admin", "user"];

    public Guid Id { get; private set; }
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public string Role { get; private set; } = null!;

    // Constructor for EF Core.
    private User() { }

    public static User Create(string email, string passwordHash, string role)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new InvalidUserException("INVALID_EMAIL",
                "El email es obligatorio.");

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new InvalidUserException("INVALID_PASSWORD_HASH",
                "El hash de la contraseña es obligatorio.");

        if (!ValidRoles.Contains(role))
            throw new InvalidUserException("INVALID_ROLE",
                "El rol debe ser 'admin' o 'user'.");

        return new User
        {
            Id = Guid.NewGuid(),
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            Role = role
        };
    }
}
