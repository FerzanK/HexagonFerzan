using UnityEngine;

public class TilePosition
{
    private Vector2Int index;
    private Vector2 position;
    private Tile tile;

    public Tile GetTile()
    {
        return tile;
    }

    public void SetTile(Tile tile)
    {   
        this.tile = tile;
    }

    public void SetIndex(Vector2Int index)
    {
        this.index = index;
    }

    public void SetPosition(Vector2 position)
    {
        this.position = position;
    }

    public Vector2 GetPosition()
    {
        return position;
    }

    public bool IsEmpty()
    {
        return tile = null;
    }

    public void ClearTile(Tile tile)
    {
        this.tile = null;
    }

}

