using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BlingoEngine.Legacy.Lingo.Analysis;
using BlingoEngine.Legacy.Lingo.Analysis.Passes;
using BlingoEngine.Legacy.Lingo.Syntax;
using Xunit;

namespace BlingoEngine.Legacy.Lingo.Tests;

public sealed class LegacyHandlerInvocationReturnTypeTests
{
    private const string Source = """
property spriteManager, myNum

on start
  spriteManager = script("SpriteManager").new(100)
end

on createBlock
  myNum = spriteManager.SAdd()
end

on updateSprite spriteNum
  spriteNum = spriteManager.SAdd()
end

-- script "SpriteManager"

on new me, capacity
  return me
end

on SAdd me
  return 42
end
""";

    [Fact]
    public void InvocationReturnType_AssignedToProperty_UsesHandlerReturnType()
    {
        var analysis = Analyze();
        var movieScope = analysis.Symbols.MovieScript;
        Assert.True(movieScope.Properties.TryGetValue("myNum", out var property));
        Assert.True(movieScope.Properties.TryGetValue("spriteManager", out var manager));
        Assert.Equal("SpriteManager", manager.ResolvedTypeName);

        var typeCollection = GetTypeCollection(analysis);
        var movieCollectionScope = GetScope(typeCollection, movieScope);
        var createBlockSymbol = movieScope.Handlers["createBlock"];
        var createBlockCollectionScope = GetHandlerScope(movieCollectionScope, "createBlock");

        var rhsTokens = GetAssignmentValueTokens(analysis, createBlockSymbol);
        var returnTypes = GetReturnTypes(analysis);

        var invocationType = DetermineInvocation(typeCollection, analysis.Symbols, movieCollectionScope, createBlockCollectionScope, rhsTokens, returnTypes);
        Assert.Equal("int", invocationType);

        MergeType(typeCollection, movieCollectionScope, createBlockCollectionScope, "myNum", invocationType);
        ApplyResolvedTypes(typeCollection);

        Assert.Equal("int", property.ResolvedTypeName);
        Assert.Contains("int", property.ResolvedTypeNames);
    }

    [Fact]
    public void InvocationReturnType_AssignedToParameter_UsesHandlerReturnType()
    {
        var analysis = Analyze();
        var movieScope = analysis.Symbols.MovieScript;
        var updateSpriteSymbol = movieScope.Handlers["updateSprite"];
        Assert.True(updateSpriteSymbol.Parameters.TryGetValue("spriteNum", out var parameter));

        var typeCollection = GetTypeCollection(analysis);
        var movieCollectionScope = GetScope(typeCollection, movieScope);
        var updateSpriteCollectionScope = GetHandlerScope(movieCollectionScope, "updateSprite");

        var rhsTokens = GetAssignmentValueTokens(analysis, updateSpriteSymbol);
        var returnTypes = GetReturnTypes(analysis);

        var invocationType = DetermineInvocation(typeCollection, analysis.Symbols, movieCollectionScope, updateSpriteCollectionScope, rhsTokens, returnTypes);
        Assert.Equal("int", invocationType);

        MergeType(typeCollection, movieCollectionScope, updateSpriteCollectionScope, "spriteNum", invocationType);
        ApplyResolvedTypes(typeCollection);

        Assert.Equal("int", parameter.ResolvedTypeName);
        Assert.Contains("int", parameter.ResolvedTypeNames);
    }

    private static BlLingoAnalysisResult Analyze()
    {
        var tokenizer = new BlLingoTokenizer();
        var tokens = tokenizer.Tokenize(Source);
        return BlLingoAnalyzer.Create(tokens).Run();
    }

    private static object GetTypeCollection(BlLingoAnalysisResult analysis)
    {
        Assert.True(analysis.Data.TryGetValue(BlLegacyTypeAnalysisPass.TypeCollectionKey, out var value));
        return value!;
    }

    private static object GetScope(object typeCollection, BlLingoClassSymbolTable classScope)
    {
        var collectionType = typeCollection.GetType();
        var getOrAddScope = collectionType.GetMethod("GetOrAddScope");
        Assert.NotNull(getOrAddScope);
        return getOrAddScope.Invoke(typeCollection, new object?[] { classScope })!;
    }

    private static object GetHandlerScope(object collectionScope, string handlerName)
    {
        var scopeType = collectionScope.GetType();
        var tryGetHandler = scopeType.GetMethod("TryGetHandler");
        Assert.NotNull(tryGetHandler);
        var args = new object?[] { handlerName, null };
        Assert.True((bool)tryGetHandler.Invoke(collectionScope, args)!);
        return args[1]!;
    }

    private static IReadOnlyDictionary<BlLingoHandlerSymbolTable, string?> GetReturnTypes(BlLingoAnalysisResult analysis)
    {
        Assert.True(analysis.TryGetData(BlLingoHandlerCodeBlockPass.HandlerReturnTypesKey, out IReadOnlyDictionary<BlLingoHandlerSymbolTable, string?>? returnTypes));
        return returnTypes!;
    }

    private static IReadOnlyList<BlSyntaxToken> GetAssignmentValueTokens(BlLingoAnalysisResult analysis, BlLingoHandlerSymbolTable handler)
    {
        Assert.True(analysis.TryGetData(BlLingoHandlerCodeBlockPass.HandlerCodeBlocksKey, out IReadOnlyDictionary<BlLingoHandlerSymbolTable, IReadOnlyList<BlLingoHandlerCodeBlock>>? blockMap));
        var blocks = blockMap![handler];
        var expression = blocks.First(block => block.Kind == BlLingoHandlerCodeBlockKind.Expression);

        var extractAssignment = typeof(BlLegacyTypeAnalysisPass).GetMethod("TryExtractAssignment", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(extractAssignment);
        var args = new object?[] { expression.Tokens, null, null };
        Assert.True((bool)extractAssignment.Invoke(null, args)!);
        return (IReadOnlyList<BlSyntaxToken>)args[2]!;
    }

    private static string DetermineInvocation(
        object typeCollection,
        BlLingoSymbolTable symbols,
        object collectionScope,
        object handlerScope,
        IReadOnlyList<BlSyntaxToken> valueTokens,
        IReadOnlyDictionary<BlLingoHandlerSymbolTable, string?> returnTypes)
    {
        var determineInvocation = typeof(BlLegacyTypeAnalysisPass).GetMethod("DetermineInvocationReturnType", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(determineInvocation);
        var result = (string?)determineInvocation.Invoke(null, new object?[]
        {
            symbols,
            typeCollection,
            collectionScope,
            handlerScope,
            valueTokens,
            returnTypes,
        });
        return result ?? string.Empty;
    }

    private static void MergeType(
        object typeCollection,
        object collectionScope,
        object handlerScope,
        string targetName,
        string typeName)
    {
        var mergeMethod = typeof(BlLegacyTypeAnalysisPass).GetMethod("MergePropertyOrParameter", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(mergeMethod);
        mergeMethod.Invoke(null, new object?[]
        {
            typeCollection,
            collectionScope,
            handlerScope,
            targetName,
            typeName,
        });
    }

    private static void ApplyResolvedTypes(object typeCollection)
    {
        var collectionType = typeCollection.GetType();
        var applyMethod = collectionType.GetMethod("ApplyResolvedTypes");
        Assert.NotNull(applyMethod);
        applyMethod.Invoke(typeCollection, Array.Empty<object?>());
    }
}
