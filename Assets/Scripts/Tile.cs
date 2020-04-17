using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;
using UnityEngine.UI;

public enum TileState
{
    Resting,
    Rotating,
    MovingDown,
    FallingDown,
    Destroyed
};

public enum TileType
{
    Regular,
    Star,
    Bomb
}

public class Tile : MonoBehaviour
{
    public TileState state = TileState.Resting;
    public TileType tileType = TileType.Regular;
    public int x;
    public int y;
    public GameEvent OnBombExplode;
    public int pointMultiplier => tileType == TileType.Star ? 2 : 1;
    public int points = 5;
    public Color tileColor;
    public int bombLife;
    public Vector2Int targetIndex;
    public GameObject bombCounter;
    private TileData previousState;
    private HexGrid grid;
    private SpriteRenderer hexSpriteRenderer;
    private SpriteRenderer bombSpriteRenderer;
    private SpriteRenderer starSpriteRenderer;
    // Start is called before the first frame update
    void Awake()
    {
        hexSpriteRenderer = GetComponent<SpriteRenderer>();
        starSpriteRenderer = gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>();
        bombSpriteRenderer = gameObject.transform.GetChild(1).GetComponent<SpriteRenderer>();
        tileColor = hexSpriteRenderer.color;
        grid = GameObject.Find("Grid").GetComponent<HexGrid>();
        UpdateState(TileState.Resting);
        Initialize();
    }

    public void Initialize()
    {
        if (tileType == TileType.Regular) EnableHexSprite();
        if (tileType == TileType.Star) EnableStarSprite();
        if (tileType == TileType.Bomb)
        {
            EnableBombSprite();
            bombLife = grid.gridSettings.bombLife;
            UpdateBombCounterText();
        }
    }

    void EnableBombSprite()
    {
        bombSpriteRenderer.enabled = true;
        hexSpriteRenderer.enabled = false;
        starSpriteRenderer.enabled = false;
        bombCounter.SetActive(true);
    }

    void EnableHexSprite()
    {
        hexSpriteRenderer.enabled = true;
        bombSpriteRenderer.enabled = false;
        starSpriteRenderer.enabled = false;
        bombCounter.SetActive(false);
    }

    void EnableStarSprite()
    {
        hexSpriteRenderer.enabled = true;
        starSpriteRenderer.enabled = true;
        bombSpriteRenderer.enabled = false;
        bombCounter.SetActive(false);
    }

    public void ProcessBombTimer()
    {
        if(tileType == TileType.Bomb)
        { 
            bombLife--;
            UpdateBombCounterText();
            if (bombLife == 0) grid.StartGridDestruction();
        }
    }

    void UpdateBombCounterText()
    {
        bombCounter.GetComponent<TextMeshPro>().text = bombLife.ToString();
    }

    void SetBombLife(int newBombLife)
    {
        bombLife = newBombLife;
        bombCounter.GetComponent<TextMeshPro>().text = bombLife.ToString();
    }

    public void UpdateState(TileState newState)
    {
        grid.RemoveTileState(this);
        state = newState;
        grid.UpdateTileState(this);
    }

    public void UpdateIndex(int newX, int newY)
    {
        x = newX;
        y = newY;
    }

    public void SetTileColor(Color color)
    {
        tileColor = color;
        hexSpriteRenderer.color = color;
        bombSpriteRenderer.color = color;
    }

    public void Reset()
    {
        UpdateState(TileState.Resting);
        GetComponent<BoxCollider2D>().enabled = true;
        tileType = TileType.Regular;
        SetTileColor(grid.getRandomColor());
    }

    public void DestroyTile()
    {
        tileType = TileType.Regular;
        DisableSprites();
        UpdateState(TileState.Destroyed);
        GetComponent<BoxCollider2D>().enabled = false;
        SpawnParticles();
    }

    public void SetTargetIndex(Vector2Int newIndex)
    {
        targetIndex = newIndex;
    }

    public void MoveDown()
    {
        UpdateState(TileState.MovingDown);
        var pos = grid.tilePositions[targetIndex].GetPosition();
        transform.DOMove(pos, grid.gridSettings.tileSpeed).SetEase(Ease.Linear).SetSpeedBased(true).OnComplete(MoveDownComplete);
    }

    void MoveDownComplete()
    {
        RefreshIndex();
        UpdateState(TileState.Resting);
        targetIndex = Vector2Int.zero;
    }

    public Vector2Int GetIndex()
    {
        return new Vector2Int(x, y);
    }

