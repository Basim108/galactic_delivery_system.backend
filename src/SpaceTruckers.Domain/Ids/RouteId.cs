namespace SpaceTruckers.Domain.Ids;

public readonly record struct RouteId(Guid Value)
{
    public static RouteId New() => new(Guid.CreateVersion7());

    public static RouteId Parse(string value) => new(Guid.Parse(value));

    public static bool TryParse(string? value, out RouteId id)
    {
        if (Guid.TryParse(value, out var guid))
        {
            id = new RouteId(guid);
            return true;
        }

        id = default;
        return false;
    }

    public override string ToString() => Value.ToString();
}
