
namespace Blingo.PacMan.Core.Game;

public sealed class PMMap
{
    private readonly List<PMTile> _tiles;
    private readonly List<PMTile> _tunnels;

    public IReadOnlyList<PMTile> Tiles => _tiles;

   

    public int Width { get; }

    public int Height { get; }

    public PMTile? House { get; }

    public PMTile? HouseCenter { get; }
    public PMTile? PacManCenter { get; }
    public int TileWidth { get; }

    public int TileHeight { get; }

    public PMMap(IEnumerable<string> data)
    {
        var rows = data.ToList();
        if (rows.Count == 0)
            throw new ArgumentException("Map data must contain at least one row.", nameof(data));

        Width = rows[0].Length;
        Height = rows.Count;

        _tiles = new List<PMTile>(Width * Height);
        _tunnels = new List<PMTile>();

        PMTile? firstHouseTile = null;

        for (var y = 0; y < Height; y++)
        {
            var row = rows[y];
            if (row.Length != Width)
                throw new ArgumentException("All rows in the map data must be of equal length.", nameof(data));

            for (var x = 0; x < Width; x++)
            {
                var tile = new PMTile(row[x], x, y, this);
                _tiles.Add(tile);

                if (tile.IsHouse() && firstHouseTile is null)
                    firstHouseTile = tile;

                if (tile.IsTunnel() && (tile.Column == 0 || tile.Column == Width - 1))
                    _tunnels.Add(tile);
            }
        }

        House = firstHouseTile;
        HouseCenter = House?.GetDown()?.GetDown();
        PacManCenter = GetTile((Width / 2)-1, Height - 10);
        if (_tiles.Count > 0)
        {
            TileWidth = _tiles[0].Width;
            TileHeight = _tiles[0].Height;
        }
    }
    public IReadOnlyList<PMTile> Tunnels => _tunnels;
    public IEnumerable<PMTile> Pills => _tiles.Where(x => x.Type == PMTile.TileType.Pill);
    public IEnumerable<PMTile> Walls => _tiles.Where(x => x.Type == PMTile.TileType.Wall);
    public IEnumerable<PMTile> Pellets => _tiles.Where(x => x.Type == PMTile.TileType.Pellet);
    public PMTile? GetTile(float column, float row, bool inPixels = false)
    {
        if (!inPixels)
        {
            return GetTile((int)MathF.Floor(column), (int)MathF.Floor(row));
        }

        if (_tiles.Count == 0)
            return null;

        if (TileWidth == 0 || TileHeight == 0)
            return null;

        var columnIndex = (int)MathF.Floor(column / TileWidth);
        var rowIndex = (int)MathF.Floor(row / TileHeight);

        return GetTile(columnIndex, rowIndex);
    }

    public PMTile GetTileByIndex(int index) => _tiles[index];
    public PMTile? GetTile(int column, int row, bool inPixels = false)
    {
        if (_tiles.Count == 0)
            return null;

        if (inPixels)
        {
            return GetTile((float)column, (float)row, true);
        }

        column = WrapColumn(column);

        if (row < 0 || row >= Height)
            return null;

        var index = row * Width + column;
        if ((uint)index >= (uint)_tiles.Count)
            return null;

        return _tiles[index];
    }

    public void DestroyItems()
    {
        foreach (var tile in _tiles)
            tile.Item?.Destroy();
    }

    public void HideItems()
    {
        foreach (var tile in _tiles)
            tile.Item?.Hide();
    }

    private int WrapColumn(int column)
    {
        if (Width == 0)
            return column;

        if (column > Width - 1)
            column = 0;
        else if (column < 0)
            column = Width - 1;

        return column;
    }

}
