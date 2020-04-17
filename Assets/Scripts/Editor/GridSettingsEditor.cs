using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(HexGrid)), CanEditMultipleObjects]
public class GridSettingsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        HexGrid myScript = GameObject.Find("Grid").GetComponent<HexGrid>();
        if (GUILayout.Button("Add Random Color"))
        {
            var R = Random.Range(0.0f, 1.0f);
            var G = Random.Range(0.0f, 1.0f);
            var B = Random.Range(0.0f, 1.0f);
            var randomColor = new Color(R, G, B, 1);
            myScript.gridSettings.AddColor(randomColor);
        }
        DrawDefaultInspector();
    }
}