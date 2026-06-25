namespace EventosVivos.Domain.Exceptions;

public sealed class VenueConflictException : DomainException
{
    public VenueConflictException()
        : base("VENUE_CONFLICT",
            "El venue ya tiene un evento activo en ese horario.")
    { }
}
