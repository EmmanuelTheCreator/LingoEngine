using System.Collections.Generic;
using LingoEngine.Lingo.Core.Tokenizer;

namespace LingoEngine.Lingo.Core;

public static class LingoToCSharpConverter
{
    public static string Convert(string lingoSource)
    {
        var trimmed = lingoSource.Trim();

        var match = System.Text.RegularExpressions.Regex.Match(
            trimmed,
            @"^member\s*\(\s*""(?<name>[^""]+)""\s*\)\.text$",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (match.Success)
        {
            return $"Member<LingoMemberText>(\"{match.Groups["name"].Value}\").Text";
        }

        if (TryConvertNewMember(trimmed, out var converted))
            return converted;

        var parser = new LingoAstParser();
        var ast = parser.Parse(lingoSource);
        return CSharpWriter.Write(ast);
    }

    public static LingoBatchResult Convert(IEnumerable<LingoScriptFile> scripts)
    {
        var result = new LingoBatchResult();
        var asts = new Dictionary<string, LingoNode>();
        var methodsPerScript = new Dictionary<string, HashSet<string>>();

        foreach (var file in scripts)
        {
            var parser = new LingoAstParser();
            var ast = parser.Parse(file.Source);
            asts[file.Name] = ast;

            var handlers = ExtractHandlerNames(file.Source);
            var custom = new HashSet<string>();
            foreach (var h in handlers)
            {
                if (!DefaultMethods.Contains(h))
                {
                    custom.Add(h);
                    result.CustomMethods.Add(h);
                }
            }
            methodsPerScript[file.Name] = custom;
        }

        var methodMap = new Dictionary<string, string>();
        foreach (var kvp in methodsPerScript)
        {
            foreach (var m in kvp.Value)
            {
                if (!methodMap.ContainsKey(m))
                    methodMap[m] = kvp.Key;
            }
        }

        var annotator = new SendSpriteTypeResolver(methodMap);
        foreach (var ast in asts.Values)
            ast.Accept(annotator);

        foreach (var kvp in asts)
            result.ConvertedScripts[kvp.Key] = CSharpWriter.Write(kvp.Value);

        return result;
    }

    /// <summary>
    /// Generates a minimal C# class wrapper for a single Lingo script.
    /// Only the class declaration and constructor are emitted.
    /// The class name is derived from the file name plus the script type suffix.
    /// </summary>
    public static string ConvertClass(LingoScriptFile script)
    {
        var suffix = script.Type switch
        {
            LingoScriptType.Movie => "MovieScript",
            LingoScriptType.Parent => "ParentScript",
            LingoScriptType.Behavior => "Behavior",
            _ => "Script"
        };

        var baseType = script.Type switch
        {
            LingoScriptType.Movie => "LingoMovieScript",
            LingoScriptType.Parent => "LingoParentScript",
            LingoScriptType.Behavior => "LingoSpriteBehavior",
            _ => "LingoScriptBase"
        };

        var className = script.Name + suffix;

        var handlers = ExtractHandlerNames(script.Source);
        string[] propDescRequired =
        {
            "getPropertyDescriptionList",
            "getBehaviorDescription",
            "getBehaviorTooltip",
            "runPropertyDialog",
            "isOKToAttach"
        };
        bool hasPropDescHandlers = true;
        foreach (string h in propDescRequired)
        {
            if (!handlers.Contains(h))
            {
                hasPropDescHandlers = false;
                break;
            }
        }

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"public class {className} : {baseType}{(hasPropDescHandlers ? ", ILingoPropertyDescriptionList" : string.Empty)}");
        sb.AppendLine("{");

        bool needsGlobal = script.Type == LingoScriptType.Movie || script.Type == LingoScriptType.Parent;
        if (needsGlobal)
        {
            sb.AppendLine("    private readonly GlobalVars _global;");
            sb.AppendLine();
        }

        sb.Append($"    public {className}(ILingoMovieEnvironment env");
        if (needsGlobal)
            sb.Append(", GlobalVars global");
        sb.Append(") : base(env)");

        if (needsGlobal)
        {
            sb.AppendLine();
            sb.AppendLine("    {");
            sb.AppendLine("        _global = global;");
            sb.AppendLine("    }");
        }
        else
        {
            sb.AppendLine(" { }");
        }

        sb.AppendLine("}");
        return sb.ToString();
    }

