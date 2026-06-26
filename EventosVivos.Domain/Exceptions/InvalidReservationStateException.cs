namespace EventosVivos.Domain.Exceptions;
public sealed class InvalidReservationStateException : DomainException
{
    public InvalidReservationStateException(string code, string message)
        : base(code, message)
    { }
}
