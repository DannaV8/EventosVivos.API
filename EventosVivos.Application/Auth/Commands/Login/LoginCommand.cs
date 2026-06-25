using MediatR;

namespace EventosVivos.Application.Auth.Commands.Login;

public sealed record LoginCommand(
    string Email,
    string Password
) : IRequest<string>;
