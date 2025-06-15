using Director.IO;
using Director.Primitives;
using Director.Scripts;
using Director.Tools;
using System.Text;
using static Director.Scripts.Datum;

namespace Director.Members
{
    /// <summary>
    /// Identifiers for script cast member fields.
    /// </summary>
    public enum ScriptCastMemberField
    {
        ScriptType = 1
    }
    /// <summary>
    /// Represents a script cast member, which stores Lingo code and associated metadata.
    /// </summary>
    public class ScriptCastMember : CastMember
    {
        private ushort _scriptId;
        private ushort _nameId;

        /// <summary>
        /// The type of script (Cast, Movie, Score, etc.)
        /// </summary>
        public ScriptType ScriptType { get; set; } = ScriptType.Cast;

        /// <summary>
        /// The actual Lingo source code stored in the script.
        /// </summary>
        public string SourceCode { get; set; } = string.Empty;

        public ScriptCastMember(Cast cast, int castId)
            : base(cast, castId, CastType.Script)
        {
        }

        public ScriptCastMember(Cast cast, int castId, ScriptCastMember source)
            : base(cast, castId, CastType.Script)
        {
            ScriptType = source.ScriptType;
            SourceCode = source.SourceCode;
        }
        public ScriptCastMember(Cast cast, int castId, SeekableReadStreamEndian stream, ushort version)
       : base(cast, castId, stream)
        {
            _type = CastType.Script;
            ScriptType = ScriptType.None;

            if (LogHelper.DebugChannelSet(5, DebugChannel.Loading))
            {
                LogHelper.DebugLog(5, DebugChannel.Loading, "ScriptCastMember::ScriptCastMember(): Contents");
                LogHelper.DebugHexdump(stream.ReadAllBytes(), (int)stream.Length);
                stream.Seek(0); // Reset stream position after dump
            }

            if (version < FileVersion.Ver400)
            {
                throw new NotSupportedException("Unhandled Script cast");
            }
            else if (version >= FileVersion.Ver400 && version < FileVersion.Ver600)
            {
                byte unk1 = stream.ReadByte();
                byte type = stream.ReadByte();

                switch (type)
                {
                    case 1:
                        ScriptType = ScriptType.Score;
                        break;
                    case 3:
                        ScriptType = ScriptType.Movie;
                        break;
                    case 7:
                        ScriptType = ScriptType.Parent;
                        LogHelper.DebugWarning($"Unhandled kParentScript {castId}");
                        break;
                    default:
                        throw new InvalidOperationException($"ScriptCastMember: Unprocessed script type: {type}");
                }

                LogHelper.DebugLog(3, DebugChannel.Loading, $"CASt: Script type: {ScriptType} ({type}), unk1: {unk1}");

                if (stream.Position != stream.Length)
                    throw new InvalidOperationException("Unexpected data remaining in script stream");
            }
            else
            {
                LogHelper.DebugWarning($"STUB: ScriptCastMember: Scripts not yet supported for version {version}");
            }
        }
        public void Read(SeekableReadStreamEndian stream, uint size, int version)
        {
            long endPos = stream.Position + size;
            _scriptId = stream.ReadUInt16();
            _nameId = stream.ReadUInt16();
            stream.Seek(endPos, SeekOrigin.Begin);
        }


        /// <summary>
        /// Creates a duplicate of this cast member.
        /// </summary>
        public override CastMember Duplicate(Cast cast, int castId)
        {
            return new ScriptCastMember(cast, castId, this);
        }

        /// <summary>
        /// Loads the script data from a stream. Expected to parse the text and script metadata.
        /// </summary>
        public override void LoadFromStream(SeekableReadStreamEndian stream, Resource res)
        {
            byte[] data = stream.ReadBytesRequired((int)(stream.Length - stream.Position));
            SourceCode = Encoding.ASCII.GetString(data);
        }

        public override string GetText() => SourceCode;

        public override string FormatInfo() => $"ScriptCastMember: Type={ScriptType}, Length={SourceCode.Length}";

        /// <summary>
        /// Indicates whether a given field exists in this cast member.
        /// </summary>
        public override bool HasField(int field)
        {
            // Placeholder: implement actual field mapping if needed
            return field == 1; // e.g., only field 1 (source text) exists
        }

        /// <summary>
        /// Gets the value of a specified field.
        /// </summary>
        public override Datum GetField(int field)
        {
            switch ((ScriptCastMemberField)field)
            {
                case ScriptCastMemberField.ScriptType:
                    return ScriptType switch
                    {
                        ScriptType.Movie => Datum.Symbol("movie"),
                        ScriptType.Score => Datum.Symbol("score"),
                        ScriptType.Parent => Datum.Symbol("parent"),
                        _ => new Datum()
                    };

                default:
                    return base.GetField(field);
            }
        }

        /// <summary>
        /// Sets the value of a specified field.
        /// </summary>
        public override bool SetField(int field, Datum d)
        {
            switch ((ScriptCastMemberField)field)
            {
                case ScriptCastMemberField.ScriptType:
                    LogHelper.DebugWarning("ScriptCastMember.SetField(): setting ScriptType! This may not recategorize the script properly.");
                    if (d.Type == DatumType.Symbol && d.StringValue != null)
                    {
                        var symbol = d.StringValue.ToLowerInvariant();
                        if (symbol == "movie")
                            ScriptType = ScriptType.Movie;
                        else if (symbol == "score")
                            ScriptType = ScriptType.Score;
                        else if (symbol == "parent")
                            ScriptType = ScriptType.Parent;
                    }
                    return true;

                default:
                    return base.SetField(field, d);
            }
        }
    }
}
