namespace ProjectorRays;

using ProjectorRays.Common;
using ProjectorRays.IO;
using ProjectorRays.Director;
using ProjectorRays.LingoDec;

public static class Program
{
    public static void Main(string[] args)
    {
        // Demonstrate JSON writer and streams
        var writer = new JSONWriter("\n");
        writer.StartObject();
        writer.WriteField("message", "Hello from ProjectorRays");

        // simple stream demo
        byte[] buffer = new byte[16];
        var ws = new WriteStream(buffer, buffer.Length, Endianness.BigEndian);
        ws.WriteUint16(0x1234);
        ws.WriteString("Hi");

        var rs = new ReadStream(buffer, buffer.Length, Endianness.BigEndian);
        uint val = rs.ReadUint16();
        string str = rs.ReadString(2);

        writer.WriteField("value", val);
        writer.WriteField("text", str);
        writer.WriteField("humanVersion", Util.HumanVersion(1922));

        // demo bytecode decompilation
        var handler = new Handler(new Script(500));
        handler.BytecodeArray.Add(new Bytecode((byte)OpCode.kOpPushInt32, 42, 0));
        handler.BytecodeArray.Add(new Bytecode((byte)OpCode.kOpPushInt32, 100, 1));
        handler.BytecodeArray.Add(new Bytecode((byte)OpCode.kOpAdd, 0, 2));
        var cw = new CodeWriter("\n");
        handler.WriteBytecodeText(cw, false);
        writer.WriteField("bytecode", cw.ToString().Trim());
        writer.EndObject();

        string output = writer.ToString();
        System.Console.WriteLine(output);
        FileIO.WriteFile("output.json", output);
    }
}
