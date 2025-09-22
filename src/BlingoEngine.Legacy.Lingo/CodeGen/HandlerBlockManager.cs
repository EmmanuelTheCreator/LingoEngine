using System;
using System.Collections.Generic;

namespace BlingoEngine.Legacy.Lingo.CodeGen;

public sealed class HandlerBlockManager
{
    private readonly BlCSharpCodeWriter _writer;
    private readonly Stack<HandlerBlockFrame> _frames = new();

    public HandlerBlockManager(BlCSharpCodeWriter writer)
    {
        _writer = writer ?? throw new ArgumentNullException(nameof(writer));
    }

    public HandlerBlockFrame? Current => _frames.Count > 0 ? _frames.Peek() : null;

    public bool IsCurrent(BlockKind kind) => _frames.Count > 0 && _frames.Peek().Kind == kind;

    public void OpenBlock(BlockKind kind, string? condition = null, bool reopenExisting = false)
    {
        if (reopenExisting && _frames.Count > 0)
        {
            _writer.WriteLine("{");
            _writer.Indent();
            return;
        }

        _writer.WriteLine("{");
        _writer.Indent();
        _frames.Push(new HandlerBlockFrame(kind, condition));
    }

    public void CloseBlock(bool leaveOnStack = false)
    {
        if (_frames.Count == 0)
        {
            return;
        }

        var frame = _frames.Peek();
        if (frame.Kind == BlockKind.Switch)
        {
            CloseActiveCase();
        }

        _writer.Unindent();
        if (frame.Kind == BlockKind.RepeatUntil)
        {
            var condition = frame.Condition ?? "false";
            _writer.WriteLine($"}} while (!({condition}));");
        }
        else
        {
            _writer.WriteLine("}");
        }

        if (!leaveOnStack)
        {
            _frames.Pop();
        }
    }

    public void CloseAll()
    {
        while (_frames.Count > 0)
        {
            CloseBlock();
        }
    }

    public void CloseActiveCase()
    {
        if (_frames.Count == 0)
        {
            return;
        }

        var frame = _frames.Peek();
        if (!frame.CaseOpen)
        {
            return;
        }

        frame.CaseOpen = false;
        _writer.WriteLine("break;");
        _writer.Unindent();
    }

    public void StartSwitchSection()
    {
        if (!IsCurrent(BlockKind.Switch))
        {
            return;
        }

        CloseActiveCase();
        var frame = _frames.Peek();
        _writer.Indent();
        frame.CaseOpen = true;
    }
}
