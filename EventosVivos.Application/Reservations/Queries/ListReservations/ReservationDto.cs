namespace EventosVivos.Application.Reservations.Queries.ListReservations;

public sealed record ReservationDto(
    Guid Id,
    Guid EventId,
    string EventTitle,
    int Quantity,
    string Status,
    string? ReservationCode,
    DateTime CreationDate
);
