using EventosVivos.Application.Auth.Commands.Login;
using EventosVivos.Application.Auth.Commands.RegisterUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventosVivos.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public sealed class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator) => _mediator = mediator;

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterUserCommand cmd, CancellationToken ct)
    {
        var id = await _mediator.Send(cmd, ct);
        return CreatedAtAction(nameof(Register), new { id }, new { id });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginCommand cmd, CancellationToken ct)
    {
        var token = await _mediator.Send(cmd, ct);
        return Ok(new { token });
    }
}