    private static HashSet<string> ExtractHandlerNames(string source)
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var regex = new System.Text.RegularExpressions.Regex(@"(?im)^\s*on\s+(\w+)");
        foreach (System.Text.RegularExpressions.Match m in regex.Matches(source))
            set.Add(m.Groups[1].Value);
        return set;
    }

    private static bool TryConvertNewMember(string source, out string converted)
    {
        converted = string.Empty;
        var tokenizer = new LingoTokenizer(source);
        var tokens = new List<LingoToken>();
        LingoToken tok;
        do
        {
            tok = tokenizer.NextToken();
            tokens.Add(tok);
        } while (tok.Type != LingoTokenType.Eof && tokens.Count < 32);

        int idx = 0;

        // optional assignment prefix
        string? lhs = null;
        if (tokens.Count > 2 && tokens[0].Type == LingoTokenType.Identifier && tokens[1].Type == LingoTokenType.Equals)
        {
            lhs = tokens[0].Lexeme;
            idx = 2;
        }

        if (idx + 4 >= tokens.Count) return false;
        if (tokens[idx].Type != LingoTokenType.Identifier) return false;
        string obj = tokens[idx].Lexeme;
        if (tokens[idx + 1].Type != LingoTokenType.Dot) return false;
        if (tokens[idx + 2].Type != LingoTokenType.Identifier || !tokens[idx + 2].Lexeme.Equals("newMember", System.StringComparison.OrdinalIgnoreCase))
            return false;
        if (tokens[idx + 3].Type != LingoTokenType.LeftParen) return false;

        int pos = idx + 4;
        if (pos >= tokens.Count) return false;
        if (tokens[pos].Type != LingoTokenType.Identifier) return false;
        string type = tokens[pos].Lexeme.ToLowerInvariant();
        string method = type switch
        {
            "bitmap" => "Picture",
            "sound" => "Sound",
            "filmloop" => "FilmLoop",
            "text" => "Text",
            _ => "Member"
        };
        pos++;

        string rest = string.Empty;
        if (pos < tokens.Count && tokens[pos].Type == LingoTokenType.Comma)
        {
            pos++;
            var parts = new List<string>();
            while (pos < tokens.Count && tokens[pos].Type != LingoTokenType.RightParen)
            {
                parts.Add(tokens[pos].Lexeme);
                pos++;
            }
            rest = string.Join(" ", parts);
        }

        if (pos >= tokens.Count || tokens[pos].Type != LingoTokenType.RightParen)
            return false;
        pos++;
        if (pos < tokens.Count && tokens[pos].Type != LingoTokenType.Eof)
            return false;

        var call = $"{obj}.New.{method}({rest})".TrimEnd();
        converted = lhs != null ? $"{lhs} = {call};" : call;
        return true;
    }

    private static readonly HashSet<string> DefaultMethods = new(
        new[]{"StepFrame","PrepareFrame","EnterFrame","ExitFrame","BeginSprite",
               "EndSprite","MouseDown","MouseUp","MouseMove","MouseEnter",
               "MouseLeave","MouseWithin","MouseExit","PrepareMovie","StartMovie",
               "StopMovie","KeyDown","KeyUp","Focus","Blur"});

    private class CustomMethodCollector : ILingoAstVisitor
    {
        public HashSet<string> Methods { get; } = new();
        public void Visit(LingoHandlerNode n) => n.Block.Accept(this);
        public void Visit(LingoCommentNode n) { }
        public void Visit(LingoNewObjNode n) { n.ObjArgs.Accept(this); }
        public void Visit(LingoLiteralNode n) { }
        public void Visit(LingoCallNode n){ if(!string.IsNullOrEmpty(n.Name)) Methods.Add(n.Name); }
        public void Visit(LingoObjCallNode n){ if(n.Name.Value!=null) Methods.Add(n.Name.Value.AsString()); }
        public void Visit(LingoBlockNode n){ foreach(var c in n.Children) c.Accept(this); }
        public void Visit(LingoIfStmtNode n){ n.Condition.Accept(this); n.ThenBlock.Accept(this); if(n.HasElse) n.ElseBlock!.Accept(this); }
        public void Visit(LingoIfElseStmtNode n){ n.Condition.Accept(this); n.ThenBlock.Accept(this); n.ElseBlock.Accept(this); }
        public void Visit(LingoPutStmtNode n){ n.Value.Accept(this); n.Target.Accept(this); }
        public void Visit(LingoBinaryOpNode n){ n.Left.Accept(this); n.Right.Accept(this); }
        public void Visit(LingoCaseStmtNode n){ n.Value.Accept(this); n.Otherwise?.Accept(this); }
        public void Visit(LingoTheExprNode n) { }
        public void Visit(LingoExitStmtNode n) { }
        public void Visit(LingoReturnStmtNode n){ n.Value?.Accept(this); }
        public void Visit(LingoTellStmtNode n){ n.Block.Accept(this); }
        public void Visit(LingoOtherwiseNode n){ n.Block.Accept(this); }
        public void Visit(LingoCaseLabelNode n){ n.Value.Accept(this); n.Block?.Accept(this); }
        public void Visit(LingoChunkExprNode n){ n.Expr.Accept(this); }
        public void Visit(LingoInverseOpNode n){ n.Expr.Accept(this); }
        public void Visit(LingoObjCallV4Node n){ n.Object.Accept(this); if(n.Name.Value!=null) Methods.Add(n.Name.Value.AsString()); n.ArgList.Accept(this); }
        public void Visit(LingoMemberExprNode n){ n.Expr.Accept(this); }
        public void Visit(LingoObjPropExprNode n){ n.Object.Accept(this); n.Property.Accept(this); }
        public void Visit(LingoPlayCmdStmtNode n){ n.Command.Accept(this); }
        public void Visit(LingoThePropExprNode n){ n.Property.Accept(this); }
        public void Visit(LingoMenuPropExprNode n){ n.Menu.Accept(this); n.Property.Accept(this); }
        public void Visit(LingoSoundCmdStmtNode n){ n.Command.Accept(this); }
        public void Visit(LingoSoundPropExprNode n){ n.Sound.Accept(this); n.Property.Accept(this); }
        public void Visit(LingoAssignmentStmtNode n){ n.Target.Accept(this); n.Value.Accept(this); }
        public void Visit(LingoSendSpriteStmtNode n){ n.Sprite.Accept(this); n.Message.Accept(this); }
        public void Visit(LingoObjBracketExprNode n){ n.Object.Accept(this); n.Index.Accept(this); }
        public void Visit(LingoSpritePropExprNode n){ n.Sprite.Accept(this); n.Property.Accept(this); }
        public void Visit(LingoChunkDeleteStmtNode n){ n.Chunk.Accept(this); }
        public void Visit(LingoChunkHiliteStmtNode n){ n.Chunk.Accept(this); }
        public void Visit(LingoRepeatWhileStmtNode n){ n.Condition.Accept(this); n.Body.Accept(this); }
        public void Visit(LingoMenuItemPropExprNode n){ n.MenuItem.Accept(this); n.Property.Accept(this); }
        public void Visit(LingoObjPropIndexExprNode n){ n.Object.Accept(this); n.PropertyIndex.Accept(this); }
        public void Visit(LingoRepeatWithInStmtNode n){ n.List.Accept(this); n.Body.Accept(this); }
        public void Visit(LingoRepeatWithToStmtNode n){ n.Start.Accept(this); n.End.Accept(this); n.Body.Accept(this); }
        public void Visit(LingoSpriteWithinExprNode n){ n.SpriteA.Accept(this); n.SpriteB.Accept(this); }
        public void Visit(LingoLastStringChunkExprNode n){ n.Source.Accept(this); }
        public void Visit(LingoSpriteIntersectsExprNode n){ n.SpriteA.Accept(this); n.SpriteB.Accept(this); }
        public void Visit(LingoStringChunkCountExprNode n){ n.Source.Accept(this); }
        public void Visit(LingoNotOpNode n){ n.Expr.Accept(this); }
        public void Visit(LingoRepeatWithStmtNode n){ n.Start.Accept(this); n.End.Accept(this); n.Body.Accept(this); }
        public void Visit(LingoRepeatUntilStmtNode n){ n.Condition.Accept(this); n.Body.Accept(this); }
        public void Visit(LingoRepeatForeverStmtNode n){ n.Body.Accept(this); }
        public void Visit(LingoRepeatTimesStmtNode n){ n.Count.Accept(this); n.Body.Accept(this); }
        public void Visit(LingoExitRepeatIfStmtNode n){ n.Condition.Accept(this); }
        public void Visit(LingoNextRepeatIfStmtNode n){ n.Condition.Accept(this); }
        public void Visit(LingoErrorNode n){}
        public void Visit(LingoEndCaseNode n){}
        public void Visit(LingoWhenStmtNode n){}
        public void Visit(LingoGlobalDeclStmtNode n){}
        public void Visit(LingoPropertyDeclStmtNode n){}
        public void Visit(LingoInstanceDeclStmtNode n){}
        public void Visit(LingoExitRepeatStmtNode n){}
        public void Visit(LingoNextRepeatStmtNode n){}
        public void Visit(LingoVarNode n){}
        public void Visit(LingoDatumNode n){}
        public void Visit(LingoNextStmtNode n){}
    }

    private class SendSpriteTypeResolver : ILingoAstVisitor
    {
        private readonly Dictionary<string, string> _methodMap;
        public SendSpriteTypeResolver(Dictionary<string, string> methodMap)
        {
            _methodMap = methodMap;
        }
        public void Visit(LingoHandlerNode n) => n.Block.Accept(this);
        public void Visit(LingoCommentNode n) { }
        public void Visit(LingoNewObjNode n) { n.ObjArgs.Accept(this); }
        public void Visit(LingoLiteralNode n) { }
        public void Visit(LingoCallNode n)
        {
            if (!string.IsNullOrEmpty(n.Name) && _methodMap.TryGetValue(n.Name, out var script))
                n.TargetType = script;
        }
        public void Visit(LingoObjCallNode n) { n.ArgList.Accept(this); }
        public void Visit(LingoBlockNode n) { foreach (var c in n.Children) c.Accept(this); }
        public void Visit(LingoIfStmtNode n) { n.Condition.Accept(this); n.ThenBlock.Accept(this); if (n.HasElse) n.ElseBlock!.Accept(this); }
        public void Visit(LingoIfElseStmtNode n) { n.Condition.Accept(this); n.ThenBlock.Accept(this); n.ElseBlock.Accept(this); }
        public void Visit(LingoPutStmtNode n) { n.Value.Accept(this); n.Target.Accept(this); }
        public void Visit(LingoBinaryOpNode n) { n.Left.Accept(this); n.Right.Accept(this); }
        public void Visit(LingoCaseStmtNode n) { n.Value.Accept(this); n.Otherwise?.Accept(this); }
        public void Visit(LingoTheExprNode n) { }
        public void Visit(LingoExitStmtNode n) { }
        public void Visit(LingoReturnStmtNode n) { n.Value?.Accept(this); }
        public void Visit(LingoTellStmtNode n) { n.Block.Accept(this); }
        public void Visit(LingoOtherwiseNode n) { n.Block.Accept(this); }
        public void Visit(LingoCaseLabelNode n) { n.Value.Accept(this); n.Block?.Accept(this); }
        public void Visit(LingoChunkExprNode n) { n.Expr.Accept(this); }
        public void Visit(LingoInverseOpNode n) { n.Expr.Accept(this); }
        public void Visit(LingoObjCallV4Node n) { n.Object.Accept(this); n.ArgList.Accept(this); }
        public void Visit(LingoMemberExprNode n) { n.Expr.Accept(this); }
        public void Visit(LingoObjPropExprNode n) { n.Object.Accept(this); n.Property.Accept(this); }
        public void Visit(LingoPlayCmdStmtNode n) { n.Command.Accept(this); }
        public void Visit(LingoThePropExprNode n) { n.Property.Accept(this); }
        public void Visit(LingoMenuPropExprNode n) { n.Menu.Accept(this); n.Property.Accept(this); }
        public void Visit(LingoSoundCmdStmtNode n) { n.Command.Accept(this); }
        public void Visit(LingoSoundPropExprNode n) { n.Sound.Accept(this); n.Property.Accept(this); }
        public void Visit(LingoAssignmentStmtNode n) { n.Target.Accept(this); n.Value.Accept(this); }
        public void Visit(LingoSendSpriteStmtNode n)
        {
            if (n.Message is LingoDatumNode dn && dn.Datum.Type == LingoDatum.DatumType.Symbol)
            {
                var name = dn.Datum.AsSymbol();
                if (_methodMap.TryGetValue(name, out var script))
                    n.TargetType = script;
            }
            n.Sprite.Accept(this);
            n.Message.Accept(this);
        }
        public void Visit(LingoObjBracketExprNode n) { n.Object.Accept(this); n.Index.Accept(this); }
        public void Visit(LingoSpritePropExprNode n) { n.Sprite.Accept(this); n.Property.Accept(this); }
        public void Visit(LingoChunkDeleteStmtNode n) { n.Chunk.Accept(this); }
        public void Visit(LingoChunkHiliteStmtNode n) { n.Chunk.Accept(this); }
        public void Visit(LingoRepeatWhileStmtNode n) { n.Condition.Accept(this); n.Body.Accept(this); }
        public void Visit(LingoMenuItemPropExprNode n) { n.MenuItem.Accept(this); n.Property.Accept(this); }
        public void Visit(LingoObjPropIndexExprNode n) { n.Object.Accept(this); n.PropertyIndex.Accept(this); }
        public void Visit(LingoRepeatWithInStmtNode n) { n.List.Accept(this); n.Body.Accept(this); }
        public void Visit(LingoRepeatWithToStmtNode n) { n.Start.Accept(this); n.End.Accept(this); n.Body.Accept(this); }
        public void Visit(LingoSpriteWithinExprNode n) { n.SpriteA.Accept(this); n.SpriteB.Accept(this); }
        public void Visit(LingoLastStringChunkExprNode n) { n.Source.Accept(this); }
        public void Visit(LingoSpriteIntersectsExprNode n) { n.SpriteA.Accept(this); n.SpriteB.Accept(this); }
        public void Visit(LingoStringChunkCountExprNode n) { n.Source.Accept(this); }
        public void Visit(LingoNotOpNode n) { n.Expr.Accept(this); }
        public void Visit(LingoVarNode n) { }
        public void Visit(LingoRepeatWithStmtNode n) { n.Start.Accept(this); n.End.Accept(this); n.Body.Accept(this); }
        public void Visit(LingoRepeatUntilStmtNode n) { n.Condition.Accept(this); n.Body.Accept(this); }
        public void Visit(LingoRepeatForeverStmtNode n) { n.Body.Accept(this); }
        public void Visit(LingoRepeatTimesStmtNode n) { n.Count.Accept(this); n.Body.Accept(this); }
        public void Visit(LingoExitRepeatIfStmtNode n) { n.Condition.Accept(this); }
        public void Visit(LingoNextRepeatIfStmtNode n) { n.Condition.Accept(this); }
        public void Visit(LingoErrorNode n) { }
        public void Visit(LingoEndCaseNode n) { }
        public void Visit(LingoWhenStmtNode n) { }
        public void Visit(LingoGlobalDeclStmtNode n) { }
        public void Visit(LingoPropertyDeclStmtNode n) { }
        public void Visit(LingoInstanceDeclStmtNode n) { }
        public void Visit(LingoExitRepeatStmtNode n) { }
        public void Visit(LingoNextRepeatStmtNode n) { }
        public void Visit(LingoDatumNode n) { }
        public void Visit(LingoNextStmtNode n) { }
    }
}
