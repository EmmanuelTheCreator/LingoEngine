using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using BlingoEngine.Lingo.Core;
using OldBlingoEngine.Lingo.Core.Tokenizer;

namespace OldBlingoEngine.Lingo.Core;

/// <summary>
/// Converts Lingo source into C# following the mapping rules documented at
/// https://github.com/EmmanuelTheCreator/BlingoEngine/blob/main/docs/design/Blingo_vs_CSharp.md.
/// </summary>
public class BlingoToCSharpConverter
{
    private readonly BlingoToCSharpConverterSettings _settings;

    public List<ErrorDto> Errors { get; } = new();

    public BlingoToCSharpConverter() : this(new BlingoToCSharpConverterSettings()) { }

    public BlingoToCSharpConverter(BlingoToCSharpConverterSettings settings)
    {
        _settings = settings;
    }

    private static string JoinContinuationLines(string source)
    {
        var lines = source.Split('\n');
        if (lines.Length == 0) return source;

        var sb = new StringBuilder();
        var previousContinues = false;

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var trimmedEnd = line.TrimEnd();

            if (previousContinues)
                sb.Append(trimmedEnd.TrimStart());
            else
            {
                if (i > 0)
                    sb.Append('\n');
                sb.Append(trimmedEnd);
            }

            if (trimmedEnd.EndsWith("\\"))
            {
                sb.Length--; // remove trailing backslash
                previousContinues = true;
            }
            else
            {
                previousContinues = false;
            }
        }

