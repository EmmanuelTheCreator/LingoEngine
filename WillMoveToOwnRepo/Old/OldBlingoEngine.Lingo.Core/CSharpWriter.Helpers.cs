using System;
using System.Collections.Generic;
using System.Linq;
using OldBlingoEngine.Lingo.Core.Tokenizer;

namespace BlingoEngine.Lingo.Core;

public partial class CSharpWriter
{
    private static string DatumToCSharp(BlingoDatum datum)
    {
        return datum.Type switch
        {
            BlingoDatum.DatumType.Integer or BlingoDatum.DatumType.Float => datum.AsString(),
            BlingoDatum.DatumType.String => $"\"{EscapeString(datum.AsString())}\"",
            BlingoDatum.DatumType.Symbol => $"Symbol(\"{datum.AsString()}\")",
            BlingoDatum.DatumType.VarRef => datum.AsString(),
            BlingoDatum.DatumType.List => datum.Value is List<BlingoNode> list
                ? "[" + string.Join(", ", list.Select(n => Write(n).Trim())) + "]"
                : "[]",
            BlingoDatum.DatumType.ArgList or BlingoDatum.DatumType.ArgListNoRet => datum.Value is List<BlingoNode> argList
                ? string.Join(", ", argList.Select(n => Write(n).Trim()))
                : string.Empty,
            BlingoDatum.DatumType.PropList => datum.Value is List<BlingoNode> propList
                ? "new BlingoPropertyList<object?>" +
                    (propList.Count == 0
                        ? "()"
                        : " { " + string.Join(", ", Enumerable.Range(0, propList.Count / 2)
                            .Select(i => $"[{Write(propList[2 * i]).Trim()}] = {Write(propList[2 * i + 1]).Trim()}")) + " }")
                : "new BlingoPropertyList<object?>()",
            _ => datum.AsString()
        };
    }

    private static string EscapeString(string s) => s
        .Replace("\\", "\\\\")
        .Replace("\"", "\\\"")
        .Replace("\n", "\\n")
        .Replace("\r", "\\r")
        .Replace("\t", "\\t")
        .Replace("\b", "\\b")
        .Replace("\u0003", "\\u0003");
}


