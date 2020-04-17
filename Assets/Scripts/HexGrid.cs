using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;


public enum GridState { Stabilizing, Stable, Changing}

[System.Serializable]
public class HexGrid : MonoBehaviour
{
    public Dictionary<Vector2Int, TilePosition> tilePositions = new Dictionary<Vector2Int, TilePosition>();
    public List<SelectionPoint> selectionPoints = new List<SelectionPoint>();
    public Dictionary<TileState, HashSet<Tile>> tileStates = new Dictionary<TileState, HashSet<Tile>>();
    public SelectionPoint currentSelectionPoint;
    public GridSettings gridSettings = new GridSettings();
    public GridState state = GridState.Stable;
    public bool isChanging => (tileStates[TileState.Resting].Count != tileCount);
    public GameObject HexTile;
    public GameEvent OnNoMoveMovesLeft;
    public GameEvent OnTileMatchEnd;
    public GameEvent OnScoreUpdate;
    public IntVariable moveCount;
    public GameEvent OnBombExplosion;
    public IntVariable score;
    private GameObject[,] tiles;
    private int bombSpawnScore = 0;
    private bool explosionOccured = false;
    private int tileCount;
    private Bounds boundingBox = new Bounds();
    private List<MatchPattern> matchPatterns;
    private void Start()
    {
        Create();
        CreateBoundingBox();
        tileCount = gridSettings.verticalCount * gridSettings.horizontalCount;
        state = GridState.Stable;
    }

    private void Update()
    {
        CheckState();
    }

    public void Reset()
    {
        foreach (var tilePosition in tilePositions)
        {
            tilePosition.Value.GetTile().Reset();
            tilePosition.Value.GetTile().Initialize();
        }
        explosionOccured = false;
        ClearMatches();
        state = GridState.Stable;
    }

    private void CheckState()
    {
        if (state == GridState.Stable && isChanging)
        {
            state = GridState.Changing;
            currentSelectionPoint.Deselect();
            return;
        }

        if (state == GridState.Changing && !isChanging)
        {
            if (CheckMatches()) return;
            state = GridState.Stabilizing;
            return;
        }

        if (state == GridState.Stabilizing)
        {
            state = GridState.Stable;
            currentSelectionPoint?.Select();
            if(!PossibleMovesExists())
            {
                OnNoMoveMovesLeft.Raise();
            }

            if (explosionOccured)
            {
                moveCount.value++;
                OnTileMatchEnd.Raise();
                explosionOccured = false;
            }
            return;
        }
    }

    private void CreatePatterns()
    {
        matchPatterns = new List<MatchPattern>();
        matchPatterns.Add(new MatchPattern(new Vector2Int(0, 2), new Vector2Int(1, 0), PatternConstraint.evenX, this));
        matchPatterns.Add(new MatchPattern(new Vector2Int(0, 2), new Vector2Int(1, 1), PatternConstraint.evenX, this));
        matchPatterns.Add(new MatchPattern(new Vector2Int(1, 0), new Vector2Int(2, 0), PatternConstraint.evenX, this));
        matchPatterns.Add(new MatchPattern(new Vector2Int(0, 1), new Vector2Int(2, 1), PatternConstraint.evenX, this));
        matchPatterns.Add(new MatchPattern(new Vector2Int(0, 1), new Vector2Int(2, 0), PatternConstraint.evenX, this));
        matchPatterns.Add(new MatchPattern(new Vector2Int(1, 1), new Vector2Int(1, 0), PatternConstraint.evenX, this));
        matchPatterns.Add(new MatchPattern(new Vector2Int(0, 1), new Vector2Int(1, 1), PatternConstraint.evenX, this));
        matchPatterns.Add(new MatchPattern(new Vector2Int(0, 1), new Vector2Int(1, -1), PatternConstraint.evenX, this));
        matchPatterns.Add(new MatchPattern(new Vector2Int(2, 1), new Vector2Int(1, -1), PatternConstraint.evenX, this));
        matchPatterns.Add(new MatchPattern(new Vector2Int(1, 1), new Vector2Int(1, -1), PatternConstraint.evenX, this));
        matchPatterns.Add(new MatchPattern(new Vector2Int(2, 0), new Vector2Int(1, 1), PatternConstraint.oddX, this));
        matchPatterns.Add(new MatchPattern(new Vector2Int(1, 2), new Vector2Int(1, 1), PatternConstraint.oddX, this));
        matchPatterns.Add(new MatchPattern(new Vector2Int(1, 2), new Vector2Int(1, 0), PatternConstraint.oddX, this));
        matchPatterns.Add(new MatchPattern(new Vector2Int(0, 1), new Vector2Int(1, 0), PatternConstraint.oddX, this));
        matchPatterns.Add(new MatchPattern(new Vector2Int(0, 1), new Vector2Int(2, 0), PatternConstraint.oddX, this));
        matchPatterns.Add(new MatchPattern(new Vector2Int(0, 1), new Vector2Int(2, 1), PatternConstraint.oddX, this));
        matchPatterns.Add(new MatchPattern(new Vector2Int(0, 1), new Vector2Int(1, 2), PatternConstraint.oddX, this));
    }

