using System.Collections.Generic;

namespace Blingo.PacMan.Core.Game;

public sealed class Map
{
    private readonly List<Tile> _tiles;
    private readonly List<Tile> _tunnels;

    public IReadOnlyList<Tile> Tiles => _tiles;

    public IReadOnlyList<Tile> Tunnels => _tunnels;

    public int Width { get; }

    public int Height { get; }

    public Tile? House { get; }

    public Tile? HouseCenter { get; }

    public int TileWidth { get; }

    public int TileHeight { get; }

    public Map(IEnumerable<string> data)
    {
        if (data is null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        var rows = new List<string>();
        foreach (var row in data)
        {
            if (row is null)
            {
                throw new ArgumentException("Map rows cannot be null.", nameof(data));
            }

            rows.Add(row);
        }

        if (rows.Count == 0)
        {
            throw new ArgumentException("Map data must contain at least one row.", nameof(data));
        }

        Width = rows[0].Length;
        Height = rows.Count;

        _tiles = new List<Tile>(Width * Height);
        _tunnels = new List<Tile>();

        Tile? firstHouseTile = null;

        for (var y = 0; y < Height; y++)
        {
            var row = rows[y];
            if (row.Length != Width)
            {
                throw new ArgumentException("All rows in the map data must be of equal length.", nameof(data));
            }

            for (var x = 0; x < Width; x++)
            {
                var tile = new Tile(row[x], x, y, this);
                _tiles.Add(tile);

                if (tile.IsHouse() && firstHouseTile is null)
                {
                    firstHouseTile = tile;
                }

                if (tile.IsTunnel() && (tile.Column == 0 || tile.Column == Width - 1))
                {
                    _tunnels.Add(tile);
                }
            }
        }

        House = firstHouseTile;
        HouseCenter = House?.GetDown()?.GetDown();

        if (_tiles.Count > 0)
        {
            TileWidth = _tiles[0].Width;
            TileHeight = _tiles[0].Height;
        }
    }

    public Tile? GetTile(float column, float row, bool inPixels = false)
    {
        return GetTile((int)column, (int)row, inPixels);
    }

    public Tile? GetTile(int column, int row, bool inPixels = false)
    {
        if (_tiles.Count == 0)
        {
            return null;
        }

        if (inPixels)
        {
            if (TileWidth == 0 || TileHeight == 0)
            {
                return null;
            }

            column /= TileWidth;
            row /= TileHeight;
        }

        column = WrapColumn(column);
        row = WrapRow(row);

        var index = row * Width + column;
        if ((uint)index >= (uint)_tiles.Count)
        {
            return null;
        }

        return _tiles[index];
    }

    public void DestroyItems()
    {
        foreach (var tile in _tiles)
        {
            tile.Item?.Destroy();
        }
    }

    public void HideItems()
    {
        foreach (var tile in _tiles)
        {
            tile.Item?.Hide();
        }
    }

    private int WrapColumn(int column)
    {
        if (Width == 0)
        {
            return column;
        }

        if (column > Width - 1)
        {
            column = 0;
        }
        else if (column < 0)
        {
            column = Width - 1;
        }

        return column;
    }

    private int WrapRow(int row)
    {
        if (Height == 0)
        {
            return row;
        }

        if (row > Height - 1)
        {
            row = 0;
        }
        else if (row < 0)
        {
            row = Height - 1;
        }

        return row;
    }
}
