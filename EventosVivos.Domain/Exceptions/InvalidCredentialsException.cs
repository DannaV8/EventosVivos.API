namespace EventosVivos.Domain.Exceptions;
public sealed class InvalidCredentialsException : DomainException
{
    public InvalidCredentialsException()
        : base("INVALID_CREDENTIALS",
            "Invalid email or password.")
    { }
}
