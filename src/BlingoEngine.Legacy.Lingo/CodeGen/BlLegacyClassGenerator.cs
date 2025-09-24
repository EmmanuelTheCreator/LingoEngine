using System;
using System.Collections.Generic;
using BlingoEngine.Legacy.Lingo.Analysis;
using BlingoEngine.Legacy.Lingo.Analysis.Passes;
using BlingoEngine.Legacy.Lingo.Syntax;

namespace BlingoEngine.Legacy.Lingo.CodeGen;

/// <summary>
/// Generates C# class skeletons for legacy Lingo scripts using analysis metadata.
/// </summary>
public sealed class BlLegacyClassGenerator
{
    private static readonly (string Handler, string Interface)[] s_eventInterfaces =
    [
        ("blur", "IHasBlurEvent"),
        ("focus", "IHasFocusEvent"),
        ("keydown", "IHasKeyDownEvent"),
        ("keyup", "IHasKeyUpEvent"),
        ("mousewithin", "IHasMouseWithinEvent"),
        ("mouseleave", "IHasMouseLeaveEvent"),
        ("mousedown", "IHasMouseDownEvent"),
        ("mouseup", "IHasMouseUpEvent"),
        ("mousemove", "IHasMouseMoveEvent"),
        ("mousewheel", "IHasMouseWheelEvent"),
        ("mouseenter", "IHasMouseEnterEvent"),
        ("mouseexit", "IHasMouseExitEvent"),
        ("beginsprite", "IHasBeginSpriteEvent"),
        ("endsprite", "IHasEndSpriteEvent"),
        ("stepframe", "IHasStepFrameEvent"),
        ("prepareframe", "IHasPrepareFrameEvent"),
        ("enterframe", "IHasEnterFrameEvent"),
        ("exitframe", "IHasExitFrameEvent"),
    ];

    private readonly BlLingoTokenizer _tokenizer = new();
    private readonly BlLegacyClassGeneratorOptions _options;

    /// <summary>
    /// Initializes a generator with default options.
    /// </summary>
    public BlLegacyClassGenerator()
        : this(new BlLegacyClassGeneratorOptions())
    {
    }

    /// <summary>
    /// Initializes a generator with the provided options.
    /// </summary>
    /// <param name="options">Naming and formatting options for the generated class.</param>
    public BlLegacyClassGenerator(BlLegacyClassGeneratorOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options.Clone();
    }

