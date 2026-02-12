namespace SpaceTruckers.Application.Exceptions;

public sealed class OptimisticConcurrencyException(string message) : Exception(message);
