using Director.Fonts;
using Director.IO;

namespace Director.Members
{
    public class CastMemberInfo
    {
        public bool AutoHilite { get; set; }
        public uint ScriptId { get; set; }
        public string Script { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Directory { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public CastType Type { get; set; } 
        public EditInfo ScriptEditInfo { get; set; } = new EditInfo();
        public FontStyle ScriptStyle { get; set; } = new FontStyle();
        public EditInfo TextEditInfo { get; set; } = new EditInfo();
        public string ModifiedBy { get; set; } = string.Empty;
        public string Comments { get; set; } = string.Empty;
        public string LinkedPath { get; set; } = string.Empty;
        public string VideoPath { get; set; } = string.Empty;

        public CastMemberInfo()
        {
            AutoHilite = false;
            ScriptId = 0;
        }
        public void LoadScriptMetadata(SeekableReadStreamEndian stream, Cast cast)
        {
            ScriptId = stream.ReadUInt32();
            AutoHilite = stream.ReadByte() != 0;
            Script = stream.ReadPascalString();        // Usually the actual script text
            Name = stream.ReadPascalString();          // Script or cast member name
            Directory = stream.ReadPascalString();     // Possibly used for linked media
            FileName = stream.ReadPascalString();      // Script file name or external reference
            ModifiedBy = stream.ReadPascalString();    // Author/editor name
            Comments = stream.ReadPascalString();      // Editor notes or comments

            // These two might be optional or version-specific:
            LinkedPath = stream.ReadPascalString();
            VideoPath = stream.ReadPascalString();

            // If EditInfo and FontStyle structures follow:
            ScriptEditInfo = new EditInfo
            {
                Rect = stream.ReadRect(),
                SelStart = stream.ReadInt32(),
                SelEnd = stream.ReadInt32(),
                Version = stream.ReadByte(),
                RulerFlag = stream.ReadByte()
            };

            ScriptStyle.Read(stream, cast: cast); // Pass the actual Cast if mapping is needed

            TextEditInfo = new EditInfo
            {
                Rect = stream.ReadRect(),
                SelStart = stream.ReadInt32(),
                SelEnd = stream.ReadInt32(),
                Version = stream.ReadByte(),
                RulerFlag = stream.ReadByte()
            };
        }
        public void LoadScriptMetadataV4(SeekableReadStreamEndian stream, Cast cast)
        {
            // Lingo bytecode has already been read before calling this

            Script = stream.ReadPascalString();        // Actual script source (not bytecode)
            Name = stream.ReadPascalString();          // Name of the script
            Directory = stream.ReadPascalString();     // Likely unused
            FileName = stream.ReadPascalString();      // External source filename
            ModifiedBy = stream.ReadPascalString();    // Editor's name
            Comments = stream.ReadPascalString();      // Comment or notes field

            LinkedPath = stream.ReadPascalString();    // Optional path to linked resource
            VideoPath = stream.ReadPascalString();     // Optional video path

            ScriptEditInfo = new EditInfo
            {
                Rect = stream.ReadRect(),
                SelStart = stream.ReadInt32(),
                SelEnd = stream.ReadInt32(),
                Version = stream.ReadByte(),
                RulerFlag = stream.ReadByte()
            };

            ScriptStyle.Read(stream, cast);

            TextEditInfo = new EditInfo
            {
                Rect = stream.ReadRect(),
                SelStart = stream.ReadInt32(),
                SelEnd = stream.ReadInt32(),
                Version = stream.ReadByte(),
                RulerFlag = stream.ReadByte()
            };
        }
    }

}
