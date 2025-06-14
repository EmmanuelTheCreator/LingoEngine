using Director.IO;
using Director.Primitives;
using System.IO;

namespace Director.Members
{
    public class RichTextCastMember : CastMember
    {
        public string Text { get; private set; } = string.Empty;
        public uint ForeColor { get; private set; }
        public uint BackColor { get; private set; }

        public RichTextCastMember(Cast cast, int castId)
            : base(cast, castId, CastType.RichText) { }

        public RichTextCastMember(Cast cast, int castId, SeekableReadStreamEndian stream)
            : base(cast, castId, stream)
        {
        }

        public RichTextCastMember(Cast cast, int castId, RichTextCastMember source)
            : base(cast, castId, CastType.RichText)
        {
            source.Load();
            _loaded = true;
            _initialRect = source._initialRect;
            _boundingRect = source._boundingRect;
            if (cast == source._cast)
                _children = source._children;

            Text = source.Text;
            ForeColor = source.ForeColor;
            BackColor = source.BackColor;
        }

        public override CastMember Duplicate(Cast cast, int castId)
        {
            return new RichTextCastMember(cast, castId, this);
        }

        public override void Load()
        {
            if (_loaded)
                return;

            int rte1Id = 0;
            foreach (var child in _children)
            {
                if (child.Tag == ResourceTags.MKTAG('R','T','E','1'))
                {
                    rte1Id = child.Index;
                    break;
                }
            }

            if (rte1Id != 0)
            {
                var arch = _cast.GetArchive();
                if (arch.HasResource(ResourceTags.MKTAG('R','T','E','1'), rte1Id))
                {
                    using var rte1 = arch.GetResource(ResourceTags.MKTAG('R','T','E','1'), rte1Id);
                    using var br = new BinaryReader(rte1.BaseStream, System.Text.Encoding.Default, leaveOpen:true);
                    var parsed = RTE1.ReadFrom(br);
                    Text = System.Text.Encoding.ASCII.GetString(parsed.RawData);
                }
            }

            _loaded = true;
        }

        public override string FormatInfo()
        {
            return $"text:{Text}";
        }

        public override void Unload()
        {
            Text = string.Empty;
            _loaded = false;
        }
    }
}
