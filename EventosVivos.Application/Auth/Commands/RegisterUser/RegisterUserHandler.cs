using EventosVivos.Application.Common.Interfaces;
using EventosVivos.Domain.Entities;
using EventosVivos.Domain.Exceptions;
using MediatR;

namespace EventosVivos.Application.Auth.Commands.RegisterUser;

public sealed class RegisterUserHandler : IRequestHandler<RegisterUserCommand, Guid>
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _uow;

    public RegisterUserHandler(IUserRepository users, IUnitOfWork uow)
    {
        _users = users;
        _uow = uow;
    }

    public async Task<Guid> Handle(RegisterUserCommand cmd, CancellationToken ct)
    {
        var email = cmd.Email.Trim().ToLowerInvariant();

        if (await _users.EmailExistsAsync(email, ct))
            throw new InvalidUserException("EMAIL_IN_USE",
                "A user with that email already exists.");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(cmd.Password);

        var user = User.Create(email, passwordHash, "user");

        await _users.AddAsync(user, ct);
        await _uow.SaveChangesAsync(ct);
        return user.Id;
    }
}
