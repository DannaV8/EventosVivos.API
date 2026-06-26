namespace EventosVivos.Domain.Exceptions;
public sealed class InvalidEventException : DomainException
{
    public InvalidEventException(string code, string message)
        : base(code, message)
    { }
}
