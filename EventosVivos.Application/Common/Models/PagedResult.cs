namespace EventosVivos.Application.Common.Models;

/// <summary>
/// Generic paged result wrapper: a single page of items plus the total
/// count across all pages, so callers can compute how many pages exist.
/// </summary>
public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize
);
