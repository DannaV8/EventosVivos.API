namespace EventosVivos.Application.Reports.Queries.EventOccupancy;

public sealed record EventOccupancyDto(
    Guid EventId,
    string Title,
    int SoldTickets,
    int AvailableTickets,
    double OccupancyPercentage,
    decimal TotalRevenue,
    string Status
);
