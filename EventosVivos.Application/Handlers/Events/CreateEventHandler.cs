using EventosVivos.Application.Common.Exceptions;
using EventosVivos.Application.Common.Interfaces;
using EventosVivos.Application.Models.Events;
using EventosVivos.Domain.Entities;
using EventosVivos.Domain.Exceptions;
using EventosVivos.Domain.Specifications;
using MediatR;

namespace EventosVivos.Application.Handlers.Events;

public sealed class CreateEventHandler : IRequestHandler<CreateEventCommand, Guid>
{
    private readonly IEventRepository _events;
    private readonly IVenueRepository _venues;
    private readonly IUnitOfWork _uow;
    private readonly VenueAvailabilitySpec _venueSpec = new();

    public CreateEventHandler(
        IEventRepository events, IVenueRepository venues, IUnitOfWork uow)
    {
        _events = events;
        _venues = venues;
        _uow = uow;
    }

    public async Task<Guid> Handle(CreateEventCommand cmd, CancellationToken ct)
    {
        var venue = await _venues.GetByIdAsync(cmd.VenueId, ct)
            ?? throw new NotFoundException($"Venue {cmd.VenueId} not found.");

        var activeEvents = await _events.GetActiveByVenueAsync(cmd.VenueId, ct);
        if (_venueSpec.HasConflict(cmd.VenueId, cmd.StartDateTime, cmd.EndDateTime, activeEvents))
            throw new VenueConflictException();

        var ev = Event.Create(
            cmd.Title, cmd.Description, venue,
            cmd.MaxCapacity, cmd.StartDateTime, cmd.EndDateTime,
            cmd.TicketPrice, cmd.Type);

        await _events.AddAsync(ev, ct);
        await _uow.SaveChangesAsync(ct);
        return ev.Id;
    }
}
