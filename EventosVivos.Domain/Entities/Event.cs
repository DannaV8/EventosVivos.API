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
                "Title must be between 5 and 100 characters.");

        if (string.IsNullOrWhiteSpace(description) || description.Length < 10 || description.Length > 500)
            throw new InvalidEventException("INVALID_DESCRIPTION",
                "Description must be between 10 and 500 characters.");

        if (maximumCapacity < 1 || maximumCapacity > venue.Capacity)
            throw new InvalidEventException("INVALID_CAPACITY",
                $"Capacity must be between 1 and {venue.Capacity} (venue capacity).");

        if (start <= DateTime.UtcNow)
            throw new InvalidEventException("PAST_EVENT",
                "Start date must be in the future.");

        if (end <= start)
            throw new InvalidEventException("INVALID_END_DATE",
                "End date must be after start date.");

        if (price < 0)
            throw new InvalidEventException("INVALID_PRICE",
                "Price must be non-negative.");

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
