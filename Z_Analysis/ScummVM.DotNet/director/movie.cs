using Director.Graphics;
using Director.IO;
using Director.Primitives;
using Director.Scripts;
using Director.ScummVM;
using Director.Tools;

namespace Director
{
    [Flags]
    public enum MovieFlags : uint
    {
        AllowOutdatedLingo = 1 << 0,
        RemapPalettesWhenNeeded = 1 << 1
    }


    public class Movie
    {
        private bool _allowOutdatedLingo;
        public bool AllowOutdatedLingo => _allowOutdatedLingo;
        private bool _remapPalettesWhenNeeded;
        private string _script = "";
        private string _changedBy = "";
        private string _createdBy = "";
        private string _origDirectory = "";

        // Core state
        private ushort _version;
        private Platform _platform;

        private Rect _movieRect;
        private LingoColor _stageColor;
        private CastMemberID _defaultPalette;

        // Dependencies
        private Cast _cast;
        private Dictionary<int, Cast> _casts = new();
        private Archive _movieArchive;
        private Score _score;
        private DirectorEngine _vm;
        private StageWindow _window;

        // Script ID tracking
        private Dictionary<uint, ushort> _castsScriptIds = new();

        // Script archiving
        private LingoArchive _lingoArchive;



        public bool LoadArchive()
        {
            SeekableReadStreamEndian? r = null;

            // Config
            if (!_cast.LoadConfig())
                return false;

            _version = _cast.Version;
            _platform = _cast.Platform;
            _movieRect = _cast.MovieRect;
            _score.CurrentFrameRate = _cast.FrameRate;
            _stageColor = _vm.TransformColor(_cast.StageColor);
            // Wait to handle _stageColor until palette is loaded in loadCast...

            // File Info
            r = _movieArchive.GetMovieResourceIfPresent(ResourceTags.VWFI);
            if (r != null)
            {
                LoadFileInfo(r);
                r.Dispose();
            }

            // Casts
            foreach (var it in _casts)
            {
                if (it.Value != _cast)
                    it.Value.LoadConfig();
                it.Value.LoadCast();
            }

            _stageColor = _vm.TransformColor(_cast.StageColor);

            // Default palette fallback
            if (DirectorApp.Instance.HasPalette(_cast.DefaultPalette))
            {
                _defaultPalette = _cast.DefaultPalette;
            }
            else
            {
                _defaultPalette = new CastMemberID((int)SystemPaletteId.ClutSystemMac, -1);
            }

            DirectorApp.Instance.LastPalette = new CastMemberID();

            bool recenter = false;
            var surface = _window.GetSurface();
            if (surface.Width != _movieRect.Width || surface.Height != _movieRect.Height)
            {
                _window.ResizeInner(_movieRect.Width, _movieRect.Height);
                recenter = true;
            }

            //if (_window == _vm.Stage)
            //{
            //    ushort windowWidth = DirectorApp.Instance.DesktopEnabled ? DirectorApp.Instance.WmWidth : (ushort)_movieRect.Width;
            //    ushort windowHeight = DirectorApp.Instance.DesktopEnabled ? DirectorApp.Instance.WmHeight : (ushort)_movieRect.Height;

            //    if (_vm.WindowManager.ScreenDims.Width != windowWidth || _vm.WindowManager.ScreenDims.Height != windowHeight)
            //    {
            //        _vm.WindowManager.ResizeScreen(windowWidth, windowHeight);
            //        recenter = true;

            //        InitGraphics(windowWidth, windowHeight, ref _vm.PixelFormat);
            //    }
            //}

            if (recenter && DirectorApp.Instance.DesktopEnabled)
                _window.Center(DirectorApp.Instance.CenterStage);

            _window.SetStageColor(_stageColor, true);

            // Score
            r = _movieArchive.GetMovieResourceIfPresent(ResourceTags.VWSC);
            if (r == null)
            {
                LogHelper.DebugWarning("Movie::loadArchive(): No VWSC resource, injecting a blank score with 1 frame");
                if (_version < FileVersion.Ver400)
                {
                    r = new MemoryReadStreamEndian(DirConstants.BlankScoreD2, DirConstants.BlankScoreD2.Length, true);
                }
                else if (_version < FileVersion.Ver600)
                {
                    r = new MemoryReadStreamEndian(DirConstants.BlankScoreD4, DirConstants.BlankScoreD4.Length, true);
                }
                else
                {
                    throw new NotSupportedException($"Movie::loadArchive(): score format not yet supported for version {_version}");
                }
            }

            _score.LoadFrames(r, _version);
            r.Dispose();

            // Action list
            r = _movieArchive.GetMovieResourceIfPresent(ResourceTags.VWAC);
            if (r != null)
            {
                _score.LoadActions(r);
                r.Dispose();
            }

            return true;
        }


