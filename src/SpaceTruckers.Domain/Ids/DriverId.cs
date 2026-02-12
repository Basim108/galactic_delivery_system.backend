namespace SpaceTruckers.Domain.Ids;

public readonly record struct DriverId(Guid Value)
{
    public static DriverId New() => new(Guid.CreateVersion7());

    public static DriverId Parse(string value) => new(Guid.Parse(value));

    public static bool TryParse(string? value, out DriverId id)
    {
        if (Guid.TryParse(value, out var guid))
        {
            id = new DriverId(guid);
            return true;
        }

        id = default;
        return false;
    }

    public override string ToString() => Value.ToString();
}
