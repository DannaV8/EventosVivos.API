using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EventosVivos.Application.Reservations.Commands.CancelReservation;
using EventosVivos.Application.Reservations.Commands.ConfirmPayment;
using EventosVivos.Application.Reservations.Commands.CreateReservation;
using EventosVivos.Application.Reservations.Queries.ListReservations;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventosVivos.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class ReservationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReservationsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var userId = GetUserId();
        var isAdmin = User.HasClaim("rol", "admin");
        var result = await _mediator.Send(new ListReservationsQuery(userId, isAdmin), ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateReservationCommand cmd, CancellationToken ct)
    {
        var userId = GetUserId();
        var cmdWithUser = cmd with { UserId = userId };
        var id = await _mediator.Send(cmdWithUser, ct);
        return CreatedAtAction(nameof(Create), new { id }, new { id });
    }

    [HttpPut("{id:guid}/confirm")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Confirm(Guid id, CancellationToken ct)
    {
        var code = await _mediator.Send(new ConfirmPaymentCommand(id), ct);
        return Ok(new { reservationCode = code });
    }

    [HttpPut("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        var isAdmin = User.HasClaim("rol", "admin");
        await _mediator.Send(new CancelReservationCommand(id, userId, isAdmin), ct);
        return Ok();
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? throw new InvalidOperationException("Token sin claim 'sub'."));
}
