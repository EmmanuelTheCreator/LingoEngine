using System;

namespace BlingoEngine.Legacy.Lingo.Analysis.Passes;

/// <summary>
/// Applies resolved legacy member and parameter types captured by earlier passes.
/// </summary>
public sealed class BlLegacyMemberTypeInferencePass : BlLingoAnalysisPass
{
    public BlLegacyMemberTypeInferencePass()
        : base("LegacyMemberTypeResolution")
    {
    }

    public override void Execute(BlLingoAnalysisContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.TryGetData(BlLegacyTypeCollectionPass.TypeCollectionKey, out BlLegacyTypeCollection? collection) ||
            collection is null)
        {
            return;
        }

        collection.ApplyResolvedTypes();
    }
}

