using System;

namespace BlingoEngine.Legacy.Lingo.Analysis;

internal static class BlLegacyHandlerFilters
{
    public static bool ShouldSkipHandler(BlLingoHandlerSymbolTable handler)
    {
        if (handler is null)
        {
            return true;
        }

        if (string.Equals(handler.OriginalName, BlLingoHandlerSymbolTable.ImplicitHandlerName, StringComparison.Ordinal))
        {
            return true;
        }

        if (IsPropertyDescriptionUtilityHandler(handler.OriginalName) ||
            IsPropertyDescriptionUtilityHandler(handler.Symbol.Name))
        {
            return true;
        }

        return false;
    }

    private static bool IsPropertyDescriptionUtilityHandler(string? name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return false;
        }

        return name.Equals("getBehaviorDescription", StringComparison.OrdinalIgnoreCase)
            || name.Equals("getBehaviorTooltip", StringComparison.OrdinalIgnoreCase)
            || name.Equals("isOKToAttach", StringComparison.OrdinalIgnoreCase);
    }
}
