using EventosVivos.Application.Models.Events;
using EventosVivos.Application.Models.Reports;
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
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 9,
        CancellationToken ct = default)
    {
        var query = new ListEventsQuery(
            type,
            startDate.HasValue ? DateTime.SpecifyKind(startDate.Value, DateTimeKind.Utc) : null,
            endDate.HasValue   ? DateTime.SpecifyKind(endDate.Value,   DateTimeKind.Utc) : null,
            venueId, status, title, page, pageSize);
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetEventByIdQuery(id), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}/report")]
    public async Task<IActionResult> Report(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new EventOccupancyQuery(id), ct);
        return Ok(result);
    }
}
