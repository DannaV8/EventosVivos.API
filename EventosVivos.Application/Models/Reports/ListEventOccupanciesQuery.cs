using MediatR;

namespace EventosVivos.Application.Models.Reports;

public sealed record ListEventOccupanciesQuery : IRequest<IReadOnlyList<EventOccupancyDto>>;
