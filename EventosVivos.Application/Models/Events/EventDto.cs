namespace EventosVivos.Application.Models.Events;

public sealed record EventDto(
    Guid Id,
    string Title,
    string Description,
    int VenueId,
    string VenueName,
    int MaxCapacity,
    DateTime StartDateTime,
    DateTime EndDateTime,
    decimal TicketPrice,
    string Type,
    string Status,
    int ConfirmedTickets,
    int LostTickets
);