    private void DisableSprites()
    {
        hexSpriteRenderer.enabled = false;
        starSpriteRenderer.enabled = false;
        bombSpriteRenderer.enabled = false;
        bombCounter.SetActive(false);
    }

    public void SpawnTile()
    {
        UpdateState(TileState.FallingDown);
        float height = Camera.main.orthographicSize * 2 + transform.position.y + GetHeight() * 10;
        transform.position = new Vector3(transform.position.x, height, 0.0f);
        SetTileColor(grid.getRandomColor());
        Initialize();
        var pos = grid.tilePositions[targetIndex].GetPosition();
        transform.DOMove(pos, 6.0f).SetEase(Ease.Linear).SetSpeedBased(true).OnComplete(SpawnComplete);
    }

    private void SpawnComplete()
    {
        RefreshIndex();
        GetComponent<BoxCollider2D>().enabled = true;
        UpdateState(TileState.Resting);
    }
    
    public void RefreshIndex()
    {
        var newIndex = grid.GetIndex(this);
        x = newIndex.x;
        y = newIndex.y;
        grid.SetTile(GetIndex(), this);
    }

    void SpawnParticles()
    {
        var particleGO = SimplePool.instance.GetObject("Particles");
        particleGO.transform.position = GetCentoid();
        particleGO.GetComponent<Renderer>().material.SetColor("_EmissionColor", tileColor);
        var ps = particleGO.GetComponent<ParticleSystem>();
        ps.Play();
    }

    public Vector3 GetCentoid()
    {
        return GetComponent<BoxCollider2D>().bounds.center;
    }

    private float GetHeight()
    {
        return GetComponent<BoxCollider2D>().bounds.size.y;
    }

    public void ChangeSortingOrder(bool increase)
    {
        if(increase) hexSpriteRenderer.sortingOrder = 1;
        else hexSpriteRenderer.sortingOrder = 0;
    }

    Vector2 GetPoint(int pointIndex)
    {
        var colliderBounds = GetComponent<BoxCollider2D>().bounds;
        Vector3 center = colliderBounds.center;
        float halfSize = colliderBounds.size.x / 2 + grid.gridSettings.offsetAmount / 2.0f;
        var angleDegrees = 60 * pointIndex;
        var angleRadians = Mathf.PI / 180 * angleDegrees;
        return new Vector2(center.x + halfSize * Mathf.Cos(angleRadians), center.y + halfSize * Mathf.Sin(angleRadians));
    }

    public List<Vector2> GetAllPoints()
    {
        List<Vector2> pointList = new List<Vector2>(6);
        for (int i = 1; i <= 6; i++)
        {
            var point = GetPoint(i);
            pointList.Add(point);
        }

        return pointList;
    }

    public int GetColorID()
    {
        return grid.gridSettings.GetColorID(tileColor);
    }

    public void SetColorWithID(int colorID)
    {
        var color = grid.gridSettings.GetColorWithID(colorID);
        SetTileColor(color);
    }

    public TileData Serialize()
    {
        TileData serializedTile = new TileData(this);
        return serializedTile;
    }

    public void Deserialize(TileData tileData)
    {
        SetColorWithID(tileData.colorID);
        tileType = tileData.tileType;
        x = tileData.x;
        y = tileData.y;
        bombLife = tileData.bombLife;
    }

    public void SaveState()
    {
        if (previousState == null) previousState = new TileData(this);
        else previousState.Update(this);
    }

    public void UndoState()
    {
        if (previousState == null) return;
        UpdateState(TileState.Resting);
        GetComponent<BoxCollider2D>().enabled = true;
        SetColorWithID(previousState.colorID);
        tileType = previousState.tileType;
        x = previousState.x;
        y = previousState.y;
        transform.position = grid.tilePositions[GetIndex()].GetPosition();
        grid.SetTile(GetIndex(), this);
        bombLife = previousState.bombLife;
        Initialize();
        if (tileType == TileType.Bomb) SetBombLife(bombLife);
    }
}

[Serializable]
public class TileData
{
    public int colorID;
    public TileType tileType;
    public int x;
    public int y;
    public int bombLife;

    public TileData(Tile tile)
    {
        CopyData(tile);
    }

    public void Update(Tile tile)
    {
        CopyData(tile);
    }

    private void CopyData(Tile tile)
    {
        colorID = tile.GetColorID();
        tileType = tile.tileType;
        x = tile.x;
        y = tile.y;
        bombLife = tile.bombLife;
    }
}