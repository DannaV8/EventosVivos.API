namespace EventosVivos.Domain.Exceptions;

public sealed class VenueConflictException : DomainException
{
    public VenueConflictException()
        : base("VENUE_CONFLICT",
            "The venue already has an active event in that time slot.")
    { }
}
