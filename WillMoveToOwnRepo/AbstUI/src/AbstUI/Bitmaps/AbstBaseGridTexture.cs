using System;
using System.Collections.Generic;
using AbstUI.Primitives;

namespace AbstUI.Bitmaps;

public abstract class AbstBaseGridTexture<TFrameworkTexture> : AbstBaseTexture2D<TFrameworkTexture>
{
    private static readonly AbstBaseTexture2D<TFrameworkTexture>[] EmptyTiles = Array.Empty<AbstBaseTexture2D<TFrameworkTexture>>();
    private static readonly (int X, int Y)[] EmptyCoordinates = Array.Empty<(int X, int Y)>();

    private readonly Dictionary<(int X, int Y), AbstBaseTexture2D<TFrameworkTexture>>? _tileMap;
    private readonly List<AbstBaseTexture2D<TFrameworkTexture>>? _tileList;
    private readonly HashSet<(int X, int Y)>? _dirtyTiles;

    public IReadOnlyList<AbstBaseTexture2D<TFrameworkTexture>> TileTextures =>
        _tileList as IReadOnlyList<AbstBaseTexture2D<TFrameworkTexture>> ?? EmptyTiles;

    public IReadOnlyCollection<(int X, int Y)> DirtyTiles =>
        _dirtyTiles as IReadOnlyCollection<(int X, int Y)> ?? EmptyCoordinates;

    public bool HasDirtyTiles => _dirtyTiles != null && _dirtyTiles.Count > 0;

    protected virtual int TileSize => 0;

    protected AbstBaseGridTexture(string name = "", bool manageTiles = true)
        : base(name)
    {
        if (manageTiles)
        {
            _tileMap = new Dictionary<(int X, int Y), AbstBaseTexture2D<TFrameworkTexture>>();
            _tileList = new List<AbstBaseTexture2D<TFrameworkTexture>>();
            _dirtyTiles = new HashSet<(int X, int Y)>();
        }
    }

    protected Dictionary<(int X, int Y), AbstBaseTexture2D<TFrameworkTexture>> TileMap =>
        _tileMap ?? throw new InvalidOperationException("Tile management is disabled for this texture.");

    protected List<AbstBaseTexture2D<TFrameworkTexture>> TileList =>
        _tileList ?? throw new InvalidOperationException("Tile management is disabled for this texture.");

    protected HashSet<(int X, int Y)> DirtyTileSet =>
        _dirtyTiles ?? throw new InvalidOperationException("Tile management is disabled for this texture.");

    protected abstract AbstBaseTexture2D<TFrameworkTexture> CreateTile((int X, int Y) coordinates, int width, int height);

    protected virtual void ResizeTile((int X, int Y) coordinates, AbstBaseTexture2D<TFrameworkTexture> tile, int width, int height)
    {
        throw new NotSupportedException($"{GetType().Name} must override ResizeTile to support dynamic tile sizing.");
    }

    protected virtual void OnTileRemoved((int X, int Y) coordinates, AbstBaseTexture2D<TFrameworkTexture> tile)
    {
        tile.Dispose();
    }

    protected AbstBaseTexture2D<TFrameworkTexture> GetOrCreateTile((int X, int Y) coordinates, int width, int height)
    {
        if (_tileMap == null)
            throw new InvalidOperationException("Tile management is disabled for this texture.");

        if (_tileMap.TryGetValue(coordinates, out var tile))
        {
            if (tile.Width != width || tile.Height != height)
                ResizeTile(coordinates, tile, width, height);
            return tile;
        }

        var created = CreateTile(coordinates, width, height);
        _tileMap.Add(coordinates, created);
        _tileList!.Add(created);
        return created;
    }

    protected bool TryGetTile((int X, int Y) coordinates, out AbstBaseTexture2D<TFrameworkTexture> tile)
    {
        if (_tileMap == null)
        {
            tile = default!;
            return false;
        }

        if (_tileMap.TryGetValue(coordinates, out var existing))
        {
            tile = existing;
            return true;
        }

        tile = default!;
        return false;
    }

    protected void RemoveTile((int X, int Y) coordinates)
    {
        if (_tileMap == null)
            return;

        if (_tileMap.TryGetValue(coordinates, out var tile))
        {
            _tileMap.Remove(coordinates);
            _tileList!.Remove(tile);
            OnTileRemoved(coordinates, tile);
        }
    }

