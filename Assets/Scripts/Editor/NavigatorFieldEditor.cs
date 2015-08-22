using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NavigationField))]
public class NavigationFieldEditor : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        NavigationField navigationField = (NavigationField)target;
        if (GUILayout.Button("It's even more fun to recompute!"))
        {
            navigationField.Populate();
            navigationField.transform.position += new Vector3(0, 0, 1);
            navigationField.transform.position -= new Vector3(0, 0, 1);
        }
    }
}
