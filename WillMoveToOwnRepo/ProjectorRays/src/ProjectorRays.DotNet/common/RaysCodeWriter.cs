using System.Text;

namespace ProjectorRays.Common;

public class RaysCodeWriter
{
    protected StringBuilder _stream = new();
    protected readonly string _lineEnding;
    protected readonly string _indentation;
    protected int _indentationLevel = 0;
    protected bool _indentationWritten = false;
    protected int _lineWidth = 0;
    protected int _size = 0;

    public bool DoIndentation = true;

    public RaysCodeWriter(string lineEnding, string indentation = "  ")
    {
        _lineEnding = lineEnding;
        _indentation = indentation;
    }

    public void Write(string str)
    {
        if (string.IsNullOrEmpty(str))
            return;
        WriteIndentation();
        _stream.Append(str);
        _lineWidth += str.Length;
        _size += str.Length;
    }

    public void Write(char ch)
    {
        WriteIndentation();
        _stream.Append(ch);
        _lineWidth += 1;
        _size += 1;
    }

    public void WriteLine(string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            _stream.Append(_lineEnding);
        }
        else
        {
            WriteIndentation();
            _stream.Append(str).Append(_lineEnding);
        }
        _indentationWritten = false;
        _lineWidth = 0;
        _size += str.Length + _lineEnding.Length;
    }

    public void WriteLine() => WriteLine(string.Empty);

    public void Indent() => _indentationLevel++;

    public void Unindent()
    {
        if (_indentationLevel > 0)
            _indentationLevel--;
    }

    public override string ToString() => _stream.ToString();
    public int LineWidth => _lineWidth;
    public int Size => _size;

    protected void WriteIndentation()
    {
        if (_indentationWritten || !DoIndentation)
            return;
        for (int i = 0; i < _indentationLevel; i++)
            _stream.Append(_indentation);
        _indentationWritten = true;
        _lineWidth = _indentationLevel * _indentation.Length;
        _size += _lineWidth;
    }
}
