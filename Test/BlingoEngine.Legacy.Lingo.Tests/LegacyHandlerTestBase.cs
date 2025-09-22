using BlingoEngine.Legacy.Lingo.Analysis;
using BlingoEngine.Legacy.Lingo.CodeGen;

namespace BlingoEngine.Legacy.Lingo.Tests;

public abstract class LegacyHandlerTestBase
{
    protected static string GenerateBehavior(string handlerSource)
    {
        var generator = new BlLegacyClassGenerator();
        return generator.GenerateClass("TestScript", handlerSource, BlLingoScriptKind.Behavior);
    }
}
