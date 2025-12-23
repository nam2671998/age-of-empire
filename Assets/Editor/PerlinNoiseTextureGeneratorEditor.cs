using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PerlinNoiseTextureGenerator))]
public class PerlinNoiseTextureGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw default inspector
        DrawDefaultInspector();
        
        // Add space
        EditorGUILayout.Space();
        
        // Get the target script
        PerlinNoiseTextureGenerator generator = (PerlinNoiseTextureGenerator)target;
        
        // Create Apply button
        if (GUILayout.Button("Apply", GUILayout.Height(30)))
        {
            // Generate texture in edit mode
            generator.GenerateTexture();
            
            // Mark the object as dirty so Unity saves the changes
            EditorUtility.SetDirty(generator);
            
            // Also mark the scene as dirty
            if (!Application.isPlaying)
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                    UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
                );
            }
        }
        
        // Add a regenerate button as well
        EditorGUILayout.Space();
        if (GUILayout.Button("Regenerate", GUILayout.Height(25)))
        {
            generator.RegenerateTexture();
            EditorUtility.SetDirty(generator);
            if (!Application.isPlaying)
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                    UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
                );
            }
        }
    }
}