    private bool PossibleMovesExists()
    {
        if(matchPatterns == null) CreatePatterns();
        for (int x = 0; x < gridSettings.horizontalCount; x++)
        {
            for (int y = 0; y < gridSettings.verticalCount; y++)
            {
                var index = new Vector2Int(x, y);
                foreach (var pattern in matchPatterns)
                {
                    if(pattern.Match(index)) return true;
                }
            }
        }

        return false;
    }

    public void StartGridDestruction()
    {
        StartCoroutine(Destroy());
    }

    public IEnumerator Destroy()
    {
        float waitDuration = 0.5f;
        int destroyCount = 0;
        foreach (var tile in tiles)
        {
            tile.GetComponent<Tile>().DestroyTile();
            if(destroyCount == 15)
            {
                destroyCount = 0;
                yield return new WaitForSeconds(waitDuration);
            }
        }
        yield return new WaitForSeconds(0.5f);
        OnBombExplosion.Raise();
    }

    public void UpdateTileState(Tile tile)
    {
        if(tileStates.ContainsKey(tile.state)) tileStates[tile.state].Add(tile);
        else tileStates.Add(tile.state, new HashSet<Tile>{tile});
    }

    public void RemoveTileState(Tile tile)
    {
        if (tileStates.ContainsKey(tile.state)) tileStates[tile.state].Remove(tile);
    }

    private void CreateBoundingBox()
    {
        foreach (var tile in tiles)
        {
            boundingBox.Encapsulate(tile.GetComponent<BoxCollider2D>().bounds);
        }
    }

    public Bounds GetBounds()
    {
        return boundingBox;
    }

    public void RotateSelectionPointCC()
    {
        if (!isChanging && currentSelectionPoint != null && !currentSelectionPoint.isRotating)
        { 
            Rotation rotation = Rotation.CC;
            currentSelectionPoint.RotateTiles(rotation);
        }
    }

    public void RotateSelectionPointCW()
    {
        if (!isChanging && currentSelectionPoint != null && !currentSelectionPoint.isRotating)
        {
            Rotation rotation = Rotation.CW;
            currentSelectionPoint.RotateTiles(rotation);
        }
    }

    public void SelectSelectionPoint(Vector3 pos)
    {
        if (isChanging || (currentSelectionPoint != null && currentSelectionPoint.isRotating)) return;
        float distance = float.MaxValue;
        foreach (var sp in selectionPoints)
        {
            var dist = Vector3.Distance(sp.position, pos);
            if (distance > dist && dist < 0.5f)
            {
                distance = dist;
                currentSelectionPoint = sp;
            }
        }

        if (currentSelectionPoint != null)
        {
            currentSelectionPoint.Select();
        }

    }

    public void generateTiles(int rowCount, int columnCount)
    {
        tiles = new GameObject[rowCount, columnCount];
    }

    public void addHex(int row, int col, GameObject hexTile)
    {
        tiles[row, col] = hexTile;
    }
    
