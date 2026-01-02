using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class TransformSnapTools
{
    [MenuItem("CONTEXT/Transform/Snap Children To Center Cell")]
    private static void SnapChildrenToIntegerPosition(MenuCommand command)
    {
        if (command?.context is not Transform root)
        {
            return;
        }
        
        if (root.childCount <= 1)
        {
            return;
        }

        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Snap Children To Center Cell");

        for (int i = 0; i < root.childCount; i++)
        {
            Transform t = root.GetChild(i);
            if (t == null || t == root)
            {
                continue;
            }

            Undo.RecordObject(t, "Snap Children To Center Cell");

            Vector3 p = t.localPosition;
            Vector3 snapped = new Vector3(Mathf.Round(p.x), Mathf.Round(p.y), Mathf.Round(p.z));
            
            snapped.x += 0.5f;
            snapped.z += 0.5f;
            if (snapped != p)
            {
                t.localPosition = snapped;
                EditorUtility.SetDirty(t);
            }
        }

        Undo.CollapseUndoOperations(group);

        if (!Application.isPlaying && root.gameObject.scene.IsValid())
        {
            EditorSceneManager.MarkSceneDirty(root.gameObject.scene);
        }
    }
}

