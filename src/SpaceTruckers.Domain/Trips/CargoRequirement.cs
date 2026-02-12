namespace SpaceTruckers.Domain.Trips;

public readonly record struct CargoRequirement(int Value)
{
    public static CargoRequirement From(int value)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, "Cargo requirement must be non-negative.");
        }

        return new CargoRequirement(value);
    }

    public override string ToString() => Value.ToString();
}
