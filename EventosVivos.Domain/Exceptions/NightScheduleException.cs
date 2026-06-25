namespace EventosVivos.Domain.Exceptions;

/// <summary>RN-03: Saturday or Sunday events cannot start after 22:00.</summary>
public sealed class NightScheduleException : DomainException
{
    public NightScheduleException()
        : base("NIGHT_TIME",
            "Los eventos de fin de semana no pueden iniciar después de las 22:00.")
    { }
}
