using System;
using System.Reflection;

namespace BlingoEngine.Tests.Fakes;

internal static class ReflectionTestHelper
{
    public static void SetPrivateField(object target, string fieldName, object? value)
    {
        var type = target.GetType();
        while (type != null)
        {
            var field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field != null)
            {
                field.SetValue(target, value);
                return;
            }

            type = type.BaseType;
        }

        throw new InvalidOperationException($"Field '{fieldName}' not found on type '{target.GetType()}'.");
    }

    public static void SetAutoProperty<T>(object target, string propertyName, T value)
    {
        SetPrivateField(target, $"<{propertyName}>k__BackingField", value);
    }
}
