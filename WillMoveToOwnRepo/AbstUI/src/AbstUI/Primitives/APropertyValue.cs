namespace AbstUI.Primitives;

public readonly record struct APropertyValue(string PropertyName, object? Value)
{
    public bool TryGetFloat(out float result) => TryGetFloat(Value, out result);

    public bool TryGetInt(out int result) => TryGetInt(Value, out result);

    public static bool TryGetFloat(object? value, out float result)
    {
        switch (value)
        {
            case float f:
                result = f;
                return true;
            case double d:
                result = (float)d;
                return true;
            case int i:
                result = i;
                return true;
            case long l:
                result = l;
                return true;
            case short s:
                result = s;
                return true;
            case byte b:
                result = b;
                return true;
        }

        result = default;
        return false;
    }

    public static bool TryGetInt(object? value, out int result)
    {
        switch (value)
        {
            case int i:
                result = i;
                return true;
            case long l:
                result = (int)l;
                return true;
            case short s:
                result = s;
                return true;
            case byte b:
                result = b;
                return true;
            case float f:
                result = (int)f;
                return true;
            case double d:
                result = (int)d;
                return true;
        }

        result = default;
        return false;
    }
}
