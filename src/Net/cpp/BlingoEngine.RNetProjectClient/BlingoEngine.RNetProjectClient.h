#pragma once

#include <string>
#include <vector>
#include <variant>
#include <optional>
#include <functional>
#include <memory>

#include <signalrclient/hub_connection.h>
#include <signalrclient/hub_connection_builder.h>
#include <nlohmann/json.hpp>

// Data Transfer Objects ------------------------------------------------------

struct HelloDto {
    std::string ProjectId;
    std::string ClientId;
    std::string Version;
    std::string ClientName;
};
NLOHMANN_DEFINE_TYPE_NON_INTRUSIVE(HelloDto, ProjectId, ClientId, Version, ClientName);

struct StageFrameDto {
    int Width;
    int Height;
    long long FrameId;
    std::string TimestampUtc;
    std::vector<uint8_t> Argb32;
};
NLOHMANN_DEFINE_TYPE_NON_INTRUSIVE(StageFrameDto, Width, Height, FrameId, TimestampUtc, Argb32);

struct SpriteDeltaDto {
    int Frame;
    int SpriteNum;
    int BeginFrame;
    int Z;
    int CastLibNum;
    int MemberNum;
    int LocH;
    int LocV;
    int Width;
    int Height;
    int Rotation;
    int Skew;
    int Blend;
    int Ink;
};
NLOHMANN_DEFINE_TYPE_NON_INTRUSIVE(SpriteDeltaDto, Frame, SpriteNum, BeginFrame, Z, CastLibNum, MemberNum, LocH, LocV, Width, Height, Rotation, Skew, Blend, Ink);

struct RNetSpriteDto {
    int SpriteNum;
    int BeginFrame;
    int Z;
    int CastLibNum;
    int MemberNum;
    int LocH;
    int LocV;
    int Width;
    int Height;
    int Rotation;
    int Skew;
    int Blend;
    int Ink;
};
NLOHMANN_DEFINE_TYPE_NON_INTRUSIVE(RNetSpriteDto, SpriteNum, BeginFrame, Z, CastLibNum, MemberNum, LocH, LocV, Width, Height, Rotation, Skew, Blend, Ink);

struct KeyframeDto {
    int Frame;
    int SpriteNum;
    int BeginFrame;
    std::string Prop;
    std::string Value;
};
NLOHMANN_DEFINE_TYPE_NON_INTRUSIVE(KeyframeDto, Frame, SpriteNum, BeginFrame, Prop, Value);

struct FilmLoopDto {
    int CastLibNum;
    int MemberNum;
    int StartFrame;
    int EndFrame;
    bool IsPlaying;
};
NLOHMANN_DEFINE_TYPE_NON_INTRUSIVE(FilmLoopDto, CastLibNum, MemberNum, StartFrame, EndFrame, IsPlaying);

struct SoundEventDto {
    int Frame;
    int CastLibNum;
    int MemberNum;
    bool IsPlaying;
};
NLOHMANN_DEFINE_TYPE_NON_INTRUSIVE(SoundEventDto, Frame, CastLibNum, MemberNum, IsPlaying);

struct TempoDto {
    int Frame;
    int Tempo;
};
NLOHMANN_DEFINE_TYPE_NON_INTRUSIVE(TempoDto, Frame, Tempo);

struct ColorPaletteDto {
    int Frame;
    std::vector<uint8_t> Argb32;
};
NLOHMANN_DEFINE_TYPE_NON_INTRUSIVE(ColorPaletteDto, Frame, Argb32);

struct FrameScriptDto {
    int Frame;
    std::string Script;
};
NLOHMANN_DEFINE_TYPE_NON_INTRUSIVE(FrameScriptDto, Frame, Script);

struct TransitionDto {
    int Frame;
    std::string Type;
    int Duration;
};
NLOHMANN_DEFINE_TYPE_NON_INTRUSIVE(TransitionDto, Frame, Type, Duration);

struct RNetMemberPropertyDto {
    int CastLibNum;
    int MemberNum;
    std::string Prop;
    std::string Value;
};
NLOHMANN_DEFINE_TYPE_NON_INTRUSIVE(RNetMemberPropertyDto, CastLibNum, MemberNum, Prop, Value);

struct RNetMoviePropertyDto {
    std::string Prop;
    std::string Value;
};
NLOHMANN_DEFINE_TYPE_NON_INTRUSIVE(RNetMoviePropertyDto, Prop, Value);

struct RNetStagePropertyDto {
    std::string Prop;
    std::string Value;
};
NLOHMANN_DEFINE_TYPE_NON_INTRUSIVE(RNetStagePropertyDto, Prop, Value);

enum class RNetSpriteCollectionEventType { Added, Removed, Cleared };
NLOHMANN_JSON_SERIALIZE_ENUM(RNetSpriteCollectionEventType,
    {
        {RNetSpriteCollectionEventType::Added, "Added"},
        {RNetSpriteCollectionEventType::Removed, "Removed"},
        {RNetSpriteCollectionEventType::Cleared, "Cleared"},
    });

struct RNetSpriteCollectionEventDto {
    std::string Manager;
    RNetSpriteCollectionEventType Event;
    int SpriteNum;
    int BeginFrame;
    std::optional<RNetSpriteDto> Sprite;
};
NLOHMANN_DEFINE_TYPE_NON_INTRUSIVE(RNetSpriteCollectionEventDto, Manager, Event, SpriteNum, BeginFrame, Sprite);

