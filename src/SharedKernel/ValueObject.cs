namespace SharedKernel;

public abstract class ValueObject
{
    private static bool EqualOperator(ValueObject left, ValueObject right)
    {
        if (ReferenceEquals(left, null) ^ ReferenceEquals(right, null))
        {
            return false;
        }

        return ReferenceEquals(left, right) || left!.Equals(right);
    }

    private static bool NotEqualOperator(ValueObject left, ValueObject right)
    {
        return !EqualOperator(left, right);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null || GetType() != obj.GetType())
        {
            return false;
        }

        var valueObject = (ValueObject)obj;
        var components = valueObject.GetEqualityComponents();

        return GetEqualityComponents().SequenceEqual(components);
    }

    public override int GetHashCode()
    {
        return GetEqualityComponents().Aggregate(0, (hash, obj) =>
        {
            var hashCode = obj is not null ? obj.GetHashCode() : 0;
            return HashCode.Combine(hash, hashCode);
        });
    }

    protected abstract IEnumerable<object?> GetEqualityComponents();

    public override string ToString()
    {
        return string.Join(", ", GetEqualityComponents());
    }

    public static bool operator ==(ValueObject one, ValueObject two)
    {
        return EqualOperator(one, two);
    }

    public static bool operator !=(ValueObject one, ValueObject two)
    {
        return NotEqualOperator(one, two);
    }
}
