namespace Grapio.Server;

public class FeatureFlag: IEquatable<FeatureFlag>
{
    public string? FlagKey { get; } = string.Empty;
    public string? Value { get; } = string.Empty;
    public string? Consumer { get; } = string.Empty;

    private FeatureFlag()
    {
    }

    public static FeatureFlag Null = new NullFeatureFlag();

    private class NullFeatureFlag : FeatureFlag
    {
    }
    
    public FeatureFlag(string flagKey, string value, string consumer = "*")
    {
        ArgumentException.ThrowIfNullOrEmpty(flagKey, nameof(flagKey));
        ArgumentNullException.ThrowIfNull(value, nameof(value));
        
        FlagKey = flagKey;
        Value = value;
        Consumer = consumer;
    }

    public override string ToString()
    {
        return $"{Consumer}.{FlagKey}=>{Value}";
    }

    public bool Equals(FeatureFlag? other)
    {
        if (ReferenceEquals(null, other)) 
            return false;
        
        if (ReferenceEquals(this, other)) 
            return true;
        
        return string.Equals(FlagKey, other.FlagKey, StringComparison.InvariantCultureIgnoreCase) && 
               string.Equals(Value, other.Value, StringComparison.InvariantCultureIgnoreCase) && 
               string.Equals(Consumer, other.Consumer, StringComparison.InvariantCultureIgnoreCase);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) 
            return false;
        
        if (ReferenceEquals(this, obj)) 
            return true;
        
        return obj.GetType() == GetType() && Equals((FeatureFlag)obj);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(FlagKey, StringComparer.InvariantCultureIgnoreCase);
        hashCode.Add(Value, StringComparer.InvariantCultureIgnoreCase);
        hashCode.Add(Consumer, StringComparer.InvariantCultureIgnoreCase);
        return hashCode.ToHashCode();
    }

    public static bool operator ==(FeatureFlag? left, FeatureFlag? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(FeatureFlag? left, FeatureFlag? right)
    {
        return !Equals(left, right);
    }
}