    protected void ClearTiles(bool disposeTiles = true)
    {
        if (_tileMap == null)
            return;

        if (disposeTiles)
        {
            foreach (var pair in _tileMap)
                OnTileRemoved(pair.Key, pair.Value);
        }

        _tileMap.Clear();
        _tileList!.Clear();
    }

    protected void DisposeRegisteredTiles()
    {
        ClearTiles();
        ClearDirtyTiles();
    }

    protected void MarkTileDirty((int X, int Y) coordinates)
    {
        DirtyTileSet.Add(coordinates);
    }

    protected void MarkTilesDirty(TileRange range)
    {
        if (!range.HasTiles)
            return;

        var dirty = DirtyTileSet;
        for (int x = range.StartX; x <= range.EndX; x++)
        {
            for (int y = range.StartY; y <= range.EndY; y++)
                dirty.Add((x, y));
        }
    }

    protected void MarkRegionDirty(APoint position, APoint size, int tileSize)
    {
        if (tileSize <= 0)
            return;

        var range = CalculateTileRange(position, size, tileSize);
        MarkTilesDirty(range);
    }

    protected void MarkAllTilesDirty(int width, int height, int tileSize)
    {
        if (tileSize <= 0)
            return;

        var dirty = DirtyTileSet;
        int tilesX = (Math.Max(width, 0) + tileSize - 1) / tileSize;
        int tilesY = (Math.Max(height, 0) + tileSize - 1) / tileSize;
        for (int x = 0; x < tilesX; x++)
        {
            for (int y = 0; y < tilesY; y++)
                dirty.Add((x, y));
        }
    }

    protected void ClearDirtyTiles()
    {
        _dirtyTiles?.Clear();
    }

    protected void ForEachTile(
        int targetWidth,
        int targetHeight,
        int tileSize,
        bool createMissing,
        Action<(int X, int Y), int, int, int, int, AbstBaseTexture2D<TFrameworkTexture>> callback)
    {
        if (_tileMap == null)
            throw new NotSupportedException("Tile management is disabled for this texture.");

        if (targetWidth <= 0 || targetHeight <= 0)
            return;

        if (tileSize <= 0)
            throw new InvalidOperationException("TileSize must be positive to iterate tiles.");

        int tilesX = (Math.Max(targetWidth, 0) + tileSize - 1) / tileSize;
        int tilesY = (Math.Max(targetHeight, 0) + tileSize - 1) / tileSize;

        for (int x = 0; x < tilesX; x++)
        {
            for (int y = 0; y < tilesY; y++)
            {
                var coordinates = (x, y);
                if (!TryComputeTileBounds(coordinates, tileSize, targetWidth, targetHeight, out int offsetX, out int offsetY, out int width, out int height))
                    continue;

                AbstBaseTexture2D<TFrameworkTexture> tile;
                if (createMissing)
                {
                    tile = GetOrCreateTile(coordinates, width, height);
                }
                else if (!TryGetTile(coordinates, out tile))
                {
                    continue;
                }

                var actualWidth = tile.Width;
                var actualHeight = tile.Height;
                if (actualWidth <= 0 || actualHeight <= 0)
                    continue;

                if (actualWidth != width || actualHeight != height)
                {
                    width = actualWidth;
                    height = actualHeight;
                }

                callback(coordinates, offsetX, offsetY, width, height, tile);
            }
        }
    }

    protected void ForEachDirtyTile(
        int targetWidth,
        int targetHeight,
        int tileSize,
        Action<(int X, int Y), int, int, int, int, AbstBaseTexture2D<TFrameworkTexture>> callback)
    {
        if (_dirtyTiles == null || _dirtyTiles.Count == 0)
            return;

        foreach (var coordinates in _dirtyTiles)
        {
            if (!TryComputeTileBounds(coordinates, tileSize, targetWidth, targetHeight, out int offsetX, out int offsetY, out int width, out int height))
                continue;

            var tile = GetOrCreateTile(coordinates, width, height);
            callback(coordinates, offsetX, offsetY, width, height, tile);
        }
    }

    protected static bool TryComputeTileBounds((int X, int Y) coordinates, int tileSize, int targetWidth, int targetHeight, out int offsetX, out int offsetY, out int width, out int height)
    {
        offsetX = coordinates.X * tileSize;
        offsetY = coordinates.Y * tileSize;
        width = Math.Min(tileSize, targetWidth - offsetX);
        height = Math.Min(tileSize, targetHeight - offsetY);

        if (width <= 0 || height <= 0)
        {
            width = 0;
            height = 0;
            return false;
        }

        return true;
    }

