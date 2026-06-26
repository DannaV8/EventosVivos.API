using System.Text.RegularExpressions;
using EventosVivos.Domain.Exceptions;

namespace EventosVivos.Domain.Entities;

public sealed class User
{
    private static readonly Regex EmailPattern = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

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
        if (string.IsNullOrWhiteSpace(email) || !EmailPattern.IsMatch(email))
            throw new InvalidUserException("INVALID_EMAIL",
                "A valid email is required.");

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new InvalidUserException("INVALID_PASSWORD_HASH",
                "Password hash is required.");

        if (!ValidRoles.Contains(role))
            throw new InvalidUserException("INVALID_ROLE",
                "Role must be 'admin' or 'user'.");

        return new User
        {
            Id = Guid.NewGuid(),
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            Role = role
        };
    }
}
