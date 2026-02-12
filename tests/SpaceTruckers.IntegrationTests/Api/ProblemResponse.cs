namespace SpaceTruckers.IntegrationTests.Api;

public sealed record ProblemResponse(string Title, int Status, string Detail, string Code);