    protected static TileRange CalculateTileRange(APoint position, APoint size, int tileSize)
    {
        if (tileSize <= 0 || size.X <= 0 || size.Y <= 0)
            return default;

        int startX = FastFloor(position.X, tileSize);
        int startY = FastFloor(position.Y, tileSize);
        int endX = FastFloor(position.X + size.X - 1, tileSize);
        int endY = FastFloor(position.Y + size.Y - 1, tileSize);

        startX = Math.Max(0, startX);
        startY = Math.Max(0, startY);
        endX = Math.Max(0, endX);
        endY = Math.Max(0, endY);

        return endX < startX || endY < startY
            ? default
            : new TileRange(startX, startY, endX, endY);
    }

    private static int FastFloor(float value, int tileSize)
    {
#if NET48
        return (int)Math.Floor(value / tileSize);
#else
        return (int)MathF.Floor(value / tileSize);
#endif
    }

    public override byte[] GetPixels()
    {
        if (_tileMap == null)
            throw new NotSupportedException("Tile management is disabled; override GetPixels in derived class.");

        int width = Width;
        int height = Height;
        if (width <= 0 || height <= 0)
            return Array.Empty<byte>();

        if (_tileMap.Count == 0)
            return new byte[checked(width * height * 4)];

        int tileSize = TileSize;
        if (tileSize <= 0)
            throw new InvalidOperationException("TileSize must be positive to read pixels.");

        int totalPixels = checked(width * height);
        var pixels = new byte[checked(totalPixels * 4)];
        int destStride = width * 4;

        ForEachTile(width, height, tileSize, createMissing: false, (coords, offsetX, offsetY, tileWidth, tileHeight, tile) =>
        {
            var tilePixels = tile.GetPixels();
            if (tilePixels.Length == 0)
                return;

            int srcStride = tileWidth * 4;
            int expectedLength = srcStride * tileHeight;
            if (tilePixels.Length < expectedLength)
                throw new InvalidOperationException($"Tile {coords} returned insufficient pixel data.");

            var tileSpan = tilePixels.AsSpan();
            var dest = pixels.AsSpan();

            for (int row = 0; row < tileHeight; row++)
            {
                int destIndex = ((offsetY + row) * destStride) + (offsetX * 4);
                tileSpan.Slice(row * srcStride, srcStride).CopyTo(dest.Slice(destIndex, srcStride));
            }
        });

        return pixels;
    }

    public override void SetARGBPixels(byte[] argbPixels)
        => SetTilePixels(argbPixels, PixelWriteFormat.ARGB);

    public override void SetRGBAPixels(byte[] rgbaPixels)
        => SetTilePixels(rgbaPixels, PixelWriteFormat.RGBA);

    private void SetTilePixels(byte[] pixels, PixelWriteFormat format)
    {
        if (_tileMap == null)
            throw new NotSupportedException("Tile management is disabled; override pixel setters in derived class.");

        if (pixels == null)
            throw new ArgumentNullException(nameof(pixels));

        int width = Width;
        int height = Height;
        if (width <= 0 || height <= 0)
        {
            if (pixels.Length != 0)
                throw new ArgumentException("Pixel buffer length does not match texture dimensions.", nameof(pixels));
            return;
        }

        int expectedLength = checked(width * height * 4);
        if (pixels.Length != expectedLength)
            throw new ArgumentException($"Expected buffer length of {expectedLength} for {width}x{height} texture.", nameof(pixels));

        int tileSize = TileSize;
        if (tileSize <= 0)
            throw new InvalidOperationException("TileSize must be positive to write pixels.");

        int srcStride = width * 4;

        ForEachTile(width, height, tileSize, createMissing: true, (coords, offsetX, offsetY, tileWidth, tileHeight, tile) =>
        {
            int tileStride = tileWidth * 4;
            int tileLength = tileStride * tileHeight;
            if (tileLength == 0)
                return;

            var tileBuffer = new byte[tileLength];
            var tileSpan = tileBuffer.AsSpan();

            for (int row = 0; row < tileHeight; row++)
            {
                int srcIndex = ((offsetY + row) * srcStride) + (offsetX * 4);
                pixels.AsSpan(srcIndex, tileStride).CopyTo(tileSpan.Slice(row * tileStride, tileStride));
            }

            switch (format)
            {
                case PixelWriteFormat.ARGB:
                    tile.SetARGBPixels(tileBuffer);
                    break;
                case PixelWriteFormat.RGBA:
                    tile.SetRGBAPixels(tileBuffer);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, null);
            }
        });
    }

    private enum PixelWriteFormat
    {
        ARGB,
        RGBA,
    }

    protected readonly struct TileRange
    {
        public TileRange(int startX, int startY, int endX, int endY)
        {
            StartX = startX;
            StartY = startY;
            EndX = endX;
            EndY = endY;
        }

        public int StartX { get; }
        public int StartY { get; }
        public int EndX { get; }
        public int EndY { get; }
        public bool HasTiles => EndX >= StartX && EndY >= StartY;
    }
}

