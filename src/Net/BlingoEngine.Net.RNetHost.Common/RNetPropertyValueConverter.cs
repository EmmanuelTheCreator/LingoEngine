using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using AbstUI.Primitives;

namespace BlingoEngine.Net.RNetHost.Common;

/// <summary>
/// Shared helpers for converting string-based property values into strongly typed engine values.
/// </summary>
public static class RNetPropertyValueConverter
{
    private const BindingFlags PropertyFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy;

    public static bool TryCreatePropertyValue(object target, string propertyName, string value, out APropertyValue change)
    {
        change = default;
        if (!TryConvertPropertyValue(target, propertyName, value, out var converted))
        {
            return false;
        }

        change = new APropertyValue(propertyName, converted);
        return true;
    }

    public static bool TryConvertPropertyValue(object target, string propertyName, string value, out object? converted)
    {
        converted = null;
        var property = FindProperty(target.GetType(), propertyName);
        if (property is null)
        {
            return false;
        }

        return TryConvertToType(property.PropertyType, value, out converted);
    }

    private static PropertyInfo? FindProperty(Type type, string propertyName)
        => type.GetProperty(propertyName, PropertyFlags)
            ?? type.GetProperty(propertyName, PropertyFlags | BindingFlags.IgnoreCase);

    public static bool TryConvertToType(Type type, string value, out object? result)
    {
        result = null;
        var underlying = Nullable.GetUnderlyingType(type);
        if (underlying != null)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                result = null;
                return true;
            }

            type = underlying;
        }

        if (type == typeof(string))
        {
            result = value;
            return true;
        }

        if (type == typeof(bool))
        {
            if (bool.TryParse(value, out var boolResult))
            {
                result = boolResult;
                return true;
            }

            return false;
        }

        if (type.IsEnum)
        {
            if (Enum.TryParse(type, value, ignoreCase: true, out var enumResult))
            {
                result = enumResult;
                return true;
            }

            return false;
        }

        if (type == typeof(int))
        {
            if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intResult))
            {
                result = intResult;
                return true;
            }

            return false;
        }

        if (type == typeof(long))
        {
            if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var longResult))
            {
                result = longResult;
                return true;
            }

            return false;
        }

        if (type == typeof(short))
        {
            if (short.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var shortResult))
            {
                result = shortResult;
                return true;
            }

            return false;
        }

        if (type == typeof(byte))
        {
            if (byte.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var byteResult))
            {
                result = byteResult;
                return true;
            }

            return false;
        }

        if (type == typeof(float))
        {
            if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var floatResult))
            {
                result = floatResult;
                return true;
            }

            return false;
        }

        if (type == typeof(double))
        {
            if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var doubleResult))
            {
                result = doubleResult;
                return true;
            }

            return false;
        }

        if (type == typeof(decimal))
        {
            if (decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var decimalResult))
            {
                result = decimalResult;
                return true;
            }

            return false;
        }

        if (type == typeof(AColor))
        {
            if (TryConvertColor(value, out var color))
            {
                result = color;
                return true;
            }

            return false;
        }

        if (type == typeof(APoint))
        {
            if (TryConvertPoint(value, out var point))
            {
                result = point;
                return true;
            }

            return false;
        }

        var converter = TypeDescriptor.GetConverter(type);
        if (converter != null && converter.CanConvertFrom(typeof(string)))
        {
            try
            {
                result = converter.ConvertFrom(null, CultureInfo.InvariantCulture, value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        return false;
    }

    public static bool TryConvertColor(string value, out AColor color)
    {
        var trimmed = value.Trim();
        if (AColors.TryGetColor(trimmed, out color))
        {
            return true;
        }

        if (trimmed.StartsWith("#", StringComparison.Ordinal))
        {
            try
            {
                color = AColor.FromHex(trimmed);
                return true;
            }
            catch
            {
                color = default;
                return false;
            }
        }

        var parts = trimmed.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 3 &&
            byte.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var r) &&
            byte.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var g) &&
            byte.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var b))
        {
            byte a = 255;
            if (parts.Length >= 4)
            {
                byte.TryParse(parts[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out a);
            }

            color = AColor.FromRGB(r, g, b, -1, string.Empty, a);
            return true;
        }

        color = default;
        return false;
    }

    public static bool TryConvertPoint(string value, out APoint point)
    {
        var trimmed = value.Trim();
        if (trimmed.StartsWith("(", StringComparison.Ordinal) && trimmed.EndsWith(")", StringComparison.Ordinal))
        {
            trimmed = trimmed.Substring(1, trimmed.Length - 2);
        }

        var parts = trimmed.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 2 &&
            float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var x) &&
            float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var y))
        {
            point = new APoint(x, y);
            return true;
        }

        point = default;
        return false;
    }
}
