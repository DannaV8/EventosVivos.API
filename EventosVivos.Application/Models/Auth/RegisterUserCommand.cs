using MediatR;

namespace EventosVivos.Application.Models.Auth;

public sealed record RegisterUserCommand(
    string Email,
    string Password
) : IRequest<Guid>;
