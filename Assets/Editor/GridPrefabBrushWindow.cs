#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GridPrefabBrushWindow : EditorWindow
{
    private enum BrushMode
    {
        Paint = 0,
        Erase = 1,
    }

    [SerializeField] private GameObject prefab;
    [SerializeField] private Transform parent;
    [SerializeField] private bool isPaintingEnabled;
    [SerializeField] private BrushMode mode;
    [SerializeField] private bool useGridManagerCellSize = true;
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private int frequency = 1;

    private Vector2Int? lastPaintedCell;
    private Vector2Int? lastMouseCell;
    private BrushMode lastMode;

    [MenuItem("Tools/Grid Prefab Brush")]
    public static void Open()
    {
        GetWindow<GridPrefabBrushWindow>("Grid Prefab Brush");
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += DuringSceneGui;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= DuringSceneGui;
    }

    private void OnGUI()
    {
        prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", prefab, typeof(GameObject), false);
        parent = (Transform)EditorGUILayout.ObjectField("Parent", parent, typeof(Transform), true);
        isPaintingEnabled = EditorGUILayout.Toggle("Enable Painting", isPaintingEnabled);
        mode = (BrushMode)EditorGUILayout.EnumPopup("Mode", mode);
        if (mode != lastMode)
        {
            lastMode = mode;
            lastMouseCell = null;
            lastPaintedCell = null;
            SceneView.RepaintAll();
        }

        useGridManagerCellSize = EditorGUILayout.Toggle("Use GridManager Cell Size", useGridManagerCellSize);
        using (new EditorGUI.DisabledScope(useGridManagerCellSize))
        {
            cellSize = EditorGUILayout.FloatField("Cell Size", cellSize);
        }

        using (new EditorGUI.DisabledScope(mode != BrushMode.Paint))
        {
            frequency = EditorGUILayout.IntField("Frequency", frequency);
        }

        if (GUILayout.Button("Reset Brush State"))
        {
            lastPaintedCell = null;
            lastMouseCell = null;
            SceneView.RepaintAll();
        }
    }

    private void DuringSceneGui(SceneView sceneView)
    {
        if (!isPaintingEnabled || prefab == null)
            return;

        Event e = Event.current;
        if (e == null)
            return;

        if (useGridManagerCellSize)
        {
            GridManager gridManager = Object.FindObjectOfType<GridManager>();
            if (gridManager != null)
            {
                SerializedObject so = new SerializedObject(gridManager);
                SerializedProperty cellSizeProp = so.FindProperty("cellSize");
                if (cellSizeProp != null && cellSizeProp.propertyType == SerializedPropertyType.Float)
                {
                    float v = cellSizeProp.floatValue;
                    if (v > 0f)
                        cellSize = v;
                }
            }
        }

        float effectiveCellSize = cellSize > 0f ? cellSize : 1f;
        int effectiveFrequency = Mathf.Max(1, frequency);

        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        if (!plane.Raycast(ray, out float distance))
            return;

        Vector3 world = ray.GetPoint(distance);
        Vector2Int cell = WorldToGrid(world, effectiveCellSize);
        Vector3 snapped = GridToWorld(cell, effectiveCellSize);

        if (mode == BrushMode.Paint)
        {
            Handles.color = new Color(0f, 1f, 1f, 0.9f);
            float radius = effectiveFrequency * effectiveCellSize;
            Handles.DrawWireDisc(snapped + Vector3.up * 0.01f, Vector3.up, radius);
        }

        Handles.color = mode == BrushMode.Paint ? new Color(0f, 1f, 0f, 0.9f) : new Color(1f, 0f, 0f, 0.9f);
        float half = effectiveCellSize * 0.5f;
        Vector3[] verts =
        {
            new Vector3(snapped.x - half, 0.01f, snapped.z - half),
            new Vector3(snapped.x - half, 0.01f, snapped.z + half),
            new Vector3(snapped.x + half, 0.01f, snapped.z + half),
            new Vector3(snapped.x + half, 0.01f, snapped.z - half),
        };
        Color fill = mode == BrushMode.Paint ? new Color(0f, 1f, 0f, 0.05f) : new Color(1f, 0f, 0f, 0.05f);
        Color outline = mode == BrushMode.Paint ? new Color(0f, 1f, 0f, 0.9f) : new Color(1f, 0f, 0f, 0.9f);
        Handles.DrawSolidRectangleWithOutline(verts, fill, outline);

        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

        bool wantsAction = (e.type == EventType.MouseDown || e.type == EventType.MouseDrag) && e.button == 0 && !e.alt;
        if (!wantsAction)
        {
            if (e.type == EventType.MouseUp && e.button == 0)
            {
                lastMouseCell = null;
            }
            return;
        }

        if (lastMouseCell.HasValue && lastMouseCell.Value == cell)
        {
            e.Use();
            return;
        }

        lastMouseCell = cell;

        if (mode == BrushMode.Paint)
        {
            bool shouldPaint = !lastPaintedCell.HasValue || GridDistance(lastPaintedCell.Value, cell) >= effectiveFrequency;
            if (shouldPaint)
            {
                CreatePrefabInstance(snapped);
                lastPaintedCell = cell;
            }
        }
        else
        {
            ErasePrefabsInCell(cell, effectiveCellSize);
        }

        e.Use();
    }

    private void CreatePrefabInstance(Vector3 position)
    {
        Object created = PrefabUtility.InstantiatePrefab(prefab);
        GameObject go = created as GameObject;
        if (go == null)
            return;

        Undo.RegisterCreatedObjectUndo(go, "Paint Prefab");
        if (parent != null)
            Undo.SetTransformParent(go.transform, parent, "Paint Prefab");

        go.transform.position = new Vector3(position.x, 0f, position.z);
        go.transform.rotation = Quaternion.identity;
    }

    private void ErasePrefabsInCell(Vector2Int cell, float gridCellSize)
    {
        GameObject prefabAsset = PrefabUtility.GetCorrespondingObjectFromSource(prefab) as GameObject ?? prefab;

        var rootsToDelete = new HashSet<GameObject>();

        if (parent != null)
        {
            foreach (Transform t in parent.GetComponentsInChildren<Transform>(true))
            {
                if (t == null)
                    continue;

                GameObject go = t.gameObject;
                GameObject root = PrefabUtility.GetOutermostPrefabInstanceRoot(go);
                if (root == null || root != go)
                    continue;

                if (!IsMatchingPrefabInCell(root, prefabAsset, cell, gridCellSize))
                    continue;

                rootsToDelete.Add(root);
            }
        }
        else
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid())
                return;

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                CollectMatchingPrefabRoots(root, prefabAsset, cell, gridCellSize, rootsToDelete);
            }
        }

        if (rootsToDelete.Count == 0)
            return;

        foreach (GameObject go in rootsToDelete)
        {
            Undo.DestroyObjectImmediate(go);
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }

    private void CollectMatchingPrefabRoots(GameObject current, GameObject prefabAsset, Vector2Int cell, float gridCellSize, HashSet<GameObject> results)
    {
        if (current == null)
            return;

        GameObject root = PrefabUtility.GetOutermostPrefabInstanceRoot(current);
        if (root == current && IsMatchingPrefabInCell(root, prefabAsset, cell, gridCellSize))
        {
            results.Add(root);
            return;
        }

        Transform tr = current.transform;
        int childCount = tr.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Transform child = tr.GetChild(i);
            if (child != null)
                CollectMatchingPrefabRoots(child.gameObject, prefabAsset, cell, gridCellSize, results);
        }
    }

    private bool IsMatchingPrefabInCell(GameObject prefabInstanceRoot, GameObject prefabAsset, Vector2Int cell, float gridCellSize)
    {
        if (prefabInstanceRoot == null)
            return false;

        if (!PrefabUtility.IsPartOfPrefabInstance(prefabInstanceRoot))
            return false;

        GameObject source = PrefabUtility.GetCorrespondingObjectFromSource(prefabInstanceRoot);
        if (source == null)
            return false;

        if (source != prefabAsset)
            return false;

        Vector2Int rootCell = WorldToGrid(prefabInstanceRoot.transform.position, gridCellSize);
        return rootCell == cell;
    }

    private static Vector2Int WorldToGrid(Vector3 worldPosition, float gridCellSize)
    {
        int x = Mathf.RoundToInt(worldPosition.x / gridCellSize);
        int z = Mathf.RoundToInt(worldPosition.z / gridCellSize);
        return new Vector2Int(x, z);
    }

    private static Vector3 GridToWorld(Vector2Int gridPosition, float gridCellSize)
    {
        return new Vector3(gridPosition.x * gridCellSize, 0f, gridPosition.y * gridCellSize);
    }

    private static int GridDistance(Vector2Int a, Vector2Int b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dz = Mathf.Abs(a.y - b.y);
        return Mathf.Max(dx, dz);
    }
}
#endif
