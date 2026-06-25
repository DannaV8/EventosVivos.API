using EventosVivos.Application.Events.Commands.CreateEvent;
using EventosVivos.Application.Events.Queries.ListEvents;
using EventosVivos.Application.Reports.Queries.EventOccupancy;
using EventosVivos.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventosVivos.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class EventsController : ControllerBase
{
    private readonly IMediator _mediator;

    public EventsController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEventCommand cmd, CancellationToken ct)
    {
        var id = await _mediator.Send(cmd, ct);
        return CreatedAtAction(nameof(Create), new { id }, new { id });
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> List(
        [FromQuery] EventType? type,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] int? venueId,
        [FromQuery] EventStatus? status,
        [FromQuery] string? title,
        CancellationToken ct)
    {
        var query = new ListEventsQuery(type, startDate, endDate, venueId, status, title);
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}/report")]
    public async Task<IActionResult> Report(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new EventOccupancyQuery(id), ct);
        return Ok(result);
    }
}
