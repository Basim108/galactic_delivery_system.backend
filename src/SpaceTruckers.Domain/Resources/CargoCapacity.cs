namespace SpaceTruckers.Domain.Resources;

public readonly record struct CargoCapacity(int Value)
{
    public static CargoCapacity From(int value)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, "Cargo capacity must be non-negative.");
        }

        return new CargoCapacity(value);
    }

    public override string ToString() => Value.ToString();
}
