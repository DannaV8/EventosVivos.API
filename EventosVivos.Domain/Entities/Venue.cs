namespace EventosVivos.Domain.Entities;
public sealed class Venue
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    public int Capacity { get; init; }
    public string City { get; init; } = null!;
}