        public InfoEntries LoadInfoEntries(SeekableReadStreamEndian stream, ushort version)
        {
            uint offset = (uint)stream.Position;
            offset += stream.ReadUInt32();

            var res = new InfoEntries
            {
                Unk1 = stream.ReadUInt32(),
                Unk2 = stream.ReadUInt32(),
                Flags = stream.ReadUInt32()
            };

            if (version >= FileVersion.Ver400)
                res.ScriptId = stream.ReadUInt32();

            stream.Position = offset;
            ushort count = stream.ReadUInt16();
            count += 1;

            LogHelper.DebugLog(3, DebugChannel.Loading, $"Movie::loadInfoEntries(): InfoEntry: {count - 1} entries");

            if (count == 1)
                return res;

            uint[] entries = new uint[count];
            for (int i = 0; i < count; i++)
                entries[i] = stream.ReadUInt32();

            for (int i = 0; i < count - 1; i++)
            {
                int len = (int)(entries[i + 1] - entries[i]);
                byte[] data = stream.ReadBytes(len);

                res.Strings.Add(new InfoStringEntry
                {
                    Data = data
                });

                LogHelper.DebugLog(6, DebugChannel.Loading, $"InfoEntry {i}: {len} bytes");
            }

            return res;
        }

        public void LoadFileInfo(SeekableReadStreamEndian stream)
        {
            LogHelper.DebugLog(2, DebugChannel.Loading, "****** Loading FileInfo VWFI");

            var fileInfo = LoadInfoEntries(stream, _version);

            var flags = (MovieFlags)fileInfo.Flags;

            _allowOutdatedLingo = (flags & MovieFlags.AllowOutdatedLingo) != 0;
            _remapPalettesWhenNeeded = (flags & MovieFlags.RemapPalettesWhenNeeded) != 0;

            _script = fileInfo.Strings[0].ReadString(false);

            if (!string.IsNullOrEmpty(_script) && ConfMan.GetBool("dump_scripts"))
            {
                _cast.DumpScript(_script, ScriptType.Movie, 0);
            }

            if (!string.IsNullOrEmpty(_script))
            {
                _cast.LingoArchive.AddCode(_script, ScriptType.Movie, 0, null, ScriptFlags.TrimGarbage);
            }

            _changedBy = fileInfo.Strings[1].ReadString();
            _createdBy = fileInfo.Strings[2].ReadString();
            _origDirectory = fileInfo.Strings[3].ReadString();

            ushort preload = 0;
            if (fileInfo.Strings[4].Length > 0)
            {
                preload = stream.IsBigEndian
                    ? (ushort)((fileInfo.Strings[4].Data[0] << 8) | fileInfo.Strings[4].Data[1])
                    : (ushort)((fileInfo.Strings[4].Data[1] << 8) | fileInfo.Strings[4].Data[0]);
            }

            if (LogHelper.DebugChannelSet(3, DebugChannel.Loading))
            {
                LogHelper.DebugLog(3, DebugChannel.Loading, $"VWFI: flags: {fileInfo.Flags}");
                LogHelper.DebugLog(3, DebugChannel.Loading, $"VWFI: allow outdated lingo: {_allowOutdatedLingo}");
                LogHelper.DebugLog(3, DebugChannel.Loading, $"VWFI: script: '{_script}'");
                LogHelper.DebugLog(3, DebugChannel.Loading, $"VWFI: changed by: '{_changedBy}'");
                LogHelper.DebugLog(3, DebugChannel.Loading, $"VWFI: created by: '{_createdBy}'");
                LogHelper.DebugLog(3, DebugChannel.Loading, $"VWFI: original directory: '{_origDirectory}'");
                LogHelper.DebugLog(3, DebugChannel.Loading, $"VWFI: preload: {preload} (0x{preload:X})");

                for (int i = 5; i < fileInfo.Strings.Count; i++)
                {
                    LogHelper.DebugLog(3, DebugChannel.Loading, $"VWFI: entry {i} ({fileInfo.Strings[i].Length} bytes)");
                    LogHelper.DebugHexdump(fileInfo.Strings[i].Data, fileInfo.Strings[i].Length);
                }
            }
        }
        public static Rect ReadRect(SeekableReadStreamEndian stream)
        {
            int top = stream.ReadInt16();
            int left = stream.ReadInt16();
            int bottom = stream.ReadInt16();
            int right = stream.ReadInt16();
            return new Rect(left, top, right, bottom);
        }
    }

}

