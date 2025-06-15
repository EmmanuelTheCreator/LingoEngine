using Microsoft.VisualBasic;
using System.Reflection.Metadata.Ecma335;

namespace Director.Members
{

    public static class CastMemberFactory
    {
        public static CastMember Create(CastType type, Cast parent, int id)
        {
            return type switch
            {
                CastType.Bitmap => new BitmapCastMember(parent, id),
                CastType.Text => new TextCastMember(parent, id),
                CastType.Shape => new ShapeCastMember(parent, id),
                CastType.FilmLoop => new FilmLoopCastMember(parent, id),
                CastType.Movie => new MovieCastMember(parent, id),
                CastType.Sound => new SoundCastMember(parent, id),
                CastType.Palette => new PaletteCastMember(parent, id),
                CastType.RichText => new RichTextCastMember(parent, id),
                CastType.Script => new ScriptCastMember(parent, id),
                _ => new CastMember(parent, parent.CastLibId, CastType.Any)
            };
        }
    }

}