struct TextStyleDto {
    int CastLibNum;
    int MemberNum;
    int Start;
    int End;
    std::string Style;
    std::string Value;
};
NLOHMANN_DEFINE_TYPE_NON_INTRUSIVE(TextStyleDto, CastLibNum, MemberNum, Start, End, Style, Value);

struct MovieStateDto {
    int Frame;
    int Tempo;
    bool IsPlaying;
};
NLOHMANN_DEFINE_TYPE_NON_INTRUSIVE(MovieStateDto, Frame, Tempo, IsPlaying);

struct LingoProjectJsonDto {
    std::string json;
};
NLOHMANN_DEFINE_TYPE_NON_INTRUSIVE(LingoProjectJsonDto, json);

// Debug command hierarchy -----------------------------------------------------

enum class RNetMemberTypeDto
{
    Unknown,
    Animgif,
    Ole,
    Bitmap,
    Palette,
    Button,
    Picture,
    Cursor,
    QuickTimeMedia,
    DigitalVideo,
    RealMedia,
    DVD,
    Script,
    Empty,
    Shape,
    Field,
    Shockwave3D,
    FilmLoop,
    Sound,
    Flash,
    Swa,
    Flashcomponent,
    Text,
    Font,
    Transition,
    Havok,
    VectorShape,
    Movie,
    WindowsMedia
};

enum class RNetSpriteTypeDto
{
    Unknown,
    Sprite2D,
    Tempo,
    ColorPalette,
    FrameScript,
    Transition,
    Sound,
};

struct SetSpritePropCmd {
    int SpriteNum;
    int BeginFrame;
    RNetSpriteTypeDto SpriteType;
    std::string Prop;
    std::string Value;
};
NLOHMANN_DEFINE_TYPE_NON_INTRUSIVE(SetSpritePropCmd, SpriteNum, BeginFrame, SpriteType, Prop, Value);

struct SetMemberPropCmd {
    int CastLibNum;
    int MemberNum;
    RNetMemberTypeDto MemberType;
    std::string Prop;
    std::string Value;
};
NLOHMANN_DEFINE_TYPE_NON_INTRUSIVE(SetMemberPropCmd, CastLibNum, MemberNum, MemberType, Prop, Value);

struct SetCastPropCmd {
    int CastLibNum;
    std::string Prop;
    std::string Value;
};
NLOHMANN_DEFINE_TYPE_NON_INTRUSIVE(SetCastPropCmd, CastLibNum, Prop, Value);

struct GoToFrameCmd {
    int Frame;
};
NLOHMANN_DEFINE_TYPE_NON_INTRUSIVE(GoToFrameCmd, Frame);

struct RewindCmd {};
inline void to_json(nlohmann::json& j, const RewindCmd&) { j = nlohmann::json::object(); }
inline void from_json(const nlohmann::json&, RewindCmd&) {}

struct PauseCmd {};
inline void to_json(nlohmann::json& j, const PauseCmd&) { j = nlohmann::json::object(); }
inline void from_json(const nlohmann::json&, PauseCmd&) {}

struct ResumeCmd {};
inline void to_json(nlohmann::json& j, const ResumeCmd&) { j = nlohmann::json::object(); }
inline void from_json(const nlohmann::json&, ResumeCmd&) {}

struct RNetCommand {
    std::variant<SetSpritePropCmd, SetMemberPropCmd, SetCastPropCmd, GoToFrameCmd, RewindCmd, PauseCmd, ResumeCmd> Command;
};
inline void to_json(nlohmann::json& j, const RNetCommand& cmd)
{
    std::visit([&j](const auto& v) { j = v; }, cmd.Command);
}

// RNetProjectClient ---------------------------------------------------------

class RNetProjectClient {
public:
    void Connect(const std::string& hubUrl, const HelloDto& hello);
    void Disconnect();

    void StreamFrames(std::function<void(const StageFrameDto&)> handler);
    void StreamDeltas(std::function<void(const SpriteDeltaDto&)> handler);
    void StreamKeyframes(std::function<void(const KeyframeDto&)> handler);
    void StreamFilmLoops(std::function<void(const FilmLoopDto&)> handler);
    void StreamSounds(std::function<void(const SoundEventDto&)> handler);
    void StreamTempos(std::function<void(const TempoDto&)> handler);
    void StreamColorPalettes(std::function<void(const ColorPaletteDto&)> handler);
    void StreamFrameScripts(std::function<void(const FrameScriptDto&)> handler);
    void StreamTransitions(std::function<void(const TransitionDto&)> handler);
    void StreamMemberProperties(std::function<void(const RNetMemberPropertyDto&)> handler);
    void StreamMovieProperties(std::function<void(const RNetMoviePropertyDto&)> handler);
    void StreamStageProperties(std::function<void(const RNetStagePropertyDto&)> handler);
    void StreamSpriteCollectionEvents(std::function<void(const RNetSpriteCollectionEventDto&)> handler);
    void StreamTextStyles(std::function<void(const TextStyleDto&)> handler);

    MovieStateDto GetMovieSnapshot();
    LingoProjectJsonDto GetCurrentProject();
    void SendCommand(const RNetCommand& cmd);
    void SendHeartbeat();

private:
    std::shared_ptr<signalr::hub_connection> _connection;
};

