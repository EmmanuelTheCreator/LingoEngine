namespace ProjectorRays.Common;

public class JSONWriter : CodeWriter
{
    private enum Context
    {
        Start,
        OpenBrace,
        Key,
        Value
    }

    private Context _context = Context.Start;

    public JSONWriter(string lineEnding, string indentation = "  ")
        : base(lineEnding, indentation)
    {
    }

    public void StartObject()
    {
        WriteValuePrefix();
        Write("{");
        Indent();
        _context = Context.OpenBrace;
    }

    public void WriteKey(string key)
    {
        WriteValuePrefix();
        WriteString(key);
        Write(": ");
        _context = Context.Key;
    }

    public void EndObject()
    {
        WriteCloseBracePrefix();
        Unindent();
        Write("}");
        _context = Context.Value;
        WriteValueSuffix();
    }

    public void StartArray()
    {
        WriteValuePrefix();
        Write("[");
        Indent();
        _context = Context.OpenBrace;
    }

    public void EndArray()
    {
        WriteCloseBracePrefix();
        Unindent();
        Write("]");
        _context = Context.Value;
        WriteValueSuffix();
    }

    public void WriteVal(uint val)
    {
        WriteValuePrefix();
        Write(val.ToString());
        _context = Context.Value;
        WriteValueSuffix();
    }

    public void WriteVal(int val)
    {
        WriteValuePrefix();
        Write(val.ToString());
        _context = Context.Value;
        WriteValueSuffix();
    }

    public void WriteVal(double val)
    {
        WriteValuePrefix();
        Write(Util.FloatToString(val));
        _context = Context.Value;
        WriteValueSuffix();
    }

    public void WriteVal(string val)
    {
        WriteValuePrefix();
        WriteString(val);
        _context = Context.Value;
        WriteValueSuffix();
    }

    public void WriteNull()
    {
        WriteValuePrefix();
        Write("null");
        _context = Context.Value;
        WriteValueSuffix();
    }

    public void WriteFourCC(uint val)
    {
        WriteValuePrefix();
        Write("\"");
        Write(Util.FourCCToString(val));
        Write("\"");
        _context = Context.Value;
        WriteValueSuffix();
    }

    public void WriteField(string key, uint val) { WriteKey(key); WriteVal(val); }
    public void WriteField(string key, int val) { WriteKey(key); WriteVal(val); }
    public void WriteField(string key, double val) { WriteKey(key); WriteVal(val); }
    public void WriteField(string key, string val) { WriteKey(key); WriteVal(val); }
    public void WriteNullField(string key) { WriteKey(key); WriteNull(); }
    public void WriteFourCCField(string key, uint val) { WriteKey(key); WriteFourCC(val); }

    public new string ToString() => base.ToString();

    private void WriteString(string str)
    {
        Write("\"");
        Write(Util.EscapeString(str));
        Write("\"");
    }

    private void WriteValuePrefix()
    {
        if (_context == Context.Value)
            Write(",");
        if (_context == Context.Value || _context == Context.OpenBrace)
            WriteLine();
    }

    private void WriteValueSuffix()
    {
        if (_indentationLevel == 0)
            WriteLine();
    }

    private void WriteCloseBracePrefix()
    {
        if (_context == Context.Value)
            WriteLine();
    }
}
