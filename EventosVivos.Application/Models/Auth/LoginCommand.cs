using MediatR;

namespace EventosVivos.Application.Models.Auth;

public sealed record LoginCommand(
    string Email,
    string Password
) : IRequest<string>;
