

using Director.Primitives;

namespace Director.Members
{

    public class PaletteCastMember : CastMember
        {
            private PaletteV4? _palette;

            public PaletteCastMember(Cast cast, int castId)
                : base(cast, castId, CastType.Palette)
            {
                _palette = null;
            }

            public PaletteCastMember(Cast cast, int castId, PaletteCastMember source)
                : base(cast, castId, CastType.Palette)
            {
                source.Load();
                _loaded = true;
                _palette = source._palette != null ? new PaletteV4(source._palette) : null;
            }

            public CastMemberID GetPaletteId()
            {
                Load();
                return _palette != null ? _palette.Id : new CastMemberID();
            }

            public void ActivatePalette()
            {
                Load();
                if (_palette != null)
                    DirectorApp.Instance.SetPalette(_palette.Id);
            }

        public override string FormatInfo()
        {
            if (_palette == null) return string.Empty;
            return "data: " + string.Join("", _palette.ToHexArray());
        }

        public override void Load()
            {
                if (_loaded)
                    return;

                int paletteId = 0;
                if (_cast.Version < 400)
                {
                    paletteId = _castId + _cast.CastIDOffset;
                }
                else if (_cast.Version < 600)
                {
                    foreach (var child in _children)
                    {
                        if (child.Tag == ResourceTags.CLUT)
                        {
                            paletteId = child.Index;
                            break;
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"STUB: PaletteCastMember.Load(): Palettes not yet supported for version {_cast.Version}");
                }

                if (paletteId != 0)
                {
                    var tag = ResourceTags.CLUT;
                    var arch = _cast.GetArchive();
                    if (arch.HasResource(tag, paletteId))
                    {
                        using var pal = arch.GetResource(tag, paletteId);
                        var palData = _cast.LoadPalette(pal, paletteId);
                        palData.Id = new CastMemberID(_castId, _cast.CastLibID);
                        DirectorApp.Instance.AddPalette(palData.Id, palData.Colors, palData.Length);
                        _palette = new PaletteV4(palData);
                    }
                    else
                    {
                        Console.WriteLine($"PaletteCastMember.Load(): no CLUT palette {paletteId} for cast index {_castId} found");
                    }
                }

                _loaded = true;
            }

            public override void Unload()
            {
                // Nothing to unload
            }
        }

    }


