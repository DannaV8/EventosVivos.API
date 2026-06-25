using EventosVivos.Application.Common.Interfaces;
using EventosVivos.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EventosVivos.Application.Auth.Commands.Login;

public sealed class LoginHandler : IRequestHandler<LoginCommand, string>
{
    private readonly IUserRepository _users;
    private readonly ITokenGenerator _tokenGenerator;
    private readonly ILogger<LoginHandler> _logger;

    public LoginHandler(
        IUserRepository users, ITokenGenerator tokenGenerator,
        ILogger<LoginHandler> logger)
    {
        _users = users;
        _tokenGenerator = tokenGenerator;
        _logger = logger;
    }

    public async Task<string> Handle(LoginCommand cmd, CancellationToken ct)
    {
        var email = cmd.Email.Trim().ToLowerInvariant();

        var user = await _users.GetByEmailAsync(email, ct);

        if (user is null || !BCrypt.Net.BCrypt.Verify(cmd.Password, user.PasswordHash))
        {
            _logger.LogWarning("Failed login attempt for email={Email}", email);
            throw new InvalidCredentialsException();
        }

        _logger.LogInformation("Login successful: user={UserId} role={Role}",
            user.Id, user.Role);

        return _tokenGenerator.GenerateToken(
            user.Id.ToString(), user.Email, user.Role);
    }
}
