using ProjectorRays.Common;
using ProjectorRays.Director;

if (args.Length == 0 || args[0] == "-h" || args[0] == "--help")
{
    System.Console.WriteLine("Usage: projector [input] [--dump-json OUTPUT]");
    return;
}

string inputPath = args[0];
string? jsonOutput = null;
for (int i = 1; i < args.Length; i++)
{
    if (args[i] == "--dump-json" && i + 1 < args.Length)
    {
        jsonOutput = args[i + 1];
        i++;
    }
}

byte[] data = System.IO.File.ReadAllBytes(inputPath);
var stream = new ReadStream(data, data.Length, Endianness.BigEndian);
var dir = new DirectorFile();
if (!dir.Read(stream))
{
    System.Console.WriteLine("Failed to read Director file");
    return;
}

if (jsonOutput != null)
{
    var writer = new JSONWriter("\n");
    writer.StartObject();
    writer.WriteField("version", dir.Version);
    writer.WriteField("casts", dir.Casts.Count);
    writer.EndObject();
    System.IO.File.WriteAllText(jsonOutput, writer.ToString());
}

System.Console.WriteLine($"Read Director file version {dir.Version}");

