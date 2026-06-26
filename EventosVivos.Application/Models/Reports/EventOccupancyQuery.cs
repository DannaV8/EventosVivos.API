using MediatR;

namespace EventosVivos.Application.Models.Reports;

public sealed record EventOccupancyQuery(Guid EventId) : IRequest<EventOccupancyDto>;
