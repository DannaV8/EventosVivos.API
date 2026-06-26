namespace EventosVivos.Domain.Exceptions;

public sealed class CapacityExceededException : DomainException
{
    public CapacityExceededException(int requested, int available)
        : base("CAPACITY_EXCEEDED",
            $"Not enough tickets. Requested: {requested}, Available: {available}")
    { }
}