    public void Create()
    {
        generateTiles(gridSettings.horizontalCount, gridSettings.verticalCount);
        SpriteRenderer tileSpriteRenderer = HexTile.GetComponent<SpriteRenderer>();
        float spriteWidth = tileSpriteRenderer.bounds.size.x + gridSettings.offsetAmount;
        float spriteHeight = tileSpriteRenderer.bounds.size.y + gridSettings.offsetAmount;
        float offsetHeight;
        int tileCount = 0;
        for (int row = 0; row < gridSettings.horizontalCount; row++)
        {
            for (int col = 0; col < gridSettings.verticalCount; col++)
            {
                if (row % 2 == 1) offsetHeight = spriteHeight / 2.0f;
                else offsetHeight = 0.0f;

                var go = Instantiate(HexTile, new Vector3((row * spriteWidth * 0.75f) , (col * spriteHeight) + offsetHeight, 0.0f), Quaternion.identity);
                go.name = "Tile" + tileCount++;
                go.transform.parent = transform;
                Tile tile = go.GetComponent<Tile>();
                tile.x = row;
                tile.y = col;
                tile.SetTileColor(getRandomColor());
                tile.SaveState();
                var index = new Vector2Int(row, col);
                var tilePosition = new TilePosition();
                tilePosition.SetTile(tile);
                tilePosition.SetIndex(index);
                tilePosition.SetPosition(go.GetComponent<BoxCollider2D>().bounds.center);
                tilePositions.Add(index, tilePosition);
                addHex(row, col, go);
            }
        }

        CenterCameraOnGrid();
        CreateSelectionPoints();
        ClearMatches();
    }

    private void CreateSelectionPoints()
    {
        List<Vector2Int> lowerNeighbourVertices = new List<Vector2Int>();
        lowerNeighbourVertices.Add(Vector2Int.left + Vector2Int.up);
        lowerNeighbourVertices.Add(Vector2Int.left);
        lowerNeighbourVertices.Add(Vector2Int.down);
        lowerNeighbourVertices.Add(Vector2Int.right);
        lowerNeighbourVertices.Add(Vector2Int.up + Vector2Int.right);

        for (int row = 1; row < gridSettings.horizontalCount; row += 2)
        {
            for (int col = 0; col < gridSettings.verticalCount; col++)
            {
                for (int i = 0; i < lowerNeighbourVertices.Count - 1; i++)
                {
                    var indexBase = new Vector2Int(row, col);
                    var index1 = indexBase + lowerNeighbourVertices[i];
                    var index2 = indexBase + lowerNeighbourVertices[i + 1];
                    try
                    {
                        var c1 = tiles[row, col].GetComponent<BoxCollider2D>().bounds.center;
                        var c2 = tiles[index1.x, index1.y].GetComponent<BoxCollider2D>().bounds.center;
                        var c3 = tiles[index2.x, index2.y].GetComponent<BoxCollider2D>().bounds.center;
                        var t = (c1 + c2 + c3) / 3.0f;
                        var selectionPoint = new SelectionPoint();
                        selectionPoint.SetPosition(t);
                        selectionPoints.Add(selectionPoint);
                    }
                    catch (Exception e){ continue;}
                }

            }
        }
    }

    private void CenterCameraOnGrid()
    {
        // Get bounding box of the grid, since grid starts from [0.0f, 0.0f],
        // we just divide the last element by to get the center of the grid
        var boundingBoxCenter = tiles[gridSettings.horizontalCount - 1, gridSettings.verticalCount - 1].transform.position / 2.0f;
        boundingBoxCenter.z = -10.0f;
        Camera.main.transform.position = boundingBoxCenter;
    }

    public void SetTile(Vector2Int tileIndex, Tile tile)
    {
        tilePositions[tileIndex].SetTile(tile);
    }

    public bool CheckMatches(bool isAutomatic = true)
    {
        bool result = false;
        if (isChanging) return false;
        var explodeList = new List<SelectionPoint>();
        foreach (var selectionPoint in selectionPoints)
        {
            if (selectionPoint.IsColorMatching())
            {
                selectionPoint.SaveTileState();
                explodeList.Add(selectionPoint);
                result = true;
            }
        }

        if (explodeList.Count > 0)
        {
            currentSelectionPoint?.Deselect();
            Dictionary<int, HashSet<Tile>> explodedTiles = new Dictionary<int, HashSet<Tile>>();
            foreach (var selectionPoint in explodeList)
            {
                selectionPoint.ExplodeSelection();
                foreach (var tile in selectionPoint.GetTiles())
                {
                    if (!explodedTiles.ContainsKey(tile.x)) explodedTiles.Add(tile.x, new HashSet<Tile>() { tile });
                    else explodedTiles[tile.x].Add(tile);
                }
            }

            int currentScore = CalculateTileScore(explodeList);
            score.value += currentScore;
            OnScoreUpdate.Raise();
            SpawnDestroyedTiles(explodedTiles);
            explosionOccured = true;
        }

        return result;
    }

