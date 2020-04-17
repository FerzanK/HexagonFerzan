using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SelectionPoint
{
    public bool isRotating;
    public Vector2 position;
    private int rotationCount = 1;
    private Rotation rotationDirection;
    private readonly HexGrid grid;
    private readonly GameObject dotGameObject;
    private readonly GameObject outlineGameobject;
    private readonly SpriteRenderer dotSpriteRenderer;
    private readonly SpriteRenderer outlineSpriteRenderer;
    private List<Tile> tileCache;
    private Vector3[] outlinePointCache;

    public SelectionPoint()
    {
        grid = GameObject.Find("Grid").GetComponent<HexGrid>();
        dotGameObject = GameObject.Find("SelectionPoint");
        dotSpriteRenderer = dotGameObject.GetComponent<SpriteRenderer>();
        outlineGameobject = dotGameObject.transform.GetChild(0).gameObject;
        outlineSpriteRenderer = outlineGameobject.GetComponent<SpriteRenderer>();
    }

    public void SetPosition(Vector2 pos)
    {
        position = pos;
    }

    public void RotateTiles(Rotation rotation)
    {
        if (grid.isChanging) return;
        isRotating = true;
        rotationDirection = rotation;
        ChangeTileState(TileState.Rotating);
        ParentTiles();
        TilesChangeSortingOrder(true);
        float angle = 120.0f * rotationCount;
        if (rotationDirection == Rotation.CW) angle = -angle;
        dotGameObject.transform.DOLocalRotate(new Vector3(0, 0, angle), 0.2f).OnKill(TileRotationComplete);
    }

    public void SaveTileState()
    {
        var tiles = GetTiles();
        foreach (var tile in tiles) tile.SaveState();
    }

    void ParentTiles()
    {
        var tiles = GetTiles();
        if (tiles == null) return;
        foreach (var tile in tiles)
        {
            tile.transform.parent = dotGameObject.transform;
        }
    }

    void TilesChangeSortingOrder(bool increase)
    {
        var tiles = GetTiles();
        if (tiles == null) return;
        foreach (var tile in tiles)
        {
            tile.ChangeSortingOrder(increase);
        }
    }

    void UnParentTiles()
    {
        var tiles = GetTiles();
        if (tiles == null) return;
        foreach (var tile in tiles)
        {
            tile.transform.parent = grid.transform;
        }
    }

    void ChangeTileState(TileState tileState)
    {
        var tiles = GetTiles();
        foreach (var tile in tiles)
        {
            tile.UpdateState(tileState);
        }
    }

    void TileRotationComplete()
    {
        isRotating = false;
        ChangeTileState(TileState.Resting);
        UnParentTiles();
        UpdateTileIndexes();
        TilesChangeSortingOrder(false);
        if (rotationCount == 3)
        {
            rotationCount = 1;
            ResetRotation();
        }
        else
        {
            bool matchesExists = grid.CheckMatches(false);
            if (matchesExists)
            {
                ResetRotation();
                rotationCount = 1;
            }
            else
            {
                rotationCount++;
                RotateTiles(rotationDirection);
            }
        }
    }

    void UpdateTileIndexes()
    {
        var tiles = GetTiles();
        if (tiles == null) return;
        foreach (var tile in tiles) tile.RefreshIndex();
    }

    void ResetRotation()
    {
        dotGameObject.transform.rotation = Quaternion.identity;
        var tiles = GetTiles();
        foreach (var tile in tiles) tile.transform.rotation = Quaternion.identity;
    }

    public bool IsColorMatching()
    {
        var tiles = GetTiles();
        if (tiles == null) return false;
        if (tiles[0].state == TileState.Resting && 
            tiles[1].state == TileState.Resting &&
            tiles[2].state == TileState.Resting)
        {
            return (tiles[0].tileColor == tiles[1].tileColor &&
                    tiles[0].tileColor == tiles[2].tileColor);
        }

        return false;
    }

    public void RandomizeColors()
    {
        var tiles = GetTiles();
        var color = grid.getRandomColor();
        while(tiles[0].tileColor == color) color = grid.getRandomColor();
        tiles[0].SetTileColor(color);
    }

    public void ExplodeSelection()
    {
        var tiles = GetTiles();
        if (tiles == null) return;
        foreach (var tile in tiles) tile.DestroyTile();
    }

    public bool IsAdjacent(SelectionPoint selectionPoint)
    {
        var tiles = GetTiles();
        var otherTiles = selectionPoint.GetTiles();
        HashSet<Tile> totalTiles = new HashSet<Tile>();
        for (int i = 0; i < 3; i++)
        {
            totalTiles.Add(tiles[i]);
            totalTiles.Add(otherTiles[i]);
            if (totalTiles.Count != 2 * (i + 1)) return true;
        }

        return false;
    }

    public bool IsSameColor(SelectionPoint selectionPoint)
    {
        var tiles = GetTiles();
        var otherTiles = selectionPoint.GetTiles();

        return IsColorMatching() && selectionPoint.IsColorMatching() && tiles[0].tileColor == otherTiles[0].tileColor;
    }

    public Color GetColor()
    {
        var tiles = GetTiles();
        return tiles[0].tileColor;
    }

    public List<Tile> GetTiles()
    {
        var colliders = Physics2D.OverlapCircleAll(position, 0.2f);
        if (colliders.Length != 3) return tileCache;
        if (tileCache == null)
        {
            tileCache = new List<Tile>(3)
            {
                colliders[0].gameObject.GetComponent<Tile>(),
                colliders[1].gameObject.GetComponent<Tile>(),
                colliders[2].gameObject.GetComponent<Tile>()
            };
            return tileCache;
        }
        tileCache[0] = colliders[0].gameObject.GetComponent<Tile>();
        tileCache[1] = colliders[1].gameObject.GetComponent<Tile>();
        tileCache[2] = colliders[2].gameObject.GetComponent<Tile>();
        return tileCache;
    }

    public void CreateOutline()
    {
        var tiles = GetTiles();
        var pointList = new List<Vector3>();
        float cullDistance = 0.08f; // used to determine if points are on top each other
        foreach (var tile in tiles)
        {
            foreach (var tilePoint in tile.GetAllPoints())
            {
                if (Vector2.Distance(position, tilePoint) < cullDistance) continue;
                bool addPoint = true;
                foreach (var point in pointList)
                {
                    if (!(Vector2.Distance(point, tilePoint) < cullDistance)) continue;
                    addPoint = false;
                    break;
                }
                if(addPoint) pointList.Add(tilePoint);
            }
        }

        //create outline by reordering by closest vertices
        for (int i = 0; i < pointList.Count - 1; i++)
        {
            float maxDistance = float.MaxValue;
            int selectedIndex = -1;
            for (int t = i + 1; t < pointList.Count; t++)
            {
                var distance = Vector2.Distance(pointList[i], pointList[t]);
                if (distance < maxDistance)
                {
                    maxDistance = distance;
                    selectedIndex = t;
                }
            }
            if(selectedIndex != -1)
            { 
                var tmp = pointList[i + 1];
                pointList[i + 1] = pointList[selectedIndex];
                pointList[selectedIndex] = tmp;
            }
        }

        outlinePointCache = pointList.ToArray();

        //create line renderer,
        //turns out line renderer sucks so all of the previous effort wasted :)
        LineRenderer l = dotGameObject.AddComponent<LineRenderer>();
        l.material = new Material(Shader.Find("Mobile/Particles/Additive"));
        l.startWidth = 0.1f;
        l.endWidth = 0.1f;
        l.loop = true;
        l.numCornerVertices = 5;
        l.numCornerVertices = 5;
        l.startColor = Color.white;
        l.endColor = Color.white;
        l.positionCount = pointList.Count;
        l.SetPositions(outlinePointCache);
        l.useWorldSpace = true;
        l.sortingOrder = 10;
        l.alignment = LineAlignment.TransformZ;
    }

    void FlipSprite()
    {
        var tiles = GetTiles();
        var xPos = dotGameObject.transform.position.x;
        if ((xPos > tiles[0].transform.position.x && xPos > tiles[1].transform.position.x) ||
            (xPos > tiles[1].transform.position.x && xPos > tiles[2].transform.position.x) ||
            (xPos > tiles[0].transform.position.x && xPos > tiles[2].transform.position.x))
        {
            outlineSpriteRenderer.flipX = false;
            var pos = outlineGameobject.transform.localPosition;
            outlineGameobject.transform.localPosition = new Vector3(Mathf.Abs(pos.x), pos.y, pos.z);
        }
        else
        {
            outlineSpriteRenderer.flipX = true;
            var pos = outlineGameobject.transform.localPosition;
            outlineGameobject.transform.localPosition = new Vector3(-Mathf.Abs(pos.x), pos.y, pos.z);
        }
    }

    public void Select()
    {
        dotSpriteRenderer.enabled = true;
        outlineSpriteRenderer.enabled = true;
        dotGameObject.transform.position = position;
        FlipSprite();
    }

    public void Deselect()
    {
        dotSpriteRenderer.enabled = false;
        outlineSpriteRenderer.enabled = false;
    }

}
