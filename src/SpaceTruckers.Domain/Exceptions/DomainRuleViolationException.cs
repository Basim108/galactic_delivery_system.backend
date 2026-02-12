namespace SpaceTruckers.Domain.Exceptions;

public sealed class DomainRuleViolationException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}
