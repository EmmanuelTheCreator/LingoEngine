using System;
using System.Linq;
using OldBlingoEngine.Lingo.Core.Tokenizer;

namespace BlingoEngine.Lingo.Core;

public partial class CSharpWriter
{
    public void Visit(BlingoObjPropExprNode node)
    {
        if (node.Property is BlingoVarNode prop && prop.VarName.Equals("charToNum", StringComparison.OrdinalIgnoreCase))
        {
            node.Object.Accept(this);
            Append(".CharToNum()");
            return;
        }

        if (node.Object is BlingoMemberExprNode member &&
            node.Property is BlingoVarNode propVarText)
        {
            var propName = propVarText.VarName;
            if (propName.Equals("text", StringComparison.OrdinalIgnoreCase) ||
                propName.Equals("line", StringComparison.OrdinalIgnoreCase) ||
                propName.Equals("word", StringComparison.OrdinalIgnoreCase) ||
                propName.Equals("char", StringComparison.OrdinalIgnoreCase))
            {
                Append("GetMember<IBlingoMemberTextBase>(");
                member.Expr.Accept(this);
                if (member.CastLib != null)
                {
                    Append(", ");
                    member.CastLib.Accept(this);
                }
                Append(").");
                Append(char.ToUpperInvariant(propName[0]) + propName[1..]);
                return;
            }
        }

        if (node.Object is BlingoVarNode objVar &&
            objVar.VarName.Equals("me", StringComparison.OrdinalIgnoreCase) &&
            node.Property is BlingoVarNode propVar)
        {
            Append(char.ToUpperInvariant(propVar.VarName[0]) + propVar.VarName[1..]);
            return;
        }

        if (node.Object is BlingoTheExprNode theNode)
        {
            node.Object.Accept(this);
            Append(".");
            if (node.Property is BlingoVarNode propVar2)
            {
                var lower = propVar2.VarName.ToLowerInvariant();
                var pascal = lower switch
                {
                    "append" => "Add",
                    "deleteone" => "DeleteOne",
                    "getpos" => "GetPos",
                    "deleteat" => "DeleteAt",
                    "getat" => "GetAt",
                    "setat" => "SetAt",
                    "count" => "Count",
                    "add" => "Add",
                    "addat" => "AddAt",
                    "addprop" => "Add",
                    _ => char.ToUpperInvariant(propVar2.VarName[0]) + propVar2.VarName[1..]
                };
                Append(pascal);
            }
            else
            {
                node.Property.Accept(this);
            }
        }
        else
        {
            var start = _sb.Length;
            if (node.Object is BlingoCallNode call)
                WriteCallExpr(call);
            else if (node.Object is BlingoObjCallNode objCall)
                WriteObjCallExpr(objCall);
            else if (node.Object is BlingoObjCallV4Node v4)
                WriteObjCallV4Expr(v4);
            else
                node.Object.Accept(this);
            TrimSemicolon(start);

            Append(".");
            if (node.Property is BlingoVarNode propVar3)
            {
                var lower = propVar3.VarName.ToLowerInvariant();
                var pascal = lower switch
                {
                    "actorlist" => "ActorList",
                    "deleteone" => "DeleteOne",
                    "getpos" => "GetPos",
                    "deleteat" => "DeleteAt",
                    "getat" => "GetAt",
                    "setat" => "SetAt",
                    "count" => "Count",
                    "append" => "Add",
                    "add" => "Add",
                    "addat" => "AddAt",
                    "addprop" => "Add",
                    _ => char.ToUpperInvariant(propVar3.VarName[0]) + propVar3.VarName[1..]
                };
                Append(pascal);
            }
            else
            {
                node.Property.Accept(this);
            }
        }
    }

    public void Visit(BlingoThePropExprNode node)
    {
        Append("TheProp(");
        node.Property.Accept(this);
        Append(")");
    }

    public void Visit(BlingoMenuPropExprNode node)
    {
        Append("MenuProp(");
        node.Menu.Accept(this);
        Append(", ");
        node.Property.Accept(this);
        Append(")");
    }

    public void Visit(BlingoSoundPropExprNode node)
    {
        Append("Sound(");
        node.Sound.Accept(this);
        Append(").");
        node.Property.Accept(this);
    }

    public void Visit(BlingoSpritePropExprNode node)
    {
        Append("Sprite(");
        node.Sprite.Accept(this);
        Append(").");
        if (node.Property is BlingoVarNode propVar)
            Append(char.ToUpperInvariant(propVar.VarName[0]) + propVar.VarName[1..]);
        else
            node.Property.Accept(this);
    }

    public void Visit(BlingoMenuItemPropExprNode node)
    {
        Append("menuItem(");
        node.MenuItem.Accept(this);
        Append(").");
        node.Property.Accept(this);
    }

    public void Visit(BlingoObjPropIndexExprNode node)
    {
        node.Object.Accept(this);
        Append(".prop[");
        node.PropertyIndex.Accept(this);
        Append("]");
    }

    public void Visit(BlingoPropertyDeclStmtNode node) { }
}


