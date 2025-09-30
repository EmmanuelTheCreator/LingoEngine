using System;
using System.Reflection;

namespace Blingo.PacMan.Tests.TestUtilities;

/// <summary>
/// Provides helpers for manipulating private instance fields during unit tests so
/// behaviour objects can be configured without public setters.
/// </summary>
internal static class PrivateFieldAccessor
{
    /// <summary>
    /// Reads the value of a private instance field from <paramref name="target"/>.
    /// </summary>
    /// <typeparam name="T">Expected field type.</typeparam>
    /// <param name="target">Object that contains the field.</param>
    /// <param name="fieldName">Name of the field to read.</param>
    /// <returns>The field value cast to <typeparamref name="T"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="target"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the field cannot be found.</exception>
    public static T GetField<T>(object target, string fieldName)
    {
        if (target is null)
        {
            throw new ArgumentNullException(nameof(target));
        }

        var field = GetFieldInfo(target.GetType(), fieldName);
        var value = field.GetValue(target);
        if (value is null)
        {
            throw new InvalidOperationException($"Field '{fieldName}' did not contain a value of type {typeof(T)}.");
        }

        return (T)value;
    }

    /// <summary>
    /// Writes <paramref name="value"/> into a private instance field on <paramref name="target"/>.
    /// </summary>
    /// <param name="target">Object that owns the private field.</param>
    /// <param name="fieldName">Name of the field to write.</param>
    /// <param name="value">Value to assign to the field.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="target"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the field cannot be found.</exception>
    public static void SetField(object target, string fieldName, object? value)
    {
        if (target is null)
        {
            throw new ArgumentNullException(nameof(target));
        }

        var field = GetFieldInfo(target.GetType(), fieldName);
        field.SetValue(target, value);
    }

    private static FieldInfo GetFieldInfo(Type type, string fieldName)
    {
        var field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        return field ?? throw new InvalidOperationException($"Field '{fieldName}' was not found on type '{type}'.");
    }
}
