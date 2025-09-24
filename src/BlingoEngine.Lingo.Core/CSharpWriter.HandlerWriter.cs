using System;
using System.Collections.Generic;
using System.Linq;
using BlingoEngine.Lingo.Core.Tokenizer;

namespace BlingoEngine.Lingo.Core;

public partial class CSharpWriter
{
    private record PropDesc(string Name, string Comment, string DefaultValue);

    private static readonly Dictionary<string, string> _knownHandlerNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ["blur"] = "Blur",
        ["focus"] = "Focus",
        ["keydown"] = "KeyDown",
        ["keyup"] = "KeyUp",
        ["mousewithin"] = "MouseWithin",
        ["mouseleave"] = "MouseLeave",
        ["mousedown"] = "MouseDown",
        ["mouseup"] = "MouseUp",
        ["mousemove"] = "MouseMove",
        ["mousewheel"] = "MouseWheel",
        ["mouseenter"] = "MouseEnter",
        ["mouseexit"] = "MouseExit",
        ["beginsprite"] = "BeginSprite",
        ["endsprite"] = "EndSprite",
        ["stepframe"] = "StepFrame",
        ["prepareframe"] = "PrepareFrame",
        ["enterframe"] = "EnterFrame",
        ["exitframe"] = "ExitFrame",
        ["preparemovie"] = "PrepareMovie",
        ["startmovie"] = "StartMovie",
        ["stopmovie"] = "StopMovie"
    };

    private static string FormatDefault(BlingoDatum datum, string? format)
    {
        var value = datum.AsString();
        if (string.Equals(format, "string", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(format, "symbol", StringComparison.OrdinalIgnoreCase))
        {
            return $"\"{EscapeString(value)}\"";
        }
        return value;
    }

    private void WritePropertyDescriptionListHandler(BlingoHandlerNode node)
    {
        Append(_methodAccessModifier);
        Append(" BehaviorPropertyDescriptionList? GetPropertyDescriptionList()");
        AppendLine();
        AppendLine("{");
        Indent();

        var propBuilder = new OrderedPropertyListBuilder<string, PropDesc>(StringComparer.OrdinalIgnoreCase);
        foreach (var child in node.Block.Children)
        {
            if (child is BlingoCallNode call &&
                call.Callee is BlingoVarNode v &&
                v.VarName.Equals("addProp", StringComparison.OrdinalIgnoreCase) &&
                call.Arguments is BlingoDatumNode argDatum &&
                argDatum.Datum.Type == BlingoDatum.DatumType.ArgList &&
                argDatum.Datum.Value is List<BlingoNode> args &&
                args.Count >= 3)
            {
                if (args[1] is not BlingoDatumNode symNode) continue;
                var propName = symNode.Datum.AsSymbol();
                if (args[2] is not BlingoDatumNode propList ||
                    propList.Datum.Type != BlingoDatum.DatumType.PropList ||
                    propList.Datum.Value is not List<BlingoNode> plist)
                    continue;

                string? comment = null;
                string? format = null;
                BlingoDatum? defDatum = null;

                for (int i = 0; i + 1 < plist.Count; i += 2)
                {
                    if (plist[i] is not BlingoDatumNode keyNode) continue;
                    var key = keyNode.Datum.AsSymbol();
                    var valNode = plist[i + 1];
                    if (key == "comment")
                    {
                        if (valNode is BlingoDatumNode dn) comment = dn.Datum.AsString();
                    }
                    else if (key == "format")
                    {
                        if (valNode is BlingoDatumNode dn) format = dn.Datum.AsSymbol();
                    }
                    else if (key == "default")
                    {
                        if (valNode is BlingoDatumNode dn) defDatum = dn.Datum;
                    }
                }

                if (propName != null && comment != null && defDatum != null)
                {
                    var defVal = FormatDefault(defDatum, format);
                    propBuilder.AddOrUpdate(propName, new PropDesc(propName, EscapeString(comment), defVal));
                }
            }
        }

        var props = propBuilder.Items;
        if (props.Count > 0)
        {
            AppendLine("return new BehaviorPropertyDescriptionList()");
            Indent();
            for (int i = 0; i < props.Count; i++)
            {
                var p = props[i];
                var line = $".Add(this, x => x.{p.Name}, \"{p.Comment}\", {p.DefaultValue})";
                if (i == props.Count - 1)
                {
                    line += ";";
                }
                AppendLine(line);
            }
            Unindent();
        }
        else
        {
            AppendLine("return new BehaviorPropertyDescriptionList();");
        }

        Unindent();
        AppendLine("}");
        AppendLine();
    }

    public void Visit(BlingoHandlerNode node)
    {
        var prevHandler = _currentHandlerName;
        var name = node.Handler?.Name ?? string.Empty;
        _currentHandlerName = name;

        if (name.Equals("getPropertyDescriptionList", StringComparison.OrdinalIgnoreCase))
        {
            WritePropertyDescriptionListHandler(node);
            _currentHandlerName = prevHandler;
            return;
        }

        if (name.Length > 0)
        {
            string pascal = _knownHandlerNames.TryGetValue(name, out var canonical)
                ? canonical
                : char.ToUpperInvariant(name[0]) + name[1..];
            var lower = name.ToLowerInvariant();
            string? paramDecl = lower switch
            {
                "blur" or "focus" => string.Empty,
                "keydown" or "keyup" => "BlingoKeyEvent key",
                "mousedown" or "mouseup" or "mousemove" or "mousewheel" or
                "mousewithin" or "mouseleave" or "mouseenter" or "mouseexit" => "BlingoMouseEvent mouse",
                _ => null
            };

            if (paramDecl != null)
            {
                Append(_methodAccessModifier);
                Append(" void ");
                Append(pascal);
                Append("(");
                Append(paramDecl);
                AppendLine(")");
                AppendLine("{");
                Indent();
                if (!string.IsNullOrEmpty(paramDecl))
                {
                    var paramVar = lower is "keydown" or "keyup" ? "key" : "mouse";
                    foreach (var a in node.Handler.ArgumentNames.Where(a => !a.Equals("me", StringComparison.OrdinalIgnoreCase)))
                        AppendLine($"var {a} = {paramVar};");
                }
                node.Block.Accept(this);
                Unindent();
                AppendLine("}");
                AppendLine();
            }
            else
            {
                Append(_methodAccessModifier);
                Append(" void ");
                Append(pascal);
                Append("(");
                var args = node.Handler.ArgumentNames
                    .Where(a => !a.Equals("me", StringComparison.OrdinalIgnoreCase))
                    .Select(a =>
                    {
                        var type = "object";
                        if (_methodSignatures != null &&
                            _methodSignatures.TryGetValue(name, out var sig))
                        {
                            var param = sig.Parameters.FirstOrDefault(p => p.Name.Equals(a, StringComparison.OrdinalIgnoreCase));
                            if (param != null)
                                type = param.Type;
                        }
                        return $"{type} {a}";
                    });
                Append(string.Join(", ", args));
                AppendLine(")");
                AppendLine("{");
                Indent();
                node.Block.Accept(this);
                Unindent();
                AppendLine("}");
                AppendLine();
            }
        }
        else
        {
            node.Block.Accept(this);
        }

        _currentHandlerName = prevHandler;
    }
}


