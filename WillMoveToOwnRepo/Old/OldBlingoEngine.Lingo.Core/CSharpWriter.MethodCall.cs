using System;
using System.Collections.Generic;
using System.Linq;
using OldBlingoEngine.Lingo.Core;
using OldBlingoEngine.Lingo.Core.Tokenizer;

namespace BlingoEngine.Lingo.Core;

public partial class CSharpWriter
{
    public void Visit(BlingoCallNode node)
    {
        if (node.Callee is BlingoVarNode varNode)
        {
            if (varNode.VarName.Equals("voidp", StringComparison.OrdinalIgnoreCase))
            {
                node.Arguments.Accept(this);
                Append(" == null");
                return;
            }
            if (varNode.VarName.Equals("objectp", StringComparison.OrdinalIgnoreCase))
            {
                node.Arguments.Accept(this);
                Append(" != null");
                return;
            }
        }

        if (!string.IsNullOrEmpty(node.Name))
        {
            if (!string.IsNullOrEmpty(node.TargetType))
            {
                var param = node.TargetType.ToLowerInvariant();
                Append("CallMovieScript<");
                Append(node.TargetType);
                Append(">(");
                Append($"{param} => {param}.");
                Append(node.Name);
                Append("());");
                AppendLine();
            }
            else
            {
                AppendLine($"{node.Name}();");
            }
            return;
        }

        if (node.Callee is BlingoVarNode func)
        {
            var name = func.VarName.ToLowerInvariant();
            List<BlingoNode>? args;
            if (node.Arguments is BlingoDatumNode dn &&
                dn.Datum.Type == BlingoDatum.DatumType.ArgList &&
                dn.Datum.Value is List<BlingoNode> list)
            {
                args = list;
            }
            else
            {
                args = new List<BlingoNode> { node.Arguments };
            }

            switch (name)
            {
                case "append" when args.Count >= 2:
                    args[0].Accept(this);
                    Append(".Add(");
                    args[1].Accept(this);
                    AppendLine(");");
                    return;
                case "add" when args.Count >= 2:
                    args[0].Accept(this);
                    Append(".Add(");
                    args[1].Accept(this);
                    AppendLine(");");
                    return;
                case "addat" when args.Count >= 3:
                    args[0].Accept(this);
                    Append(".AddAt(");
                    args[1].Accept(this);
                    Append(", ");
                    args[2].Accept(this);
                    AppendLine(");");
                    return;
                case "addprop" when args.Count >= 3:
                    args[0].Accept(this);
                    Append(".Add(");
                    args[1].Accept(this);
                    Append(", ");
                    args[2].Accept(this);
                    AppendLine(");");
                    return;
                case "deleteat" when args.Count >= 2:
                    args[0].Accept(this);
                    Append(".DeleteAt(");
                    args[1].Accept(this);
                    AppendLine(");");
                    return;
                case "getat" when args.Count >= 2:
                    args[0].Accept(this);
                    Append(".GetAt(");
                    args[1].Accept(this);
                    Append(")");
                    return;
                case "setat" when args.Count >= 3:
                    args[0].Accept(this);
                    Append(".SetAt(");
                    args[1].Accept(this);
                    Append(", ");
                    args[2].Accept(this);
                    AppendLine(");");
                    return;
                case "count" when args.Count >= 1:
                    args[0].Accept(this);
                    Append(".Count");
                    return;
                case "deleteone" when args.Count >= 2:
                    args[0].Accept(this);
                    Append(".DeleteOne(");
                    args[1].Accept(this);
                    AppendLine(");");
                    return;
                case "deleteall" when args.Count >= 1:
                    args[0].Accept(this);
                    AppendLine(".DeleteAll();");
                    return;
                case "duplicate" when args.Count >= 1:
                    args[0].Accept(this);
                    Append(".Duplicate()");
                    return;
                case "getone" when args.Count >= 1:
                    args[0].Accept(this);
                    Append(".GetOne()");
                    return;
                case "getlast" when args.Count >= 1:
                    args[0].Accept(this);
                    Append(".GetLast()");
                    return;
                case "getavalue" when args.Count >= 1:
                    args[0].Accept(this);
                    Append(".GetAValue()");
                    return;
                case "ilk" when args.Count >= 1:
                    args[0].Accept(this);
                    Append(".Ilk()");
                    return;
                case "listp" when args.Count >= 1:
                    args[0].Accept(this);
                    Append(".ListP()");
                    return;
                case "tolist" when args.Count >= 1:
                    args[0].Accept(this);
                    Append(".ToList()");
                    return;
                case "sort" when args.Count >= 1:
                    args[0].Accept(this);
                    if (args.Count >= 2)
                    {
                        Append(".Sort(");
                        args[1].Accept(this);
                        AppendLine(");");
                    }
                    else
                    {
                        AppendLine(".Sort();");
                    }
                    return;
                case "setprop" when args.Count >= 3:
                    args[0].Accept(this);
                    Append(".SetProp(");
                    args[1].Accept(this);
                    Append(", ");
                    args[2].Accept(this);
                    AppendLine(");");
                    return;
                case "deleteprop" when args.Count >= 2:
                    args[0].Accept(this);
                    Append(".DeleteProp(");
                    args[1].Accept(this);
                    AppendLine(");");
                    return;
                case "getprop" when args.Count >= 2:
                    args[0].Accept(this);
                    Append(".GetProp(");
                    args[1].Accept(this);
                    Append(")");
                    return;
                case "getaprop" when args.Count >= 2:
                    args[0].Accept(this);
                    Append(".GetaProp(");
                    args[1].Accept(this);
                    Append(")");
                    return;
                case "getaprop" when args.Count >= 1:
                    args[0].Accept(this);
                    Append(".GetAProp()");
                    return;
                case "getpropat" when args.Count >= 2:
                    args[0].Accept(this);
                    Append(".GetPropAt(");
                    args[1].Accept(this);
                    Append(")");
                    return;
                case "setaprop" when args.Count >= 3:
                    args[0].Accept(this);
                    Append(".SetaProp(");
                    args[1].Accept(this);
                    Append(", ");
                    args[2].Accept(this);
                    AppendLine(");");
                    return;
                case "setaprop" when args.Count >= 2:
                    args[0].Accept(this);
                    Append(".SetAProp(");
                    args[1].Accept(this);
                    AppendLine(");");
                    return;
                case "findpos" when args.Count >= 2:
                    args[0].Accept(this);
                    Append(".FindPos(");
                    args[1].Accept(this);
                    Append(")");
                    return;
                case "findposnear" when args.Count >= 2:
                    args[0].Accept(this);
                    Append(".FindPosNear(");
                    args[1].Accept(this);
                    Append(")");
                    return;
                case "getpos" when args.Count >= 2:
                    args[0].Accept(this);
                    Append(".GetPos(");
                    args[1].Accept(this);
                    Append(")");
                    return;
                case "max" when args.Count >= 1:
                    args[0].Accept(this);
                    Append(".Max()");
                    return;
                case "min" when args.Count >= 1:
                    args[0].Accept(this);
                    Append(".Min()");
                    return;
                case "value" when args.Count >= 1:
                    Append("Convert.ToInt32(");
                    args[0].Accept(this);
                    Append(")");
                    return;
            }
        }

        WriteCallExpr(node);
        AppendLine(";");
    }

