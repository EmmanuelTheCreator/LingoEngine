using System;
using System.Collections.Generic;

namespace BlingoEngine.Legacy.Lingo.Analysis.Passes;

/// <summary>
/// Captures the set of known class names so later passes can resolve references to them.
/// </summary>
public sealed class BlLingoClassLinkPass : BlLingoAnalysisPass
{
    public const string KnownClassesKey = nameof(KnownClassesKey);

    public BlLingoClassLinkPass()
        : base("ClassLink")
    {
    }

    public override void Execute(BlLingoAnalysisContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var symbols = context.Symbols;
        var known = new HashSet<string>(symbols.Classes.Keys, StringComparer.OrdinalIgnoreCase);

        foreach (var classScope in symbols.ClassScopes.Values)
        {
            known.Add(classScope.Symbol.Name);
        }

        if (context.TryGetData<List<BlCodeSymbol>>(BlLingoTypeLinkPass.PendingTypeSymbolsKey, out var pending) &&
            pending is not null)
        {
            foreach (var symbol in pending)
            {
                if (symbol.ResolvedTypeName is not null)
                {
                    continue;
                }

                var typeCode = symbol.TypeCode;
                if (typeCode is null)
                {
                    continue;
                }

                if (known.Contains(typeCode))
                {
                    symbol.SetResolvedTypeName(typeCode);
                }
            }
        }

        context.SetData(KnownClassesKey, known);
    }
}
