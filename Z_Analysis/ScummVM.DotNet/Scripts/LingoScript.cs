using Director.IO;
using Director.Primitives;
using Director.Scripts.Lingo;

namespace Director.Scripts
{

    public class LingoScript
    {
        // Raw header values
        public uint TotalLength { get; set; }
        public uint TotalLength2 { get; set; }
        public ushort HeaderLength { get; set; }
        public ushort ScriptNumber { get; set; }
        public short Unknown20 { get; set; }
        public short ParentNumber { get; set; }

        // Script metadata
        public ScriptFlags Flags { get; set; }
        public short Unknown42 { get; set; }
        public int CastId { get; set; }
        public short FactoryNameId { get; set; }
        public ushort HandlerVectorsCount { get; set; }
        public uint HandlerVectorsOffset { get; set; }
        public uint HandlerVectorsSize { get; set; }

        public ushort PropertiesCount { get; set; }
        public uint PropertiesOffset { get; set; }

        public ushort GlobalsCount { get; set; }
        public uint GlobalsOffset { get; set; }

        public ushort HandlersCount { get; set; }
        public uint HandlersOffset { get; set; }

        public ushort LiteralsCount { get; set; }
        public uint LiteralsOffset { get; set; }

        public uint LiteralsDataCount { get; set; }
        public uint LiteralsDataOffset { get; set; }

        // Contextual resolution
        public List<short> PropertyNameIds { get; set; } = new();
        public List<short> GlobalNameIds { get; set; } = new();

        public string FactoryName { get; set; } = string.Empty;
        public List<string> PropertyNames { get; set; } = new();
        public List<string> GlobalNames { get; set; } = new();
        //public List<LingoHandler> Handlers { get; set; } = new();
        //public List<LingoLiteral> Literals { get; set; } = new();
        public List<LingoScript> Factories { get; set; } = new();
        public List<Handler> Handlers { get; set; } = new();

        public ushort Version { get; }
        public LingoScriptContext? Context { get; private set; }
        public int? ContextId { get; set; } // optional: not part of original struct

        public bool IsFactory => PropertyNames.Count > 0 || (Handlers.Count > 0 && Handlers[0].IsMethod);
        public LingoScript(ushort version = 0)
        {
            Version = version;
        }
        

        public void SetContext(LingoScriptContext context)
        {
            //todo
            Context = context;
        }


        public int Id { get; set; }
        public ScriptType Type { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;



        public void Read(SeekableReadStreamEndian stream)
        {
            stream.Seek(8);
            TotalLength = stream.ReadUInt32BE();
            TotalLength2 = stream.ReadUInt32BE();
            HeaderLength = stream.ReadUInt16BE();
            ScriptNumber = stream.ReadUInt16BE();
            Unknown20 = stream.ReadInt16BE();
            ParentNumber = stream.ReadInt16BE();

            stream.Seek(38);
            Flags = (ScriptFlags)stream.ReadUInt32BE();
            Unknown42 = stream.ReadInt16BE();
            CastId = stream.ReadInt32BE();
            FactoryNameId = stream.ReadInt16BE();
            HandlerVectorsCount = stream.ReadUInt16BE();
            HandlerVectorsOffset = stream.ReadUInt32BE();
            HandlerVectorsSize = stream.ReadUInt32BE();
            PropertiesCount = stream.ReadUInt16BE();
            PropertiesOffset = stream.ReadUInt32BE();
            GlobalsCount = stream.ReadUInt16BE();
            GlobalsOffset = stream.ReadUInt32BE();
            HandlersCount = stream.ReadUInt16BE();
            HandlersOffset = stream.ReadUInt32BE();
            LiteralsCount = stream.ReadUInt16BE();
            LiteralsOffset = stream.ReadUInt32BE();
            LiteralsDataCount = stream.ReadUInt32BE();
            LiteralsDataOffset = stream.ReadUInt32BE();

            PropertyNameIds = ReadVarnameTable(stream, PropertiesCount, PropertiesOffset);
            GlobalNameIds = ReadVarnameTable(stream, GlobalsCount, GlobalsOffset);

            //Handlers = Enumerable.Range(0, HandlersCount)
            //    .Select(_ => new LingoHandler(this)).ToList();

            //if (Flags.HasFlag(ScriptFlags.EventScript) && HandlersCount > 0)
            //    Handlers[0].IsGenericEvent = true;

            //stream.Seek(HandlersOffset);
            //foreach (var handler in Handlers)
            //    handler.ReadRecord(stream);
            //foreach (var handler in Handlers)
            //    handler.ReadData(stream);

            stream.Seek(LiteralsOffset);
            //Literals = Enumerable.Range(0, LiteralsCount)
            //    .Select(_ => new LingoLiteral(this.Version)).ToList();

            //foreach (var lit in Literals)
            //    lit.ReadRecord(stream);
            //foreach (var lit in Literals)
            //    lit.ReadData(stream, LiteralsDataOffset);
        }

        private List<short> ReadVarnameTable(SeekableReadStreamEndian stream, ushort count, uint offset)
        {
            stream.Seek(offset);
            var list = new List<short>(count);
            for (int i = 0; i < count; i++)
                list.Add(stream.ReadInt16BE());
            return list;
        }

        public string ScriptText(string lineEnding = "\n", bool dotSyntax = true)
        {
            var writer = new CodeWriterVisitor(dotSyntax, false, lineEnding);
            WriteScriptText(writer);
            return writer.ToString();
        }
        public void WriteScriptText(CodeWriterVisitor writer)
        {
            var origSize = writer.Length;

            // Write property declarations
            if (!IsFactory && PropertyNames.Count > 0)
            {
                writer.Write("property ");
                for (int i = 0; i < PropertyNames.Count; i++)
                {
                    if (i > 0)
                        writer.Write(", ");
                    writer.Write(PropertyNames[i]);
                }
                writer.WriteLine();
            }

            // Write global declarations
            if (GlobalNames.Count > 0)
            {
                writer.Write("global ");
                for (int i = 0; i < GlobalNames.Count; i++)
                {
                    if (i > 0)
                        writer.Write(", ");
                    writer.Write(GlobalNames[i]);
                }
                writer.WriteLine();
            }

            // If it's a factory, write the factory line
            if (IsFactory)
            {
                if (writer.Length != origSize)
                    writer.WriteLine();

                writer.Write("factory ");
                writer.WriteLine(FactoryName);
            }

            // Write handlers
            for (int i = 0; i < Handlers.Count; i++)
            {
                if ((!IsFactory || i > 0) && writer.Length != origSize)
                    writer.WriteLine();

                Handlers[i].Ast?.Root?.Accept(writer);
            }

            // Recursively write factories
            foreach (var factory in Factories)
            {
                if (writer.Length != origSize)
                    writer.WriteLine();

                factory.WriteScriptText(writer);
            }
        }

        public void Parse()
        {
            foreach (var handler in Handlers)
                handler.Parse();
        }
        public void SetOnlyInLctxContexts()
        {
            // Marker method – can be a flag or logic depending on archive behavior
        }

    }
}

