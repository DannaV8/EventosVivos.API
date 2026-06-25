using MediatR;

namespace EventosVivos.Application.Reports.Queries.EventOccupancy;

public sealed record EventOccupancyQuery(Guid EventId) : IRequest<EventOccupancyDto>;
