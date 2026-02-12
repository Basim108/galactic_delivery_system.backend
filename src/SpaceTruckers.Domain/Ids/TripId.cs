namespace SpaceTruckers.Domain.Ids;

public readonly record struct TripId(Guid Value)
{
    public static TripId New() => new(Guid.CreateVersion7());

    public static TripId Parse(string value) => new(Guid.Parse(value));

    public static bool TryParse(string? value, out TripId id)
    {
        if (Guid.TryParse(value, out var guid))
        {
            id = new TripId(guid);
            return true;
        }

        id = default;
        return false;
    }

    public override string ToString() => Value.ToString();
}
