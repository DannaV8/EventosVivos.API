using EventosVivos.Application.Common.Interfaces;
using EventosVivos.Application.Models.Reports;
using MediatR;

namespace EventosVivos.Application.Handlers.Reports;

public sealed class ListEventOccupanciesHandler(IEventRepository events)
    : IRequestHandler<ListEventOccupanciesQuery, IReadOnlyList<EventOccupancyDto>>
{
    public async Task<IReadOnlyList<EventOccupancyDto>> Handle(
        ListEventOccupanciesQuery query, CancellationToken ct)
        => await events.ListOccupanciesAsync(ct);
}
