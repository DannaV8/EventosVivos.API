namespace EventosVivos.Domain.Exceptions;

/// <summary>RN-03: Saturday or Sunday events cannot start after 22:00.</summary>
public sealed class NightScheduleException : DomainException
{
    public NightScheduleException()
        : base("NIGHT_TIME",
            "Weekend events cannot start after 10:00 PM.")
    { }
}
