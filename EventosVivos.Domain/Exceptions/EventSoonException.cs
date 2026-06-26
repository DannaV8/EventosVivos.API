namespace EventosVivos.Domain.Exceptions;
public sealed class EventSoonException : DomainException
{
    public EventSoonException()
        : base("EVENT_SOON",
            "Event starts in less than 1 hour.")
    { }
}
