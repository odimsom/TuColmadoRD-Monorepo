namespace TuColmadoRD.Core.Domain.Base;

/// <summary>
/// Base class for smart-enum like domain value objects.
/// </summary>
public abstract class Enumeration : IComparable
{
    /// <summary>
    /// Numeric identifier persisted in storage.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Human-readable name.
    /// </summary>
    public string Name { get; }

    protected Enumeration(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public override string ToString() => Name;

    public override bool Equals(object? obj)
    {
        if (obj is not Enumeration other)
        {
            return false;
        }

        return GetType() == obj.GetType() && Id == other.Id;
    }

    public override int GetHashCode() => Id.GetHashCode();

    public int CompareTo(object? other) => Id.CompareTo(((Enumeration?)other)?.Id);
}
