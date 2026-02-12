namespace SpaceTruckers.Domain.Ids;

public readonly record struct VehicleId(Guid Value)
{
    public static VehicleId New() => new(Guid.CreateVersion7());

    public static VehicleId Parse(string value) => new(Guid.Parse(value));

    public static bool TryParse(string? value, out VehicleId id)
    {
        if (Guid.TryParse(value, out var guid))
        {
            id = new VehicleId(guid);
            return true;
        }

        id = default;
        return false;
    }

    public override string ToString() => Value.ToString();
}
