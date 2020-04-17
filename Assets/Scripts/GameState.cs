using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameState
{
    public int score = 0;
    public int highScore = 0;
    public int moveCount = 0;
    public bool bombSpawned;
    public GridSettings gridSettings;
    public GameManager gameManager;
    public List<TileData> gridTileData = new List<TileData>();

    public void Add(Tile tile)
    {
        gridTileData.Add(tile.Serialize());
    }

    public void Add(GameManager gameManager)
    {
        this.gameManager = gameManager;
    }

    public void Add(HexGrid grid)
    {
        grid.Serialize(this);
    }

    public void Add(GridSettings gridSettings)
    {
        this.gridSettings = gridSettings;
    }

    public string Serialize()
    {
        var gameStateJSONSerialized = JsonUtility.ToJson(this);
        Debug.Log(gameStateJSONSerialized);
        return gameStateJSONSerialized;
    }

}
