namespace EventosVivos.Domain.Exceptions;

public sealed class CapacityExceededException : DomainException
{
    public CapacityExceededException(int requested, int available)
        : base("CAPACITY_EXCEEDED",
            $"No hay suficientes entradas. Solicitadas: {requested}, Disponibles: {available}")
    { }
}
