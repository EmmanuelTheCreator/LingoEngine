using System;

namespace BlingoEngine.Legacy.Lingo.CodeGen;

/// <summary>
/// Configures naming conventions for generated legacy Lingo C# classes.
/// </summary>
public sealed class BlLegacyClassGeneratorOptions
{
    private string _behaviorSuffix = "Behavior";
    private string _parentSuffix = "Parent";
    private string _movieScriptSuffix = "MovieScript";
    private string _scriptSuffix = "Script";

    /// <summary>
    /// Gets or sets the suffix appended to generated behavior script classes.
    /// </summary>
    public string BehaviorSuffix
    {
        get => _behaviorSuffix;
        set => _behaviorSuffix = ValidateSuffix(value, nameof(BehaviorSuffix));
    }

    /// <summary>
    /// Gets or sets the suffix appended to generated parent script classes.
    /// </summary>
    public string ParentSuffix
    {
        get => _parentSuffix;
        set => _parentSuffix = ValidateSuffix(value, nameof(ParentSuffix));
    }

    /// <summary>
    /// Gets or sets the suffix appended to generated movie script classes.
    /// </summary>
    public string MovieScriptSuffix
    {
        get => _movieScriptSuffix;
        set => _movieScriptSuffix = ValidateSuffix(value, nameof(MovieScriptSuffix));
    }

    /// <summary>
    /// Gets or sets the suffix appended to generated script classes when the type is unknown.
    /// </summary>
    public string ScriptSuffix
    {
        get => _scriptSuffix;
        set => _scriptSuffix = ValidateSuffix(value, nameof(ScriptSuffix));
    }

    /// <summary>
    /// Creates a copy of the current options instance.
    /// </summary>
    public BlLegacyClassGeneratorOptions Clone()
    {
        return new BlLegacyClassGeneratorOptions
        {
            BehaviorSuffix = BehaviorSuffix,
            ParentSuffix = ParentSuffix,
            MovieScriptSuffix = MovieScriptSuffix,
            ScriptSuffix = ScriptSuffix,
        };
    }

    private static string ValidateSuffix(string? value, string propertyName)
    {
        if (value is null)
        {
            throw new ArgumentNullException(propertyName);
        }

        return value;
    }
}
