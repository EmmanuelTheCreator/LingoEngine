using System;
using System.Collections.Generic;

namespace BlingoEngine.Legacy.Lingo.Analysis.Passes;

public sealed class BlLegacyTypeCollectionPass : BlLingoAnalysisPass
{
    public const string TypeCollectionKey = "LegacyTypeCollection";

    public BlLegacyTypeCollectionPass()
        : base("LegacyTypeCollection")
    {
    }

    public override void Execute(BlLingoAnalysisContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var symbols = context.Symbols;
        var collection = new BlLegacyTypeCollection();

        foreach (var classScope in EnumerateClasses(symbols))
        {
            var scope = collection.GetOrAddScope(classScope);

            foreach (var property in classScope.Properties.Values)
            {
                if (property is not null)
                {
                    scope.RegisterProperty(property);
                }
            }

            foreach (var handler in classScope.Handlers.Values)
            {
                if (handler is null)
                {
                    continue;
                }

                var handlerScope = scope.RegisterHandler(handler);
                collection.RegisterHandler(handlerScope);
            }
        }

        context.SetData(TypeCollectionKey, collection);
    }

    private static IEnumerable<BlLingoClassSymbolTable> EnumerateClasses(BlLingoSymbolTable symbols)
    {
        yield return symbols.MovieScript;
        foreach (var scope in symbols.ClassScopes.Values)
        {
            yield return scope;
        }
    }
}

