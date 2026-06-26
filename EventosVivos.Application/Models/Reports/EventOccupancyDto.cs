namespace EventosVivos.Application.Models.Reports;

public sealed record EventOccupancyDto(
    Guid EventId,
    string Title,
    int SoldTickets,
    int AvailableTickets,
    double OccupancyPercentage,
    decimal TotalRevenue,
    string Status
);