public interface ITextureGridOwner<TFrameworkTexture>
{
    void DestroyTileTexture(TFrameworkTexture texture);
    byte[] GetTexturePixels(TFrameworkTexture texture, int width, int height);
    void SetTextureARGBPixels(TFrameworkTexture texture, int width, int height, byte[] argbPixels);
    void SetTextureRGBAPixels(TFrameworkTexture texture, int width, int height, byte[] rgbaPixels);
}

public sealed class TextureGridTile<TFrameworkTexture> : AbstBaseTexture2D<TFrameworkTexture>
{
    private readonly ITextureGridOwner<TFrameworkTexture> _owner;
    private readonly (int X, int Y) _coordinates;
    private int _width;
    private int _height;
    private AbstBaseTexture2D<TFrameworkTexture>? _pixelsHost;

    public TextureGridTile(
        string baseName,
        ITextureGridOwner<TFrameworkTexture> owner,
        (int X, int Y) coordinates,
        int width,
        int height,
        TFrameworkTexture texture,
        AbstBaseTexture2D<TFrameworkTexture>? pixelsHost)
        : base($"{baseName}_Tile_{coordinates.X}_{coordinates.Y}")
    {
        _owner = owner;
        _coordinates = coordinates;
        _width = width;
        _height = height;
        Texture = texture;
        _pixelsHost = pixelsHost;
    }

    public override int Width => _width;
    public override int Height => _height;
    public TFrameworkTexture Texture { get; private set; }
    public (int X, int Y) Coordinates => _coordinates;

    public void UpdateTexture(TFrameworkTexture texture, int width, int height, AbstBaseTexture2D<TFrameworkTexture>? pixelsHost)
    {
        ReleaseCurrentTexture();
        Texture = texture;
        _width = width;
        _height = height;
        _pixelsHost = pixelsHost;
    }

    protected override void DisposeTexture()
    {
        ReleaseCurrentTexture();
    }

    public override byte[] GetPixels()
    {
        if (IsDefault(Texture))
            return Array.Empty<byte>();

        if (_pixelsHost != null)
            return _pixelsHost.GetPixels();

        return _owner.GetTexturePixels(Texture, Width, Height);
    }

    public override void SetARGBPixels(byte[] argbPixels)
    {
        if (IsDefault(Texture))
            return;

        if (_pixelsHost != null)
        {
            _pixelsHost.SetARGBPixels(argbPixels);
            return;
        }

        _owner.SetTextureARGBPixels(Texture, Width, Height, argbPixels);
    }

    public override void SetRGBAPixels(byte[] rgbaPixels)
    {
        if (IsDefault(Texture))
            return;

        if (_pixelsHost != null)
        {
            _pixelsHost.SetRGBAPixels(rgbaPixels);
            return;
        }

        _owner.SetTextureRGBAPixels(Texture, Width, Height, rgbaPixels);
    }

    public override IAbstTexture2D Clone() => throw new NotSupportedException("Grid tiles do not support cloning.");

    private void ReleaseCurrentTexture()
    {
        if (_pixelsHost != null)
        {
            _pixelsHost.Dispose();
            _pixelsHost = null;
            Texture = default!;
            return;
        }

        if (!IsDefault(Texture))
        {
            _owner.DestroyTileTexture(Texture);
            Texture = default!;
        }
    }

    private static bool IsDefault(TFrameworkTexture value)
        => EqualityComparer<TFrameworkTexture>.Default.Equals(value, default!);
}
