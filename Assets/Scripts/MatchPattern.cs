using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;


public enum PatternConstraint { oddX, evenX, NoConstraint}
public class MatchPattern
{
    public Vector2Int index1;
    public Vector2Int index2;
    public PatternConstraint constraint = PatternConstraint.NoConstraint;
    private int maxX;
    private int maxY;
    private HexGrid grid;

    public MatchPattern(Vector2Int index1, Vector2Int index2, PatternConstraint constraint, HexGrid grid)
    {
        this.index1 = index1;
        this.index2 = index2;
        this.constraint = constraint;
        maxX = grid.gridSettings.horizontalCount;
        maxY = grid.gridSettings.verticalCount;
        this.grid = grid;
    }

    public MatchPattern(Vector2Int index1, Vector2Int index2, HexGrid grid)
    {
        this.index1 = index1;
        this.index2 = index2;
        maxX = grid.gridSettings.horizontalCount;
        maxY = grid.gridSettings.verticalCount;
        this.grid = grid;
    }

    public bool Match(Vector2Int targetIndex)
    {
        if (!CheckConstaint(targetIndex)) return false;
        var gridIndex1 = targetIndex + index1;
        if (!BoundsCheck(gridIndex1)) return false;
        var gridIndex2 = targetIndex + index2;
        if (!BoundsCheck(gridIndex2)) return false;
        var targetColor = grid.tilePositions[targetIndex].GetTile().tileColor;
        var tileColor1 = grid.tilePositions[gridIndex1].GetTile().tileColor;
        var tileColor2 = grid.tilePositions[gridIndex2].GetTile().tileColor;
        if (targetColor == tileColor1 && targetColor == tileColor2)
        {
            return true;
        }
        return false;
    }

    bool CheckConstaint(Vector2Int index)
    {
        switch (constraint)
        {
            case PatternConstraint.oddX:
                return index.x % 2 != 0;
            case PatternConstraint.evenX:
                return index.x % 2 == 0;
            case PatternConstraint.NoConstraint:
                return true;
        }

        return true;
    }

    bool BoundsCheck(Vector2Int index)
    {
        if (index.x < 0 || index.y < 0 || index.x >= maxX || index.y >= maxY) return false;
        return true;
    }

}
