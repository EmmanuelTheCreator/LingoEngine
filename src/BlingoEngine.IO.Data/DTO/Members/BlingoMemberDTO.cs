using System.Text.Json.Serialization;
using BlingoEngine.IO.Data.DTO.FilmLoops;
using BlingoEngine.IO.Data.DTO.Sprites;

namespace BlingoEngine.IO.Data.DTO.Members;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(BlingoMemberBitmapDTO), nameof(BlingoMemberBitmapDTO))]
[JsonDerivedType(typeof(BlingoMemberFieldDTO), nameof(BlingoMemberFieldDTO))]
[JsonDerivedType(typeof(BlingoMemberShapeDTO), nameof(BlingoMemberShapeDTO))]
[JsonDerivedType(typeof(BlingoMemberSoundDTO), nameof(BlingoMemberSoundDTO))]
[JsonDerivedType(typeof(BlingoMemberTextDTO), nameof(BlingoMemberTextDTO))]
[JsonDerivedType(typeof(BlingoMemberFilmLoopDTO), nameof(BlingoMemberFilmLoopDTO))]
[JsonDerivedType(typeof(BlingoSpriteBehaviorDTO), nameof(BlingoSpriteBehaviorDTO))]
public class BlingoMemberDTO
{
    public string Name { get; set; } = string.Empty;
    public int CastLibNum { get; set; }
    public int NumberInCast { get; set; }
    public BlingoMemberTypeDTO Type { get; set; }
    public BlingoPointDTO RegPoint { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public long Size { get; set; }
    public string Comments { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public int PurgePriority { get; set; }
}
