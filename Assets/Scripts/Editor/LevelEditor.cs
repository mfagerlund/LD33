using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Level))]
public class LevelEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Level level = (Level)target;
        if (GUILayout.Button("Rebuild ais"))
        {
            level.RebuildAis();
        }
    }
}