    private void WriteCallExpr(BlingoCallNode node)
    {
        if (node.Callee is BlingoObjPropExprNode prop &&
            prop.Property is BlingoVarNode { VarName: "new" } &&
            prop.Object is BlingoCallNode { Callee: BlingoVarNode { VarName: "script" }, Arguments: BlingoDatumNode dn } &&
            dn.Datum.Type == BlingoDatum.DatumType.String)
        {
            var scriptName = dn.Datum.AsString();
            if (_scriptTypes.TryGetValue(scriptName, out var st))
            {
                var cls = CSharpName.ComposeName(scriptName, st, _settings);
                Append("new ");
                Append(cls);
                Append("(_env, _globalvars");
                if (node.Arguments is not BlingoBlockNode)
                {
                    Append(", ");
                    node.Arguments.Accept(this);
                }
                Append(")");
                return;
            }
        }

        if (node.Callee is BlingoVarNode func)
        {
            var name = func.VarName;
            if (name.Equals("castLib", StringComparison.OrdinalIgnoreCase))
                Append("CastLib");
            else if (name.Equals("sprite", StringComparison.OrdinalIgnoreCase))
                Append("Sprite");
            else
                Append(name);
        }
        else
        {
            node.Callee.Accept(this);
        }
        Append("(");
        node.Arguments.Accept(this);
        Append(")");
    }

    private void WriteObjCallExpr(BlingoObjCallNode node)
    {
        var name = node.Name.Value.AsString();
        if (name.Equals("castLib", StringComparison.OrdinalIgnoreCase))
            Append("CastLib");
        else
            Append(name);
        Append("(");
        node.ArgList.Accept(this);
        Append(")");
    }

    private void WriteObjCallV4Expr(BlingoObjCallV4Node node)
    {
        var methodName = node.Name.Value.AsString();

        // script("Foo").new(args)
        if (methodName.Equals("new", StringComparison.OrdinalIgnoreCase) &&
            node.Object is BlingoCallNode call &&
            call.Callee is BlingoVarNode { VarName: "script" } &&
            call.Arguments is BlingoDatumNode dn &&
            dn.Datum.Type == BlingoDatum.DatumType.String)
        {
            var scriptName = dn.Datum.AsString();
            if (_scriptTypes.TryGetValue(scriptName, out var st))
            {
                var cls = CSharpName.ComposeName(scriptName, st, _settings);
                Append("new ");
                Append(cls);
                Append("(_env, _globalvars");
                bool hasArgs = node.ArgList.Datum.Type == BlingoDatum.DatumType.ArgList &&
                               node.ArgList.Datum.Value is List<BlingoNode> list && list.Count > 0;
                if (hasArgs)
                {
                    Append(", ");
                    node.ArgList.Accept(this);
                }
                Append(")");
                return;
            }
        }

        var startLen = _sb.Length;
        if (node.Object is BlingoCallNode callObj)
            WriteCallExpr(callObj);
        else if (node.Object is BlingoObjCallNode objCall)
            WriteObjCallExpr(objCall);
        else
            node.Object.Accept(this);
        TrimSemicolon(startLen);

        Append(".");
        var lower = methodName.ToLowerInvariant();
        var pascal = lower switch
        {
            "deleteone" => "DeleteOne",
            "getpos" => "GetPos",
            "deleteat" => "DeleteAt",
            "getat" => "GetAt",
            "setat" => "SetAt",
            "count" => "Count",
            "add" => "Add",
            "addat" => "AddAt",
            "addprop" => "Add",
            _ => methodName
        };
        Append(pascal);
        Append("(");
        node.ArgList.Accept(this);
        Append(")");
    }
}


