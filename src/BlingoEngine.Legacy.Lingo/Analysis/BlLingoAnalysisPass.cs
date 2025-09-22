using System;

namespace BlingoEngine.Legacy.Lingo.Analysis;

/// <summary>
/// Represents a unit of work executed during analysis.
/// </summary>
public abstract class BlLingoAnalysisPass
{
    /// <summary>
    /// Initializes a new <see cref="BlLingoAnalysisPass"/> with the provided name.
    /// </summary>
    /// <param name="name">A friendly name used for diagnostics.</param>
    protected BlLingoAnalysisPass(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
    }

    /// <summary>
    /// Gets the friendly name of the pass.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Executes the pass using the supplied context.
    /// </summary>
    public abstract void Execute(BlLingoAnalysisContext context);
}