    private void SpawnDestroyedTiles(Dictionary<int, HashSet<Tile>> explodedTiles)
    {
        foreach (var x in explodedTiles)
        {
            int explosionCount = explodedTiles[x.Key].Count;
            int explosionCountFromBottom = 0;
            var explodedTileSpawnIndex = gridSettings.verticalCount - explosionCount;
            for (int y = 0;  y < gridSettings.verticalCount; y++)
            {
                var index = new Vector2Int(x.Key, y);
                var tile = tilePositions[index].GetTile();
                if (tile.state == TileState.Destroyed)
                {
                    var newIndex = new Vector2Int(x.Key, explodedTileSpawnIndex);
                    tile.SetTargetIndex(newIndex);
                    if (Random.Range(0, 100) < gridSettings.starSpawnPercent) tile.tileType = TileType.Star;
                    if (score.value > bombSpawnScore + gridSettings.bombSpawnScore)
                    {
                        bombSpawnScore = score.value;
                        tile.tileType = TileType.Bomb;
                    }
                    tile.SpawnTile();
                    explodedTileSpawnIndex++;
                    explosionCountFromBottom++;
                }
                else
                {
                    if (explosionCountFromBottom > 0)
                    {
                        var newIndex = new Vector2Int(x.Key, y - explosionCountFromBottom);
                        tile.SetTargetIndex(newIndex);
                        tile.MoveDown();

                    }
                }
            }
        }
    }

    private int CalculateTileScore(List<SelectionPoint> explosionList)
    {
        int totalScore = 0;
        Dictionary<Color,List<SelectionPoint>> colorCoded = new Dictionary<Color, List<SelectionPoint>>();
        foreach (var selectionPoint in explosionList)
        {
            var spColor = selectionPoint.GetColor();
            if (colorCoded.ContainsKey(spColor)) colorCoded[spColor].Add(selectionPoint);
            else colorCoded.Add(spColor, new List<SelectionPoint>(){selectionPoint});
        }

        HashSet<Tile> uniqueTiles = new HashSet<Tile>();
        foreach (var spDict in colorCoded)
        {
            Vector2 scoreTextSpawnPosition = Vector2.zero;
            foreach (var selectionPoint in spDict.Value)
            {
                foreach (var tile in selectionPoint.GetTiles()){uniqueTiles.Add(tile);};
                scoreTextSpawnPosition += selectionPoint.position;
            }

            scoreTextSpawnPosition = scoreTextSpawnPosition / spDict.Value.Count;
            int newScore = calculateScoreFromTiles(uniqueTiles.ToList());
            totalScore += newScore;
            SpawnScoreText(scoreTextSpawnPosition, newScore);
            uniqueTiles.Clear();
        }
        return totalScore;
    }

    private int calculateScoreFromTiles(List<Tile> tiles)
    {
        int multiplier = 1;
        int scoreSum = 0;
        foreach (var tile in tiles)
        {
            multiplier *= tile.pointMultiplier;
            scoreSum += tile.points;
        }

        return multiplier * scoreSum;
    }

    private void SpawnScoreText(Vector2 position, int score)
    {
        var go = SimplePool.instance.GetObject("TilePoints");
        go.transform.position = position;
        go.GetComponent<TilePoints>().ShowPoints(score);
    }

    public void ClearMatches()
    {
        bool removedAllMatches = false;
        while(!removedAllMatches)
        {
            removedAllMatches = true;
            foreach (var selectionPoint in selectionPoints)
            {
                if (selectionPoint.IsColorMatching())
                {
                    selectionPoint.RandomizeColors();
                    removedAllMatches = false;
                    break;
                }
            }
        }
    }
    
    public Vector2Int GetIndex(Tile tile)
    {
        float minDistance = float.MaxValue;
        Vector2Int index = new Vector2Int();
        foreach (var tilePosition in tilePositions)
        {
            float distance = Vector2.Distance(tile.transform.position, tilePosition.Value.GetPosition());
            if (minDistance > distance)
            {
                minDistance = distance;
                index = tilePosition.Key;
            }
        }
        return index;
    }

    public Color getRandomColor()
    {
        int randomIndex = Random.Range(0, gridSettings.colors.Count);
        return gridSettings.colors[randomIndex];
    }

    public void Serialize(GameState state)
    {
        foreach (var tile in tiles) state.Add(tile.GetComponent<Tile>());
        state.Add(gridSettings);
    }

}