    /// <summary>
    /// Generates the class declaration for the provided script source.
    /// </summary>
    /// <param name="scriptName">The name of the script being converted.</param>
    /// <param name="source">The original Lingo source text.</param>
    /// <param name="declaredKind">Optional explicit script kind metadata.</param>
    public string GenerateClass(string scriptName, string source, BlLingoScriptKind declaredKind = BlLingoScriptKind.Unknown)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scriptName);
        source ??= string.Empty;

        var tokens = _tokenizer.Tokenize(source);
        var analysis = BlLingoAnalyzer.Create(tokens).Run();
        var classScope = ResolveClassScope(scriptName, analysis);
        var scriptKind = DetermineScriptKind(declaredKind, classScope);

        var className = BlCSharpName.ComposeClassName(scriptName, scriptKind, _options);
        var baseType = GetBaseType(scriptKind);
        var interfaces = CollectInterfaces(classScope);

        var writer = new BlCSharpCodeWriter();
        var handlerConverter = new BlLegacyHandlerConverter(source, tokens, _options, analysis);
        var interfaceSuffix = interfaces.Count > 0 ? ", " + string.Join(", ", interfaces) : string.Empty;

        writer.WriteLine($"public class {className} : {baseType}{interfaceSuffix}");
        writer.WriteLine("{");

        using (writer.IndentScope())
        {
            var memberInfo = ResolveClassMemberInfo(classScope, analysis);
            var needsGlobal = memberInfo.HasGlobalDeclarations &&
                scriptKind is BlLingoScriptKind.Parent or BlLingoScriptKind.Movie;

            WritePropertyDeclarations(writer, memberInfo.Properties);
            if (memberInfo.Properties.Count > 0)
            {
                writer.WriteLine();
            }

            if (needsGlobal)
            {
                writer.WriteLine("private readonly GlobalVars _global;");
                writer.WriteLine();
            }

            WriteConstructor(writer, className, needsGlobal);

            if (interfaces.Contains("IBlingoPropertyDescriptionList"))
            {
                WritePropertyDescriptionListStubs(writer);
            }

            WriteHandlers(writer, classScope, handlerConverter, memberInfo.HandlerOrder);
        }

        writer.WriteLine("}");

        return writer.ToString();
    }

    private static BlLingoClassSymbolTable ResolveClassScope(string scriptName, BlLingoAnalysisResult analysis)
    {
        if (!string.IsNullOrWhiteSpace(scriptName) && analysis.Symbols.ClassScopes.TryGetValue(scriptName, out var explicitClass))
        {
            return explicitClass;
        }

        return analysis.Symbols.MovieScript;
    }

    private static BlLegacyClassMemberInfo ResolveClassMemberInfo(
        BlLingoClassSymbolTable classScope,
        BlLingoAnalysisResult analysis)
    {
        if (analysis.TryGetData<IReadOnlyDictionary<BlLingoClassSymbolTable, BlLegacyClassMemberInfo>>(BlLegacyClassMemberPass.ClassMemberInfoKey, out var map) &&
            map is not null &&
            classScope is not null &&
            map.TryGetValue(classScope, out var info) &&
            info is not null)
        {
            return info;
        }

        return BlLegacyClassMemberInfo.Empty;
    }

    private static BlLingoScriptKind DetermineScriptKind(BlLingoScriptKind declaredKind, BlLingoClassSymbolTable classScope)
    {
        if (classScope is null)
        {
            return declaredKind == BlLingoScriptKind.Unknown ? BlLingoScriptKind.Behavior : declaredKind;
        }

        var kind = declaredKind != BlLingoScriptKind.Unknown ? declaredKind : classScope.ScriptKind;
        if (kind == BlLingoScriptKind.Unknown)
        {
            kind = classScope.IsMovieScript ? BlLingoScriptKind.Movie : BlLingoScriptKind.Behavior;
        }

        if (classScope.Handlers.ContainsKey("getPropertyDescriptionList"))
        {
            return BlLingoScriptKind.Behavior;
        }

        return kind;
    }

    private static List<string> CollectInterfaces(BlLingoClassSymbolTable classScope)
    {
        var result = new List<string>();
        if (classScope is null)
        {
            return result;
        }

        var seen = new HashSet<string>(StringComparer.Ordinal);

        if (classScope.Handlers.ContainsKey("getPropertyDescriptionList") && seen.Add("IBlingoPropertyDescriptionList"))
        {
            result.Add("IBlingoPropertyDescriptionList");
        }

        foreach (var (handler, iface) in s_eventInterfaces)
        {
            if (classScope.Handlers.ContainsKey(handler) && seen.Add(iface))
            {
                result.Add(iface);
            }
        }

        return result;
    }

    private static string GetBaseType(BlLingoScriptKind kind)
    {
        return kind switch
        {
            BlLingoScriptKind.Movie => "BlingoMovieScript",
            BlLingoScriptKind.Parent => "BlingoParentScript",
            BlLingoScriptKind.Behavior => "BlingoSpriteBehavior",
            _ => "BlingoScriptBase",
        };
    }

    private static void WriteConstructor(BlCSharpCodeWriter writer, string className, bool needsGlobal)
    {
        writer.Write($"public {className}(IBlingoMovieEnvironment env");
        if (needsGlobal)
        {
            writer.Write(", GlobalVars global");
            writer.WriteLine(") : base(env)");
            writer.WriteLine("{");
            using (writer.IndentScope())
            {
                writer.WriteLine("_global = global;");
            }

            writer.WriteLine("}");
        }
        else
        {
            writer.WriteLine(") : base(env) { }");
        }
    }

    private static void WritePropertyDeclarations(BlCSharpCodeWriter writer, IReadOnlyList<BlLegacyPropertyInfo> properties)
    {
        if (properties is null || properties.Count == 0)
        {
            return;
        }

        foreach (var property in properties)
        {
            var commentSuffix = string.IsNullOrWhiteSpace(property.Comment) ? string.Empty : " // " + property.Comment;
            writer.WriteLine($"public {property.Type} {property.Name} {{ get; set; }}{commentSuffix}");
        }
    }

    private static void WritePropertyDescriptionListStubs(BlCSharpCodeWriter writer)
    {
        writer.WriteLine();
        writer.WriteLine("public BehaviorPropertyDescriptionList? GetPropertyDescriptionList()");
        writer.WriteLine("{");
        using (writer.IndentScope())
        {
            writer.WriteLine("return null;");
        }

        writer.WriteLine("}");
        writer.WriteLine();
        writer.WriteLine("public string? GetBehaviorDescription() => null;");
        writer.WriteLine();
        writer.WriteLine("public string? GetBehaviorTooltip() => null;");
        writer.WriteLine();
        writer.WriteLine("public bool IsOKToAttach(BlingoSymbol spriteType, int spriteNum) => true;");
    }

    private static void WriteHandlers(
        BlCSharpCodeWriter writer,
        BlLingoClassSymbolTable classScope,
        BlLegacyHandlerConverter handlerConverter,
        IReadOnlyList<BlLingoHandlerSymbolTable> orderedHandlers)
    {
        if (classScope is null || classScope.Handlers.Count == 0)
        {
            return;
        }

        var classHandlers = new HashSet<BlLingoHandlerSymbolTable>();
        foreach (var handler in classScope.Handlers.Values)
        {
            if (handler is not null)
            {
                classHandlers.Add(handler);
            }
        }

        var handlers = new List<BlLingoHandlerSymbolTable>();
        var seen = new HashSet<BlLingoHandlerSymbolTable>();

        if (orderedHandlers is not null)
        {
            foreach (var handler in orderedHandlers)
            {
                if (handler is null || !classHandlers.Contains(handler) || BlLegacyHandlerFilters.ShouldSkipHandler(handler) || !seen.Add(handler))
                {
                    continue;
                }

                handlers.Add(handler);
            }
        }

        foreach (var handler in classScope.Handlers.Values)
        {
            if (handler is null || BlLegacyHandlerFilters.ShouldSkipHandler(handler) || !seen.Add(handler))
            {
                continue;
            }

            handlers.Add(handler);
        }

        if (handlers.Count == 0)
        {
            return;
        }

        var resolvedReturnTypes = new Dictionary<BlLingoHandlerSymbolTable, string>();
        foreach (var handler in handlers)
        {
            var type = DetermineHandlerReturnType(classScope, handler, handlerConverter);
            resolvedReturnTypes[handler] = type;
        }

        var first = true;
        foreach (var handler in handlers)
        {
            if (!first)
            {
                writer.WriteLine();
            }

            first = false;
            var returnType = resolvedReturnTypes.TryGetValue(handler, out var resolved) ? resolved : "void";
            WriteHandler(writer, classScope, handler, handlerConverter, returnType);
        }
    }

    private static void WriteHandler(
        BlCSharpCodeWriter writer,
        BlLingoClassSymbolTable classScope,
        BlLingoHandlerSymbolTable handler,
        BlLegacyHandlerConverter handlerConverter,
        string returnType)
    {
        var methodName = BlCSharpName.SanitizeIdentifier(handler.Symbol.Name);
        if (string.IsNullOrEmpty(methodName))
        {
            methodName = "Handler";
        }

        writer.Write($"public {returnType} {methodName}(");
        var parameters = ComposeHandlerParameters(handler);
        writer.WriteSeparated(parameters, static (w, parameter) => w.Write(parameter));
        writer.WriteLine(")");
        writer.WriteLine("{");
        writer.Indent();
        handlerConverter.WriteHandlerBody(writer, handler);
        writer.Unindent();
        writer.WriteLine("}");
    }

    private static string DetermineHandlerReturnType(
        BlLingoClassSymbolTable classScope,
        BlLingoHandlerSymbolTable handler,
        BlLegacyHandlerConverter handlerConverter)
    {
        var directInference = handlerConverter.InferReturnType(handler);
        if (!string.IsNullOrWhiteSpace(directInference))
        {
            return directInference!;
        }

        var inferred = handlerConverter.GetReturnType(handler);
        if (!string.IsNullOrWhiteSpace(inferred))
        {
            return inferred!;
        }

        var handlerName = handler?.Symbol?.Name;
        if (string.IsNullOrEmpty(handlerName))
        {
            handlerName = handler?.OriginalName;
        }

        var scriptName = classScope?.Symbol?.Name;
        var scriptKind = classScope?.ScriptKind ?? BlLingoScriptKind.Unknown;
        var resolved = BlLegacyHandlerReturnTypeRegistry.Resolve(scriptKind, scriptName, handlerName ?? string.Empty);
        if (!string.IsNullOrWhiteSpace(resolved))
        {
            return resolved!;
        }

        if (handler is not null && handlerConverter.HasReturnValue(handler))
        {
            return "object?";
        }

        return "void";
    }

    private static List<string> ComposeHandlerParameters(BlLingoHandlerSymbolTable handler)
    {
        var parameters = new List<string>();
        var usedNames = new HashSet<string>(StringComparer.Ordinal);

        foreach (var symbol in handler.Parameters.Values)
        {
            var originalName = symbol.Name;
            if (string.Equals(originalName, "me", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var sanitized = BlCSharpName.SanitizeIdentifier(originalName);
            if (string.IsNullOrEmpty(sanitized))
            {
                sanitized = "arg";
            }

            var candidate = sanitized;
            var suffix = 1;
            while (!usedNames.Add(candidate))
            {
                candidate = sanitized + suffix.ToString();
                suffix++;
            }

            var typeName = ResolveParameterType(symbol);
            parameters.Add($"{typeName} {candidate}");
        }

        return parameters;
    }

    private static string ResolveParameterType(BlCodeSymbol symbol)
    {
        if (symbol is null)
        {
            return "object";
        }

        var resolved = symbol.ResolvedTypeName;
        if (string.IsNullOrWhiteSpace(resolved))
        {
            return "object";
        }

        var normalized = NormalizeParameterType(resolved);
        return string.IsNullOrEmpty(normalized) ? "object" : normalized;
    }

    private static string NormalizeParameterType(string typeName)
    {
        var normalized = typeName.Trim();
        if (string.Equals(normalized, "object?", StringComparison.Ordinal))
        {
            return "object";
        }

        return normalized;
    }

}
