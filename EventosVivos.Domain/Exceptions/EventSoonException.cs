namespace EventosVivos.Domain.Exceptions;
public sealed class EventSoonException : DomainException
{
    public EventSoonException()
        : base("EVENT_SOON",
            "El evento comienza en menos de 1 hora.")
    { }
}
