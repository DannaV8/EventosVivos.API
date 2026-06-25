using EventosVivos.Domain.Enums;
using EventosVivos.Domain.Exceptions;

namespace EventosVivos.Domain.Entities;

public sealed class Event
{
    private bool _cancelled;

    public Guid Id { get; private set; }
    public string Title { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public int VenueId { get; private set; }
    public Venue Venue { get; private set; } = null!;
    public int MaxCapacity { get; private set; }
    public DateTime StartDateTime { get; private set; }
    public DateTime EndDateTime { get; private set; }
    public decimal TicketPrice { get; private set; }
    public EventType Type{ get; private set; }

    public EventStatus Status =>
        _cancelled                       ? EventStatus.Cancelled  :
        DateTime.UtcNow > EndDateTime    ? EventStatus.Completed  :
                                           EventStatus.Active;


    private Event() { }

    public static Event Create(
        string title, string description, Venue venue,
        int maximumCapacity, DateTime start, DateTime end,
        decimal price, EventType type)
    {
        ArgumentNullException.ThrowIfNull(venue);

        if (string.IsNullOrWhiteSpace(title) || title.Length < 5 || title.Length > 100)
            throw new InvalidEventException("INVALID_TITLE",
                "El título debe tener entre 5 y 100 caracteres.");

        if (string.IsNullOrWhiteSpace(description) || description.Length < 10 || description.Length > 500)
            throw new InvalidEventException("INVALID_DESCRIPTION",
                "La descripción debe tener entre 10 y 500 caracteres.");

        if (maximumCapacity < 1 || maximumCapacity > venue.Capacity)
            throw new InvalidEventException("INVALID_CAPACITY",
                $"La capacidad debe estar entre 1 y {venue.Capacity} (capacidad del venue).");

        if (start <= DateTime.UtcNow)
            throw new InvalidEventException("PAST_EVENT",
                "La fecha de inicio debe ser futura.");

        if (end <= start)
            throw new InvalidEventException("INVALID_END_DATE",
                "La fecha de fin debe ser posterior al inicio.");

        var isWeekend = start.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
        if (isWeekend && start.Hour >= 22)
            throw new NightScheduleException();

        return new Event
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description,
            VenueId = venue.Id,
            Venue = venue,
            MaxCapacity = maximumCapacity,
            StartDateTime = start,
            EndDateTime = end,
            TicketPrice = price,
            Type = type,
            _cancelled = false
        };
    }

    public void Cancel() => _cancelled = true;
}
