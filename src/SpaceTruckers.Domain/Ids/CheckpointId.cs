namespace SpaceTruckers.Domain.Ids;

public readonly record struct CheckpointId(Guid Value)
{
    public static CheckpointId New() => new(Guid.CreateVersion7());

    public static CheckpointId Parse(string value) => new(Guid.Parse(value));

    public static bool TryParse(string? value, out CheckpointId id)
    {
        if (Guid.TryParse(value, out var guid))
        {
            id = new CheckpointId(guid);
            return true;
        }

        id = default;
        return false;
    }

    public override string ToString() => Value.ToString();
}