        return sb.ToString();
    }

    public string Convert(string blingoSource, ConversionOptions? options = null)
    {
        Errors.Clear();
        options ??= new ConversionOptions();
        blingoSource = blingoSource.Replace("\r", "\n");
        blingoSource = JoinContinuationLines(blingoSource);
        var trimmed = blingoSource.Trim();

        var match = Regex.Match(
            trimmed,
            @"^member\s*\(\s*""(?<name>[^""]+)""\s*\)\.(?<prop>text|line|word|char)$",
            RegexOptions.IgnoreCase);
        if (match.Success)
        {
            var prop = match.Groups["prop"].Value;
            var pascal = char.ToUpperInvariant(prop[0]) + prop[1..];
            return $"GetMember<IBlingoMemberTextBase>(\"{match.Groups["name"].Value}\").{pascal}";
        }

        match = Regex.Match(
            trimmed,
            @"^castLib\s*\(\s*(?<arg>[^\)]+)\s*\)\.save\s*\(\s*\)\s*$",
            RegexOptions.IgnoreCase);
        if (match.Success)
        {
            return $"CastLib({match.Groups["arg"].Value}).Save();";
        }

        if (TryConvertNewMember(trimmed, out var converted))
            return converted;

        var parser = new BlingoAstParser();
        try
        {
            var ast = parser.Parse(blingoSource);
            return CSharpWriter.Write(ast, options.MethodAccessModifier, settings: _settings);
        }
        catch (Exception ex)
        {
            LogError(null, blingoSource, ex);
            throw;
        }
    }

    public string Convert(BlingoScriptFile script, ConversionOptions? options = null)
    {
        Convert(new[] { script }, options);
        return script.CSharp;
    }

    public BlingoBatchResult Convert(IEnumerable<BlingoScriptFile> scripts, ConversionOptions? options = null)
    {
        Errors.Clear();
        options ??= new ConversionOptions();
        var result = new BlingoBatchResult();
        var scriptList = scripts.ToList();
        var asts = new Dictionary<string, BlingoNode>();
        var methodMap = new Dictionary<string, string>();
        var propInfo = new Dictionary<string, List<(string Name, string Type, string? Default)>>();
        var newHandlers = new Dictionary<string, BlingoHandlerNode?>();
        var sourceMap = new Dictionary<string, string>();
        foreach (var s in scriptList)
        {
            var src = s.Source.Replace("\r", "\n");
            src = JoinContinuationLines(src);
            sourceMap[s.Name] = src;
            var st = s.Detection switch
            {
                ScriptDetectionType.Auto => DetectScriptType(src),
                ScriptDetectionType.Behavior => BlingoScriptType.Behavior,
                ScriptDetectionType.Parent => BlingoScriptType.Parent,
                ScriptDetectionType.Movie => BlingoScriptType.Movie,
                _ => BlingoScriptType.Behavior
            };
            s.Type = st;
        }
        var typeMap = new Dictionary<string, BlingoScriptType>(StringComparer.OrdinalIgnoreCase);
        foreach (var s in scriptList)
        {
            typeMap[s.Name] = s.Type;
            typeMap[CSharpName.SanitizeIdentifier(s.Name)] = s.Type;
            typeMap[CSharpName.NormalizeScriptName(s.Name)] = s.Type;
        }

        // First pass: parse scripts, gather handler signatures and property names
        foreach (var file in scriptList)
        {
            var fileItem = file;

            try
            {
                var parser = new BlingoAstParser();
                try
                {
                    var source = sourceMap[file.Name];
                    var ast = parser.Parse(source);
                    if (ast is BlingoBlockNode block)
                    {
                        while (block.Children.Count > 0 && block.Children[0] is BlingoCommentNode)
                            block.Children.RemoveAt(0);
                        var nh = block.Children.OfType<BlingoHandlerNode>()
                            .FirstOrDefault(h => h.Handler != null && h.Handler.Name.Equals("new", StringComparison.OrdinalIgnoreCase));
                        newHandlers[file.Name] = nh;
                        if (nh != null)
                            block.Children.Remove(nh);
                    }
                    asts[file.Name] = ast;
                }
                catch (Exception ex)
                {
                    LogError(file.Name, sourceMap[file.Name], ex);
                    throw;
                }

                var signatures = ExtractHandlerSignatures(sourceMap[file.Name]);
                signatures.Remove("new");
                var methodSigs = new List<MethodSignature>();
                foreach (var kv in signatures)
                {
                    var paramInfos = kv.Value.Select(p => new ParameterInfo(p, "object")).ToList();
                    methodSigs.Add(new MethodSignature(kv.Key, paramInfos));
                    if (!DefaultMethods.Contains(kv.Key) && !methodMap.ContainsKey(kv.Key))
                    {
                        var className = CSharpName.ComposeName(file.Name, file.Type, _settings);
                        methodMap[kv.Key] = className;
                        result.CustomMethods.Add(kv.Key);
                    }
                }
                result.Methods[file.Name] = methodSigs;

                var propDecls = ExtractPropertyDeclarations(sourceMap[file.Name]);
                var propDescs = ExtractPropertyDescriptions(sourceMap[file.Name]);
                var fieldMap = new Dictionary<string, (string Type, string? Default)>(StringComparer.OrdinalIgnoreCase);
                foreach (var n in propDecls)
                    fieldMap[n] = ("object", null);
                foreach (var d in propDescs)
                    fieldMap[d.Name] = (FormatToCSharpType(d.Format), d.Default);

                propInfo[file.Name] = fieldMap.Select(kv => (kv.Key, kv.Value.Type, kv.Value.Default)).ToList();
            }
            catch (Exception ex)
            {
                fileItem.Errors += Environment.NewLine + ex.Message;
            }
        }

        // Second pass: infer property and parameter types
        foreach (var file in asts.Keys.ToList())
        {
            var fileItem = scripts.First(x => x.Name == file);

            try
            {
                var fields = propInfo.TryGetValue(file, out var list) ? list.ToDictionary(x => x.Name, x => (x.Type, x.Default), StringComparer.OrdinalIgnoreCase) : new Dictionary<string, (string Type, string? Default)>(StringComparer.OrdinalIgnoreCase);
                var source = sourceMap[file];
                var inferredProps = InferPropertyTypes(source, fields.Keys);
                foreach (var kv in inferredProps)
                {
                    if (fields.TryGetValue(kv.Key, out var existing) && existing.Type == "object")
                        fields[kv.Key] = (kv.Value, existing.Default);
                }
                result.Properties[file] = fields.Select(kv => new PropertyInfo(kv.Key, kv.Value.Type)).ToList();

                var methods = result.Methods.TryGetValue(file, out var msList) ? msList : new List<MethodSignature>();
                var paramTypes = InferParameterTypes(source, methods, fields.ToDictionary(kv => kv.Key, kv => kv.Value.Type, StringComparer.OrdinalIgnoreCase));
                foreach (var ms in methods)
                {
                    if (!paramTypes.TryGetValue(ms.Name, out var map)) continue;
                    for (int i = 0; i < ms.Parameters.Count; i++)
                    {
                        var p = ms.Parameters[i];
                        if (map.TryGetValue(p.Name, out var type) && p.Type == "object")
                            ms.Parameters[i] = p with { Type = type };
                    }
                }
            }
            catch (Exception ex)
            {
                fileItem.Errors += Environment.NewLine + ex.Message;
            }
        }

        // Link SendSprite methods
        var generatedBehaviors = new Dictionary<string, string>();
        var annotator = new SendSpriteTypeResolver(methodMap, generatedBehaviors, _settings);
        foreach (var ast in asts)
        {
            var fileItem = scripts.First(x => x.Name == ast.Key);

            try
            {
                ast.Value.Accept(annotator);
            }
            catch (Exception ex)
            {
                fileItem.Errors += Environment.NewLine + ex.Message;
            }
        }


        // Generate final class code inserting methods
        foreach (var script in scriptList)
        {
            try
            {
                newHandlers.TryGetValue(script.Name, out var nh);
                var classCode = ConvertClass(script, nh, typeMap, includePropertyDescriptionStub: false);
                var sigMap = result.Methods.TryGetValue(script.Name, out var msList)
                    ? msList.ToDictionary(m => m.Name, m => m, StringComparer.OrdinalIgnoreCase)
                    : new Dictionary<string, MethodSignature>(StringComparer.OrdinalIgnoreCase);
                var methods = CSharpWriter.Write(asts[script.Name], options.MethodAccessModifier, typeMap, sigMap, _settings);
                var insertIdx = classCode.LastIndexOf('}');
                if (insertIdx >= 0)
                    classCode = classCode[..insertIdx] + methods + classCode[insertIdx..];
                else
                    classCode += methods;

                var ns = BuildNamespace(script, options);
                var sb = new StringBuilder();
                sb.AppendLine("using System;");
                sb.AppendLine("using BlingoEngine.Lingo.Core;");
                sb.AppendLine();
                if (!string.IsNullOrWhiteSpace(ns))
                {
                    sb.Append("namespace ").Append(ns).AppendLine(";");
                    sb.AppendLine();
                }
                sb.Append(classCode);
                var finalCode = sb.ToString();
                result.ConvertedScripts[script.Name] = finalCode;
                script.CSharp = finalCode;
            }
            catch (Exception ex)
            {
                script.Errors += Environment.NewLine + ex.Message;
            }
            script.Errors += GetCurrentErrorsAndFlush();
        }

        foreach (var kvp in generatedBehaviors)
        {
            var fileItem = scripts.FirstOrDefault(x => x.Name == kvp.Value);

            try
            {
                var safeName = CSharpName.SanitizeIdentifier(kvp.Value);
                var scriptBody = GenerateSendSpriteBehaviorClass(safeName, kvp.Key);
                var ns = BuildNamespace(fileItem ?? new BlingoScriptFile(kvp.Value, string.Empty), options);
                var sb = new StringBuilder();
                sb.AppendLine("using System;");
                sb.AppendLine("using BlingoEngine.Lingo.Core;");
                sb.AppendLine();
                if (!string.IsNullOrWhiteSpace(ns))
                {
                    sb.Append("namespace ").Append(ns).AppendLine(";");
                    sb.AppendLine();
                }
                sb.Append(scriptBody);
                var script = sb.ToString();
                result.ConvertedScripts[kvp.Value] = script;
                if (fileItem != null)
                    fileItem.CSharp = script;
            }
            catch (Exception ex)
            {
                if (fileItem != null)
                    fileItem.Errors += Environment.NewLine + ex.Message;
            }
            if (fileItem != null)
                fileItem.Errors += GetCurrentErrorsAndFlush();
        }

        return result;
    }
    public string GetCurrentErrorsAndFlush()
    {
        var errors = string.Join("\n", Errors.Select(e =>
           string.IsNullOrEmpty(e.File)
               ? $"Line {e.LineNumber}: {e.LineText} - {e.Error}"
               : $"{e.File}:{e.LineNumber}: {e.LineText} - {e.Error}"));
        Errors.Clear();
        return errors;
    }
    /// <summary>
    /// Generates a minimal C# class wrapper for a single Lingo script.
    /// Only the class declaration and constructor are emitted.
    /// The class name is derived from the file name plus the script type suffix.
    /// </summary>
    public string ConvertClass(BlingoScriptFile script)
    {
        BlingoHandlerNode? newHandler = null;
        try
        {
            var source = script.Source.Replace("\r", "\n");
            var parser = new BlingoAstParser();
            var ast = parser.Parse(source);
            if (ast is BlingoBlockNode block)
                newHandler = block.Children.OfType<BlingoHandlerNode>()
                    .FirstOrDefault(h => h.Handler != null && h.Handler.Name.Equals("new", StringComparison.OrdinalIgnoreCase));
            var normalized = new BlingoScriptFile(script.Name, source, script.Type);
            return ConvertClass(normalized, newHandler);
        }
        catch
        {
            // ignore parse errors here; they will surface later if needed
        }

        return ConvertClass(script, newHandler);
    }

    public string ConvertClass(
        BlingoScriptFile script,
        BlingoHandlerNode? newHandler,
        IReadOnlyDictionary<string, BlingoScriptType>? scriptTypes = null,
        bool includePropertyDescriptionStub = true)
    {
        var source = script.Source.Replace("\r", "\n");
        var handlers = ExtractHandlerNames(source);
        var scriptType = handlers.Contains("getPropertyDescriptionList") ? BlingoScriptType.Behavior : script.Type;

        var baseType = scriptType switch
        {
            BlingoScriptType.Movie => "BlingoMovieScript",
            BlingoScriptType.Parent => "BlingoParentScript",
            BlingoScriptType.Behavior => "BlingoSpriteBehavior",
            _ => "BlingoScriptBase"
        };

        var className = CSharpName.ComposeName(script.Name, scriptType, _settings);
        bool hasPropDescHandler = handlers.Contains("getPropertyDescriptionList");
        var propDescs = ExtractPropertyDescriptions(source);
        var propDecls = ExtractPropertyDeclarations(source);

        var leadingComments = ExtractLeadingComments(source);

        var sb = new StringBuilder();
        if (leadingComments.Count > 0)
        {
            foreach (var comment in leadingComments)
            {
                if (comment.Length == 0)
                    sb.AppendLine();
                else
                    sb.AppendLine($"// {comment}");
            }
            sb.AppendLine();
        }
        var interfaces = new List<string>();
        if (hasPropDescHandler)
            interfaces.Add("IBlingoPropertyDescriptionList");

        var eventInterfaces = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["blur"] = "IHasBlurEvent",
            ["focus"] = "IHasFocusEvent",
            ["keydown"] = "IHasKeyDownEvent",
            ["keyup"] = "IHasKeyUpEvent",
            ["mousewithin"] = "IHasMouseWithinEvent",
            ["mouseleave"] = "IHasMouseLeaveEvent",
            ["mousedown"] = "IHasMouseDownEvent",
            ["mouseup"] = "IHasMouseUpEvent",
            ["mousemove"] = "IHasMouseMoveEvent",
            ["mousewheel"] = "IHasMouseWheelEvent",
            ["mouseenter"] = "IHasMouseEnterEvent",
            ["mouseexit"] = "IHasMouseExitEvent",
            ["beginsprite"] = "IHasBeginSpriteEvent",
            ["endsprite"] = "IHasEndSpriteEvent",
            ["stepframe"] = "IHasStepFrameEvent",
            ["prepareframe"] = "IHasPrepareFrameEvent",
            ["enterframe"] = "IHasEnterFrameEvent",
            ["exitframe"] = "IHasExitFrameEvent"
        };

        foreach (var kv in eventInterfaces)
        {
            if (handlers.Contains(kv.Key))
                interfaces.Add(kv.Value);
        }

        var interfaceSuffix = interfaces.Count > 0 ? ", " + string.Join(", ", interfaces) : string.Empty;

        sb.AppendLine($"public class {className} : {baseType}{interfaceSuffix}");
        sb.AppendLine("{");

        var fieldMap = new Dictionary<string, (string Type, string? Default)>(StringComparer.OrdinalIgnoreCase);
        foreach (var n in propDecls)
            fieldMap[n] = ("object", null);
        foreach (var d in propDescs)
            fieldMap[d.Name] = (FormatToCSharpType(d.Format), d.Default);

        var inferredTypes = InferPropertyTypes(source, fieldMap.Keys);
        foreach (var kv in inferredTypes)
        {
            if (fieldMap.TryGetValue(kv.Key, out var existing) && existing.Type == "object")
                fieldMap[kv.Key] = (kv.Value, existing.Default);
        }
        var fieldTypes = fieldMap.ToDictionary(kv => kv.Key, kv => kv.Value.Type, StringComparer.OrdinalIgnoreCase);
        if (fieldMap.Count > 0)
        {
            foreach (var kv in fieldMap)
            {
                var line = $"    public {kv.Value.Type} {kv.Key} {{ get; set; }}";
                if (kv.Value.Default != null)
                    line += $" = {kv.Value.Default};";
                else if (kv.Value.Type.StartsWith("BlingoList<", StringComparison.Ordinal) ||
                         kv.Value.Type.StartsWith("BlingoPropertyList<", StringComparison.Ordinal))
                    line += " = new();";
                sb.AppendLine(line);
            }
            sb.AppendLine();
        }

        bool needsGlobal = scriptType == BlingoScriptType.Movie || scriptType == BlingoScriptType.Parent;
        if (needsGlobal)
        {
            sb.AppendLine("    private readonly GlobalVars _global;");
            sb.AppendLine();
        }

        sb.Append($"    public {className}(IBlingoMovieEnvironment env");
        if (needsGlobal)
            sb.Append(", GlobalVars global");
        Dictionary<string, string> ctorParamTypes = new(StringComparer.OrdinalIgnoreCase);
        if (newHandler != null)
        {
            var argNames = newHandler.Handler.ArgumentNames.Where(a => !a.Equals("me", StringComparison.OrdinalIgnoreCase)).ToList();
            if (argNames.Count > 0)
            {
                var ms = new MethodSignature("new", argNames.Select(a => new ParameterInfo(a, "object")).ToList());
                var map = InferParameterTypes(source, new[] { ms }, fieldTypes);
                if (map.TryGetValue("new", out var m))
                    ctorParamTypes = m;
            }
            foreach (var arg in argNames)
            {
                var type = ctorParamTypes.TryGetValue(arg, out var t) ? t : "object";
                sb.Append($", {type} {arg}");
            }
        }
        sb.Append(") : base(env)");

        if (!needsGlobal && newHandler == null)
        {
            sb.AppendLine(" { }");
        }
        else
        {
            sb.AppendLine();
            sb.AppendLine("    {");
            if (needsGlobal)
                sb.AppendLine("        _global = global;");
            if (newHandler != null)
            {
                var methodCode = CSharpWriter.Write(newHandler, "public", scriptTypes, settings: _settings);
                var bodyStart = methodCode.IndexOf('{');
                var bodyEnd = methodCode.LastIndexOf('}');
                var body = bodyStart >= 0 && bodyEnd > bodyStart
                    ? methodCode.Substring(bodyStart + 1, bodyEnd - bodyStart - 1)
                    : string.Empty;
                foreach (var line in body.Split('\n', StringSplitOptions.RemoveEmptyEntries))
                    sb.AppendLine("        " + line.Trim());
            }
            sb.AppendLine("    }");
        }

        if (hasPropDescHandler && includePropertyDescriptionStub)
        {
            var props = propDescs;
            sb.AppendLine();
            sb.AppendLine("    public BehaviorPropertyDescriptionList? GetPropertyDescriptionList()");
            sb.AppendLine("    {");
            sb.AppendLine("        return new BehaviorPropertyDescriptionList()");
            for (int i = 0; i < props.Count; i++)
            {
                var p = props[i];
                sb.AppendLine($"            .Add(this, x => x.{p.Name}, \"{p.Comment}\", {p.Default})");
            }
            sb.AppendLine("        ;");
            sb.AppendLine("    }");
        }

        sb.AppendLine("}");
        return sb.ToString();
    }

    private static string GenerateSendSpriteBehaviorClass(string className, string method)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"public class {className} : BlingoSpriteBehavior");
        sb.AppendLine("{");
        sb.AppendLine($"    public {className}(IBlingoMovieEnvironment env) : base(env) {{ }}");
        sb.AppendLine($"    public object? {method}(params object?[] args) => null;");
        sb.AppendLine("}");
        return sb.ToString();
    }

    private static string BuildNamespace(BlingoScriptFile script, ConversionOptions options)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(options.Namespace))
            parts.Add(options.Namespace);

        if (!string.IsNullOrWhiteSpace(script.RelativeDirectory))
        {
            var dirs = script.RelativeDirectory.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var d in dirs)
            {
                var seg = CSharpName.SanitizeIdentifier(d);
                if (seg.Length > 0)
                    parts.Add(char.ToUpperInvariant(seg[0]) + seg[1..]);
            }
        }

        return string.Join('.', parts);
    }

    private static Dictionary<string, List<string>> ExtractHandlerSignatures(string source)
    {
        var dict = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        var regex = new Regex(@"(?im)^\s*on\s+(?<name>\w+)(?<params>[^\r\n]*)");
        foreach (Match m in regex.Matches(source))
        {
            var name = m.Groups["name"].Value;
            var paramStr = m.Groups["params"].Value.Trim();
            var list = new List<string>();
            if (!string.IsNullOrEmpty(paramStr))
            {
                var parts = paramStr.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var p in parts)
                    list.Add(p.Trim());
            }
            dict[name] = list;
        }
        return dict;
    }

    private static HashSet<string> ExtractHandlerNames(string source)
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var regex = new Regex(@"(?im)^\s*on\s+(\w+)");
        foreach (Match m in regex.Matches(source))
            set.Add(m.Groups[1].Value);
        return set;
    }

    private static List<(string Name, string Default, string Comment, string Format)> ExtractPropertyDescriptions(string source)
    {
        var builder = new OrderedPropertyListBuilder<string, (string Name, string Default, string Comment, string Format)>(StringComparer.OrdinalIgnoreCase);
        var regex = new Regex(@"addProp\s+description,#(?<name>\w+),\s*\[(?<body>[^\]]*)\]", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        foreach (Match m in regex.Matches(source))
        {
            var name = m.Groups["name"].Value;
            var body = m.Groups["body"].Value;
            var defMatch = Regex.Match(body, @"#default:(?<val>[^#\]]+)");
            var fmtMatch = Regex.Match(body, @"#format:#(?<fmt>\w+)");
            var commentMatch = Regex.Match(body, @"#comment:""(.*?)""", RegexOptions.Singleline);
            var def = defMatch.Success ? defMatch.Groups["val"].Value.Trim().TrimEnd(',') : "0";
            var fmt = fmtMatch.Success ? fmtMatch.Groups["fmt"].Value.Trim().ToLowerInvariant() : string.Empty;
            if (Regex.IsMatch(def, @"(?i)rgb\s*\((.+)\)"))
                def = Regex.Replace(def, @"(?i)rgb\s*\((.+)\)", "AColor.FromCode($1)");
            if (fmt == "string" || fmt == "symbol")
                def = $"\"{Escape(def.Trim('"'))}\"";
            else if (fmt == "color" && Regex.IsMatch(def, @"^\d+$"))
                def = $"AColor.FromCode({def})";
            var comment = commentMatch.Success ? Escape(commentMatch.Groups[1].Value) : string.Empty;
            var entry = (name, def, comment, fmt);
            builder.AddOrUpdate(name, entry);
        }
        return builder.ToList();
    }

    private static List<string> ExtractLeadingComments(string source)
    {
        var result = new List<string>();
        if (string.IsNullOrEmpty(source))
            return result;

        var lines = source.Split('\n');
        var seenComment = false;
        foreach (var rawLine in lines)
        {
            var line = rawLine.TrimEnd('\r');
            var trimmed = line.Trim();

            if (!seenComment && trimmed.Length == 0)
                continue;

            if (trimmed.StartsWith("--"))
            {
                var comment = trimmed.Length > 2 ? trimmed[2..].TrimStart() : string.Empty;
                result.Add(comment);
                seenComment = true;
                continue;
            }

            if (seenComment && trimmed.Length == 0)
            {
                result.Add(string.Empty);
                continue;
            }

            break;
        }

        while (result.Count > 0 && result[^1].Length == 0)
            result.RemoveAt(result.Count - 1);

        return result;
    }

    private static List<string> ExtractPropertyDeclarations(string source)
    {
        var list = new List<string>();
        try
        {
            var parser = new BlingoAstParser();
            var ast = parser.Parse(source);
            if (ast is BlingoBlockNode block)
            {
                foreach (var child in block.Children)
                {
                    if (child is BlingoPropertyDeclStmtNode prop)
                        list.AddRange(prop.Names);
                }
            }
        }
        catch
        {
            // ignore parse errors and return whatever was collected so far
        }

        if (list.Count == 0)
        {
            var regex = new Regex(@"(?im)^\s*property\s+(.+)$");
            foreach (Match m in regex.Matches(source))
            {
                var names = m.Groups[1].Value;
                foreach (var n in names.Split(new[] { ',', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries))
                    list.Add(n.Trim());
            }
        }
        return list;
    }

    private static Dictionary<string, string> InferPropertyTypes(string source, IEnumerable<string> propertyNames)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var targets = new HashSet<string>(propertyNames, StringComparer.OrdinalIgnoreCase);
        var lines = source.Split('\n');
        var locals = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            var anyAssign = Regex.Match(trimmed, @"(?i)^(\w+)\\s*=\\s*(.+)$");
            if (anyAssign.Success)
            {
                var lhs = anyAssign.Groups[1].Value;
                var rhsAll = anyAssign.Groups[2].Value.Trim();
                var t = InferTypeFromExpression(rhsAll);
                if (!targets.Contains(lhs))
                    locals[lhs] = t;
            }

            foreach (var name in targets)
            {
                var existingType = result.TryGetValue(name, out var ex) ? ex : null;
                if (existingType != null && existingType != "object") continue;
                var m = Regex.Match(trimmed, $"(?i)^{Regex.Escape(name)}\\s*=\\s*(.+)$");
                if (m.Success)
                {
                    var rhs = Regex.Replace(m.Groups[1].Value, @"--.*$", string.Empty).Trim();
                    var type = InferTypeFromExpression(rhs);
                    if (type == "object")
                    {
                        var idx = Regex.Match(rhs, @"^(\w+)\[");
                        if (idx.Success)
                        {
                            var srcName = idx.Groups[1].Value;
                            if (result.TryGetValue(srcName, out var srcType) && srcType.StartsWith("BlingoList<"))
                            {
                                var elemType = Regex.Match(srcType, @"<(.*)>").Groups[1].Value;
                                type = elemType;
                            }
                            else if (locals.TryGetValue(srcName, out var localType) && localType.StartsWith("BlingoList<"))
                            {
                                var elemType = Regex.Match(localType, @"<(.*)>").Groups[1].Value;
                                type = elemType;
                            }
                        }
                    }
                    result[name] = type;
                    continue;
                }

                var listOp = Regex.Match(trimmed,
                    $"(?i)^(append|add|addat|setat|setprop|setaprop|addprop)\\s+{Regex.Escape(name)}\\s*,\\s*(?:[^,]+,\\s*)?(.+)$");
                if (listOp.Success)
                {
                    var rhs = Regex.Replace(listOp.Groups[2].Value, @"--.*$", string.Empty).Trim();
                    var elemType = InferTypeFromExpression(rhs);
                    var opName = listOp.Groups[1].Value.ToLowerInvariant();
                    if (opName.Contains("prop"))
                        result[name] = $"BlingoPropertyList<{elemType}>";
                    else
                        result[name] = $"BlingoList<{elemType}>";
                    continue;
                }

                var propOps = $"(?i)^(deleteprop|getprop|getaprop|getpropat|findpos|findposnear|setaprop)\\s+{Regex.Escape(name)}\\b";
                var listOps = $"(?i)^(deleteat|deleteone|getat|getpos|count|max|min)\\s+{Regex.Escape(name)}\\b";
                if (!result.ContainsKey(name))
                {
                    if (Regex.IsMatch(trimmed, propOps))
                        result[name] = "BlingoPropertyList<object>";
                    else if (Regex.IsMatch(trimmed, listOps))
                        result[name] = "BlingoList<object>";
                }

                if (!result.ContainsKey(name))
                {
                    var listElem = Regex.Match(trimmed, $"(?i)^{Regex.Escape(name)}\\s*=\\s*(\\w+)\\[");
                    if (listElem.Success)
                    {
                        var src = listElem.Groups[1].Value;
                        if (result.TryGetValue(src, out var srcType) && srcType.StartsWith("BlingoList<"))
                        {
                            var elemType = Regex.Match(srcType, @"<(.*)>").Groups[1].Value;
                            result[name] = elemType;
                            continue;
                        }
                    }
                    var direct = Regex.Match(trimmed, $"(?i)^{Regex.Escape(name)}\\s*=\\s*(\\w+)\\b");
                    if (direct.Success)
                    {
                        var src = direct.Groups[1].Value;
                        if (result.TryGetValue(src, out var srcType))
                        {
                            result[name] = srcType;
                            continue;
                        }
                    }
                }
            }
        }
        return result;
    }

    private static Dictionary<string, Dictionary<string, string>> InferParameterTypes(
        string source,
        IEnumerable<MethodSignature> methods,
        IDictionary<string, string> propertyTypes)
    {
        var result = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        var methodLookup = methods.ToDictionary(m => m.Name, m => m, StringComparer.OrdinalIgnoreCase);
        var lines = source.Split('\n');
        string? current = null;
        foreach (var raw in lines)
        {
            var trimmed = raw.Trim();
            var onMatch = Regex.Match(trimmed, @"(?i)^on\s+(\w+)");
            if (onMatch.Success)
            {
                current = onMatch.Groups[1].Value;
                if (!result.ContainsKey(current))
                    result[current] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                continue;
            }
            if (Regex.IsMatch(trimmed, @"(?i)^end\b"))
            {
                current = null;
                continue;
            }
            if (current == null) continue;
            if (!methodLookup.TryGetValue(current, out var sig)) continue;
            foreach (var param in sig.Parameters)
            {
                if (result[current].ContainsKey(param.Name)) continue;
                var assign = Regex.Match(trimmed, @$"(?i)(?:^|then\s+|;\s*){Regex.Escape(param.Name)}\s*=\s*(.+)$");
                if (assign.Success)
                {
                    var rhsExpr = Regex.Replace(assign.Groups[1].Value, @"--.*$", string.Empty).Trim();
                    var rhsType = InferTypeFromExpression(rhsExpr);
                    if (rhsType == "object")
                    {
                        var rhsVar = Regex.Match(rhsExpr, @"^(\w+)$");
                        if (rhsVar.Success && propertyTypes.TryGetValue(rhsVar.Groups[1].Value, out var propType))
                            rhsType = propType;
                    }
                    result[current][param.Name] = rhsType;
                    continue;
                }

                var reverse = Regex.Match(trimmed, @$"(?i)(?:^|then\s+|;\s*)(\w+)\s*=\s*{Regex.Escape(param.Name)}\b");
                if (reverse.Success)
                {
                    var lhs = reverse.Groups[1].Value;
                    if (propertyTypes.TryGetValue(lhs, out var propType))
                    {
                        result[current][param.Name] = propType;
                        continue;
                    }
                }

                if (Regex.IsMatch(trimmed, $"{Regex.Escape(param.Name)}\\.text", RegexOptions.IgnoreCase))
                {
                    result[current][param.Name] = "string";
                    continue;
                }

                var concatPattern1 = "\"[^\"]*\"\\s*(?:&&|&)\\s*" + Regex.Escape(param.Name) + "\\b";
                var concatPattern2 = Regex.Escape(param.Name) + "\\s*(?:&&|&)\\s*\"";
                if (Regex.IsMatch(trimmed, concatPattern1, RegexOptions.IgnoreCase) ||
                    Regex.IsMatch(trimmed, concatPattern2, RegexOptions.IgnoreCase))
                {
                    result[current][param.Name] = "string";
                }
            }
        }
        return result;
    }

    private static string InferTypeFromExpression(string rhs)
    {
        if (Regex.IsMatch(rhs, "^\".*\"$")) return "string";
        if (rhs.Contains("member(", StringComparison.OrdinalIgnoreCase) && rhs.Contains(").text", StringComparison.OrdinalIgnoreCase)) return "string";
        if (Regex.IsMatch(rhs, @"(?i)^rgb\s*\(")) return "AColor";
        if (rhs.StartsWith("[") && rhs.EndsWith("]"))
        {
            var inner = rhs.Substring(1, rhs.Length - 2);
            var elems = inner.Split(',').Select(e => e.Trim()).Where(e => e.Length > 0).ToList();
            if (elems.Count == 0) return "BlingoList<object>";
            var elemType = InferTypeFromExpression(elems[0]);
            foreach (var e in elems.Skip(1))
            {
                var t = InferTypeFromExpression(e);
                if (t != elemType)
                {
                    elemType = "object";
                    break;
                }
            }
            return $"BlingoList<{elemType}>";
        }
        if (Regex.IsMatch(rhs, @"^(true|false)$", RegexOptions.IgnoreCase)) return "bool";
        if (Regex.IsMatch(rhs, @"^[-+]?[0-9]+$")) return "int";
        if (Regex.IsMatch(rhs, @"^[-+]?[0-9]*\\.[0-9]+$")) return "float";
        if (Regex.IsMatch(rhs, @"[-+*/]") && Regex.IsMatch(rhs, @"[0-9]"))
        {
            if (rhs.Contains('.')) return "float";
            return "int";
        }
        return "object";
    }

    private static string FormatToCSharpType(string fmt) => fmt switch
    {
        "integer" => "int",
        "float" => "float",
        "boolean" => "bool",
        "string" or "symbol" => "string",
        "color" => "AColor",
        _ => "object"
    };

    private static string Escape(string value) => value.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n");

    private static bool TryConvertNewMember(string source, out string converted)
    {
        converted = string.Empty;
        var tokenizer = new BlingoTokenizer(source);
        var tokens = new List<BlingoToken>();
        BlingoToken tok;
        do
        {
            tok = tokenizer.NextToken();
            tokens.Add(tok);
        } while (tok.Type != BlingoTokenType.Eof && tokens.Count < 32);

        int idx = 0;

        // optional assignment prefix
        string? lhs = null;
        if (tokens.Count > 2 && tokens[0].Type == BlingoTokenType.Identifier && tokens[1].Type == BlingoTokenType.Equals)
        {
            lhs = tokens[0].Lexeme;
            idx = 2;
        }

        if (idx + 4 >= tokens.Count) return false;
        if (tokens[idx].Type != BlingoTokenType.Identifier) return false;
        string obj = tokens[idx].Lexeme;
        if (tokens[idx + 1].Type != BlingoTokenType.Dot) return false;
        if (tokens[idx + 2].Type != BlingoTokenType.Identifier || !tokens[idx + 2].Lexeme.Equals("newMember", StringComparison.OrdinalIgnoreCase))
            return false;
        if (tokens[idx + 3].Type != BlingoTokenType.LeftParen) return false;

        int pos = idx + 4;
        if (pos >= tokens.Count) return false;
        if (tokens[pos].Type != BlingoTokenType.Symbol) return false;
        string type = tokens[pos].Lexeme.ToLowerInvariant();
        string method = type switch
        {
            "bitmap" => "Bitmap",
            "sound" => "Sound",
            "filmloop" => "FilmLoop",
            "text" => "Text",
            _ => "Member"
        };
        pos++;

        string rest = string.Empty;
        if (pos < tokens.Count && tokens[pos].Type == BlingoTokenType.Comma)
        {
            pos++;
            var parts = new List<string>();
            while (pos < tokens.Count && tokens[pos].Type != BlingoTokenType.RightParen)
            {
                parts.Add(tokens[pos].Lexeme);
                pos++;
            }
            rest = string.Join(" ", parts);
        }

        if (pos >= tokens.Count || tokens[pos].Type != BlingoTokenType.RightParen)
            return false;
        pos++;
        if (pos < tokens.Count && tokens[pos].Type != BlingoTokenType.Eof)
            return false;

        var call = $"{obj}.New.{method}({rest})".TrimEnd();
        converted = lhs != null ? $"{lhs} = {call};" : call;
        return true;
    }

    private void LogError(string? file, string source, Exception ex)
    {
        var lineNumber = 0;
        var lineText = string.Empty;
        try
        {
            var match = Regex.Match(ex.Message, @"line (\d+)");
            if (match.Success && int.TryParse(match.Groups[1].Value, out var line))
            {
                var lines = source.Split('\n');
                if (line - 1 >= 0 && line - 1 < lines.Length)
                    lineText = lines[line - 1];
                lineNumber = line;
            }
        }
        catch
        {
            // ignored
        }

        Errors.Add(new ErrorDto(file ?? string.Empty, lineNumber, lineText, ex.Message));
    }

    private static BlingoScriptType DetectScriptType(string source)
    {
        if (Regex.IsMatch(source, @"(?im)^\s*on\s+getPropertyDescriptionList\b"))
            return BlingoScriptType.Behavior;
        if (Regex.IsMatch(source, @"(?im)^\s*on\s+new\s+me\b"))
            return BlingoScriptType.Parent;
        if (Regex.IsMatch(source, @"(?im)^\s*on\s+(startmovie|endmovie)\b"))
            return BlingoScriptType.Movie;
        return BlingoScriptType.Behavior;
    }

    private static readonly HashSet<string> DefaultMethods = new(new[]
        {
            "StepFrame","PrepareFrame","EnterFrame","ExitFrame","BeginSprite",
            "EndSprite","MouseDown","MouseUp","MouseMove","MouseEnter",
            "MouseLeave","MouseWithin","MouseExit","PrepareMovie","StartMovie",
            "StopMovie","KeyDown","KeyUp","Focus","Blur"
        }, StringComparer.OrdinalIgnoreCase);

    private class CustomMethodCollector : IBlingoAstVisitor
    {
        public HashSet<string> Methods { get; } = new();
        public void Visit(BlingoHandlerNode n) => n.Block.Accept(this);
        public void Visit(BlingoCommentNode n) { }
        public void Visit(BlingoNewObjNode n) { n.ObjArgs.Accept(this); }
        public void Visit(BlingoLiteralNode n) { }
        public void Visit(BlingoCallNode n) { if (!string.IsNullOrEmpty(n.Name)) Methods.Add(n.Name); }
        public void Visit(BlingoObjCallNode n) { if (n.Name.Value != null) Methods.Add(n.Name.Value.AsString()); }
        public void Visit(BlingoBlockNode n) { foreach (var c in n.Children) c.Accept(this); }
        public void Visit(BlingoIfStmtNode n) { n.Condition.Accept(this); n.ThenBlock.Accept(this); if (n.HasElse) n.ElseBlock!.Accept(this); }
        public void Visit(BlingoIfElseStmtNode n) { n.Condition.Accept(this); n.ThenBlock.Accept(this); n.ElseBlock.Accept(this); }
        public void Visit(BlingoPutStmtNode n) { n.Value.Accept(this); n.Target?.Accept(this); }
        public void Visit(BlingoBinaryOpNode n) { n.Left.Accept(this); n.Right.Accept(this); }
        public void Visit(BlingoCaseStmtNode n) { n.Value.Accept(this); n.Otherwise?.Accept(this); }
        public void Visit(BlingoTheExprNode n) { }
        public void Visit(BlingoExitStmtNode n) { }
        public void Visit(BlingoReturnStmtNode n) { n.Value?.Accept(this); }
        public void Visit(BlingoTellStmtNode n) { n.Block.Accept(this); }
        public void Visit(BlingoOtherwiseNode n) { n.Block.Accept(this); }
        public void Visit(BlingoCaseLabelNode n) { n.Value.Accept(this); n.Block?.Accept(this); }
        public void Visit(BlingoChunkExprNode n) { n.Expr.Accept(this); }
        public void Visit(BlingoInverseOpNode n) { n.Expr.Accept(this); }
        public void Visit(BlingoObjCallV4Node n) { n.Object.Accept(this); if (n.Name.Value != null) Methods.Add(n.Name.Value.AsString()); n.ArgList.Accept(this); }
        public void Visit(BlingoMemberExprNode n) { n.Expr.Accept(this); n.CastLib?.Accept(this); }
        public void Visit(BlingoObjPropExprNode n) { n.Object.Accept(this); n.Property.Accept(this); }
        public void Visit(BlingoPlayCmdStmtNode n) { n.Command.Accept(this); }
        public void Visit(BlingoThePropExprNode n) { n.Property.Accept(this); }
        public void Visit(BlingoMenuPropExprNode n) { n.Menu.Accept(this); n.Property.Accept(this); }
        public void Visit(BlingoSoundCmdStmtNode n) { n.Command.Accept(this); }
        public void Visit(BlingoSoundPropExprNode n) { n.Sound.Accept(this); n.Property.Accept(this); }
        public void Visit(BlingoCursorStmtNode n) { n.Value.Accept(this); }
        public void Visit(BlingoGoToStmtNode n) { n.Target.Accept(this); }
        public void Visit(BlingoAssignmentStmtNode n) { n.Target.Accept(this); n.Value.Accept(this); }
        public void Visit(BlingoSendSpriteStmtNode n) { n.Sprite.Accept(this); n.Message.Accept(this); n.Arguments?.Accept(this); }
        public void Visit(BlingoSendSpriteExprNode n) { n.Sprite.Accept(this); n.Message.Accept(this); n.Arguments?.Accept(this); }
        public void Visit(BlingoRangeExprNode n) { n.Start.Accept(this); n.End.Accept(this); }
        public void Visit(BlingoObjBracketExprNode n) { n.Object.Accept(this); n.Index.Accept(this); }
        public void Visit(BlingoSpritePropExprNode n) { n.Sprite.Accept(this); n.Property.Accept(this); }
        public void Visit(BlingoChunkDeleteStmtNode n) { n.Chunk.Accept(this); }
        public void Visit(BlingoChunkHiliteStmtNode n) { n.Chunk.Accept(this); }
        public void Visit(BlingoRepeatWhileStmtNode n) { n.Condition.Accept(this); n.Body.Accept(this); }
        public void Visit(BlingoMenuItemPropExprNode n) { n.MenuItem.Accept(this); n.Property.Accept(this); }
        public void Visit(BlingoObjPropIndexExprNode n) { n.Object.Accept(this); n.PropertyIndex.Accept(this); }
        public void Visit(BlingoRepeatWithInStmtNode n) { n.List.Accept(this); n.Body.Accept(this); }
        public void Visit(BlingoRepeatWithToStmtNode n) { n.Start.Accept(this); n.End.Accept(this); n.Body.Accept(this); }
        public void Visit(BlingoSpriteWithinExprNode n) { n.SpriteA.Accept(this); n.SpriteB.Accept(this); }
        public void Visit(BlingoLastStringChunkExprNode n) { n.Source.Accept(this); }
        public void Visit(BlingoSpriteIntersectsExprNode n) { n.SpriteA.Accept(this); n.SpriteB.Accept(this); }
        public void Visit(BlingoStringChunkCountExprNode n) { n.Source.Accept(this); }
        public void Visit(BlingoNotOpNode n) { n.Expr.Accept(this); }
        public void Visit(BlingoRepeatWithStmtNode n) { n.Start.Accept(this); n.End.Accept(this); n.Body.Accept(this); }
        public void Visit(BlingoRepeatUntilStmtNode n) { n.Condition.Accept(this); n.Body.Accept(this); }
        public void Visit(BlingoRepeatForeverStmtNode n) { n.Body.Accept(this); }
        public void Visit(BlingoRepeatTimesStmtNode n) { n.Count.Accept(this); n.Body.Accept(this); }
        public void Visit(BlingoExitRepeatIfStmtNode n) { n.Condition.Accept(this); }
        public void Visit(BlingoNextRepeatIfStmtNode n) { n.Condition.Accept(this); }
        public void Visit(BlingoErrorNode n) { }
        public void Visit(BlingoEndCaseNode n) { }
        public void Visit(BlingoWhenStmtNode n) { }
        public void Visit(BlingoGlobalDeclStmtNode n) { }
        public void Visit(BlingoPropertyDeclStmtNode n) { }
        public void Visit(BlingoInstanceDeclStmtNode n) { }
        public void Visit(BlingoExitRepeatStmtNode n) { }
        public void Visit(BlingoNextRepeatStmtNode n) { }
        public void Visit(BlingoVarNode n) { }
        public void Visit(BlingoDatumNode n) { }
        public void Visit(BlingoNextStmtNode n) { }
    }

    private class SendSpriteTypeResolver : IBlingoAstVisitor
    {
        private readonly Dictionary<string, string> _methodMap;
        private readonly Dictionary<string, string> _generated;
        private readonly BlingoToCSharpConverterSettings _settings;
        public SendSpriteTypeResolver(Dictionary<string, string> methodMap, Dictionary<string, string> generated, BlingoToCSharpConverterSettings settings)
        {
            _methodMap = methodMap;
            _generated = generated;
            _settings = settings;
        }
        public void Visit(BlingoHandlerNode n) => n.Block.Accept(this);
        public void Visit(BlingoCommentNode n) { }
        public void Visit(BlingoNewObjNode n) { n.ObjArgs.Accept(this); }
        public void Visit(BlingoLiteralNode n) { }
        public void Visit(BlingoCallNode n)
        {
            if (!string.IsNullOrEmpty(n.Name) && _methodMap.TryGetValue(n.Name, out var script))
                n.TargetType = script;
        }
        public void Visit(BlingoObjCallNode n) { n.ArgList.Accept(this); }
        public void Visit(BlingoBlockNode n) { foreach (var c in n.Children) c.Accept(this); }
        public void Visit(BlingoIfStmtNode n) { n.Condition.Accept(this); n.ThenBlock.Accept(this); if (n.HasElse) n.ElseBlock!.Accept(this); }
        public void Visit(BlingoIfElseStmtNode n) { n.Condition.Accept(this); n.ThenBlock.Accept(this); n.ElseBlock.Accept(this); }
        public void Visit(BlingoPutStmtNode n) { n.Value.Accept(this); n.Target?.Accept(this); }
        public void Visit(BlingoBinaryOpNode n) { n.Left.Accept(this); n.Right.Accept(this); }
        public void Visit(BlingoCaseStmtNode n) { n.Value.Accept(this); n.Otherwise?.Accept(this); }
        public void Visit(BlingoTheExprNode n) { }
        public void Visit(BlingoExitStmtNode n) { }
        public void Visit(BlingoReturnStmtNode n) { n.Value?.Accept(this); }
        public void Visit(BlingoTellStmtNode n) { n.Block.Accept(this); }
        public void Visit(BlingoOtherwiseNode n) { n.Block.Accept(this); }
        public void Visit(BlingoCaseLabelNode n) { n.Value.Accept(this); n.Block?.Accept(this); }
        public void Visit(BlingoChunkExprNode n) { n.Expr.Accept(this); }
        public void Visit(BlingoInverseOpNode n) { n.Expr.Accept(this); }
        public void Visit(BlingoObjCallV4Node n) { n.Object.Accept(this); n.ArgList.Accept(this); }
        public void Visit(BlingoMemberExprNode n) { n.Expr.Accept(this); n.CastLib?.Accept(this); }
        public void Visit(BlingoObjPropExprNode n) { n.Object.Accept(this); n.Property.Accept(this); }
        public void Visit(BlingoPlayCmdStmtNode n) { n.Command.Accept(this); }
        public void Visit(BlingoThePropExprNode n) { n.Property.Accept(this); }
        public void Visit(BlingoMenuPropExprNode n) { n.Menu.Accept(this); n.Property.Accept(this); }
        public void Visit(BlingoSoundCmdStmtNode n) { n.Command.Accept(this); }
        public void Visit(BlingoSoundPropExprNode n) { n.Sound.Accept(this); n.Property.Accept(this); }
        public void Visit(BlingoCursorStmtNode n) { n.Value.Accept(this); }
        public void Visit(BlingoGoToStmtNode n) { n.Target.Accept(this); }
        public void Visit(BlingoAssignmentStmtNode n) { n.Target.Accept(this); n.Value.Accept(this); }
        public void Visit(BlingoSendSpriteStmtNode n)
        {
            if (n.Message is BlingoDatumNode dn && dn.Datum.Type == BlingoDatum.DatumType.Symbol)
            {
                var name = dn.Datum.AsSymbol();
                if (_methodMap.TryGetValue(name, out var script))
                {
                    n.TargetType = script;
                }
                else
                {
                    if (!_generated.TryGetValue(name, out var className))
                    {
                        var pascal = char.ToUpperInvariant(name[0]) + name[1..];
                        className = CSharpName.ComposeName(pascal, BlingoScriptType.Behavior, _settings);
                        _generated[name] = className;
                    }
                    _methodMap[name] = className;
                    n.TargetType = className;
                }
            }
            n.Sprite.Accept(this);
            n.Message.Accept(this);
            n.Arguments?.Accept(this);
        }
        public void Visit(BlingoSendSpriteExprNode n)
        {
            if (n.Message is BlingoDatumNode dn && dn.Datum.Type == BlingoDatum.DatumType.Symbol)
            {
                var name = dn.Datum.AsSymbol();
                if (_methodMap.TryGetValue(name, out var script))
                {
                    n.TargetType = script;
                }
                else
                {
                    if (!_generated.TryGetValue(name, out var className))
                    {
                        var pascal = char.ToUpperInvariant(name[0]) + name[1..];
                        className = CSharpName.ComposeName(pascal, BlingoScriptType.Behavior, _settings);
                        _generated[name] = className;
                    }
                    _methodMap[name] = className;
                    n.TargetType = className;
                }
            }
            n.Sprite.Accept(this);
            n.Message.Accept(this);
            n.Arguments?.Accept(this);
        }
        public void Visit(BlingoRangeExprNode n) { n.Start.Accept(this); n.End.Accept(this); }
        public void Visit(BlingoObjBracketExprNode n) { n.Object.Accept(this); n.Index.Accept(this); }
        public void Visit(BlingoSpritePropExprNode n) { n.Sprite.Accept(this); n.Property.Accept(this); }
        public void Visit(BlingoChunkDeleteStmtNode n) { n.Chunk.Accept(this); }
        public void Visit(BlingoChunkHiliteStmtNode n) { n.Chunk.Accept(this); }
        public void Visit(BlingoRepeatWhileStmtNode n) { n.Condition.Accept(this); n.Body.Accept(this); }
        public void Visit(BlingoMenuItemPropExprNode n) { n.MenuItem.Accept(this); n.Property.Accept(this); }
        public void Visit(BlingoObjPropIndexExprNode n) { n.Object.Accept(this); n.PropertyIndex.Accept(this); }
        public void Visit(BlingoRepeatWithInStmtNode n) { n.List.Accept(this); n.Body.Accept(this); }
        public void Visit(BlingoRepeatWithToStmtNode n) { n.Start.Accept(this); n.End.Accept(this); n.Body.Accept(this); }
        public void Visit(BlingoSpriteWithinExprNode n) { n.SpriteA.Accept(this); n.SpriteB.Accept(this); }
        public void Visit(BlingoLastStringChunkExprNode n) { n.Source.Accept(this); }
        public void Visit(BlingoSpriteIntersectsExprNode n) { n.SpriteA.Accept(this); n.SpriteB.Accept(this); }
        public void Visit(BlingoStringChunkCountExprNode n) { n.Source.Accept(this); }
        public void Visit(BlingoNotOpNode n) { n.Expr.Accept(this); }
        public void Visit(BlingoVarNode n) { }
        public void Visit(BlingoRepeatWithStmtNode n) { n.Start.Accept(this); n.End.Accept(this); n.Body.Accept(this); }
        public void Visit(BlingoRepeatUntilStmtNode n) { n.Condition.Accept(this); n.Body.Accept(this); }
        public void Visit(BlingoRepeatForeverStmtNode n) { n.Body.Accept(this); }
        public void Visit(BlingoRepeatTimesStmtNode n) { n.Count.Accept(this); n.Body.Accept(this); }
        public void Visit(BlingoExitRepeatIfStmtNode n) { n.Condition.Accept(this); }
        public void Visit(BlingoNextRepeatIfStmtNode n) { n.Condition.Accept(this); }
        public void Visit(BlingoErrorNode n) { }
        public void Visit(BlingoEndCaseNode n) { }
        public void Visit(BlingoWhenStmtNode n) { }
        public void Visit(BlingoGlobalDeclStmtNode n) { }
        public void Visit(BlingoPropertyDeclStmtNode n) { }
        public void Visit(BlingoInstanceDeclStmtNode n) { }
        public void Visit(BlingoExitRepeatStmtNode n) { }
        public void Visit(BlingoNextRepeatStmtNode n) { }
        public void Visit(BlingoDatumNode n) { }
        public void Visit(BlingoNextStmtNode n) { }
    }
}

