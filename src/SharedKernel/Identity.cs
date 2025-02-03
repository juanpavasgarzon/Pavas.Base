namespace SharedKernel;

public abstract class Identity<T>(T value) where T : struct
{
    public T Value { get; } = value;

    public override bool Equals(object? obj)
    {
        return obj is Identity<T> other && EqualityComparer<T>.Default.Equals(Value, other.Value);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string? ToString()
    {
        return Value.ToString();
    }
}
