using UnityEngine;

public class TilePosition : IGridItem
{
    private Vector2 position;
    private Tile tile;
    private Vector2Int index;
    private static GridSearch<TilePosition> spatialSearch = new GridSearch<TilePosition>();

    public TilePosition(Tile tile, Vector2Int index)
    {
        this.tile = tile;
        this.index = index;
        position = tile.GetCentoid();
        spatialSearch.Add(this);
    }

    public static TilePosition GetClosestTilePosition(Vector2 pos)
    {
        return spatialSearch.Search(pos);
    }

    public Vector2Int GetIndex()
    {
        return index;
    }

    public Tile GetTile()
    {
        return tile;
    }

    public void SetTile(Tile tile)
    {   
        this.tile = tile;
    }

    public void SetPosition(Vector2 position)
    {
        this.position = position;
    }

    public Vector2 GetPosition()
    {
        return position;
    }

}

