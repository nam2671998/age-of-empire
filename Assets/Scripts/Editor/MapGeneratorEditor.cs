using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapGenerator))]
public sealed class MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (Application.isPlaying)
            return;

        EditorGUILayout.Space();
        
        if (GUILayout.Button("Generate Map"))
        {
            ((MapGenerator)target).Generate();
            EditorUtility.SetDirty(target);
        }
    }
}
