namespace EventosVivos.Domain.Exceptions;

public sealed class InvalidUserException : DomainException
{
    public InvalidUserException(string code, string message)
        : base(code, message)
    { }
}
