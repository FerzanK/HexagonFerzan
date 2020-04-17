using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GridSettings
{
    public List<Color> colors;
    public int verticalCount;
    public int horizontalCount;
    public float offsetAmount;
    public int bombSpawnScore;
    public int starSpawnPercent;
    public int bombLife;
    public float tileSpeed;
    private Dictionary<Color, int> colorDict;
    private Dictionary<int, Color> colorIDDict;
    public int GetColorID(Color color)
    {
        if (colorDict == null) InitializeLookupDictionaries();
        return colorDict[color];
    }

    public void AddColor(Color color)
    {
        colors.Add(color);
    }

    private void InitializeLookupDictionaries()
    {
        colorDict = new Dictionary<Color, int>();
        colorIDDict = new Dictionary<int, Color>();
        for (int i = 0; i < colors.Count; i++)
        {
            colorDict.Add(colors[i], i);
            colorIDDict.Add(i, colors[i]);
        }
    }

    public Color GetColorWithID(int colorID)
    {
        if (colorIDDict == null) InitializeLookupDictionaries();
        return colorIDDict[colorID];
    }
}
