using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSearch<T> where T : IGridItem
{
    readonly Dictionary<Vector2Int, List<T>> gridItems = new Dictionary<Vector2Int,List<T>>();

    void Add(Vector2 position, T item)
    {
        var gridIndex = ConvertToIndex(position);
        if (gridItems.ContainsKey(gridIndex)) gridItems[gridIndex].Add(item);
        else
        {
            gridItems.Add(gridIndex, new List<T>(){item});
        }
    }

    public void Add(T item)
    {
        Add(item.GetPosition(), item);
    }

    List<T> GetItems(Vector2 position)
    {
        var gridIndex = ConvertToIndex(position);
        return gridItems[gridIndex];
    }

    public T Search(Vector2 position)
    {
        var gridIndex = ConvertToIndex(position);
        float shortestDist = float.MaxValue;
        T closestItem = default(T);
        int numberOfChecks = 0;
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                var targetIndex = new Vector2Int(gridIndex.x + x, gridIndex.y + y);
                if(targetIndex.x < 0 || targetIndex.y < 0 || !gridItems.ContainsKey(targetIndex)) continue;
                foreach (var item in gridItems[targetIndex])
                {
                    var itemPos = item.GetPosition();
                    var dist = Vector2.Distance(position, itemPos);
                    numberOfChecks++;
                    if (dist < shortestDist)
                    {
                        shortestDist = dist;
                        closestItem = item;
                    }
                }
            }
        }
        Debug.Log("Number of grid checks:" + numberOfChecks);
        return closestItem;
    }

    Vector2Int ConvertToIndex(Vector2 position)
    {
        return new Vector2Int((int)(position.x*2), (int)(position.y*2));
    }
}
