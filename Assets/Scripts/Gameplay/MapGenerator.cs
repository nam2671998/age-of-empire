using System;
using System.Collections.Generic;
using UnityEngine;

// Create predictable, fair starting areas for up to 8 factions.
// Avoid factions spawning on the same row/column/diagonal by using an N-Queens layout.
// Make resource distribution easy to reason about by spawning inside fixed 3x3 sections.
//
// The map is treated as a square grid of mapSizeCells * mapSizeCells.
// The grid is split into n * n "regions" (like a chessboard), where n = factionCount.
// Starter region of each faction is put into a region using an N-Queens placement (one per row, unique column, no diagonals). For exception for n <= 3, they are placed diagonal
// Each selected region gets:
// - A castle at its center (section 4 of a 3x3 split).
// - Dense resources in 3 out of the 4 corner sections (0, 2, 6, 8).
// - Animals anywhere in the region except the center section.

/// <summary>
/// Generates a simple procedural "starter layout" for an RTS map.
/// </summary>
public class MapGenerator : MonoBehaviour
{
    [SerializeField, Range(1, 8)] private int factionCount = 2;
    [SerializeField] private int mapSizeCells = 1000;
    [SerializeField] private int seed;
    [SerializeField] private bool useRandomSeed = true;
    [SerializeField] private Transform generatedRoot;

    [Header("Prefabs")]
    [SerializeField] private GameObject castlePrefab;
    [SerializeField] private GameObject foodTreePrefab;
    [SerializeField] private GameObject treePrefab;
    [SerializeField] private GameObject rockPrefab;
    [SerializeField] private GameObject animalPrefab;
    [SerializeField] private GameObject barbarianPrefab;

    private void Start()
    {
        Generate();
    }

    public void Generate()
    {
        int n = Mathf.Clamp(factionCount, 1, 8);
        int mapSize = Mathf.Max(1, mapSizeCells);

        int actualSeed = useRandomSeed ? Environment.TickCount : seed;
        var rng = new System.Random(actualSeed);

        EnsureGeneratedRoot();
        ClearGeneratedRoot();

        if (!TryPlaceCastles(rng, n, out int[] colsByRow))
            return;

        int k = Mathf.Max(1, mapSize / n);
        int sectionBaseSize = Mathf.Max(1, k / 3);

        int foodTreeCount = k;
        int treeCount = k;
        int rockCount = k;
        int animalCount = k / n;

        // Cache starter regions for fast lookup
        bool[][] isStarterRegion = new bool[n][];
        for (int index = 0; index < n; index++)
        {
            isStarterRegion[index] = new bool[n];
        }

        for (int r = 0; r < n; r++)
            isStarterRegion[r][colsByRow[r]] = true;
        Transform firstCastle = null;

        for (int row = 0; row < n; row++)
        {
            for (int col = 0; col < n; col++)
            {
                int regionOriginX = col * k;
                int regionOriginZ = row * k;

                bool hasCastle = isStarterRegion[row][col];

                if (hasCastle)
                {
                    // Castle placement
                    Vector3 castlePos = new Vector3(
                        regionOriginX + (k * 0.5f),
                        0f,
                        regionOriginZ + (k * 0.5f)
                    );
                    GameObject castle = SpawnPrefab(castlePrefab, castlePos, "Castle", generatedRoot);
                    if (firstCastle == null)
                    {
                        firstCastle = castle.transform;
                        
                        GetSectionBounds(regionOriginX, regionOriginZ, k, sectionBaseSize,
                            new[] {1, 3, 5, 7}[rng.Next(4)], out int minX, out int minZ, out int maxX, out int maxZ);
                        SpawnRandomInBounds(rng, animalPrefab, 5, minX, minZ, maxX, maxZ, generatedRoot);
                        SpawnRandomInBounds(rng, barbarianPrefab, 5, minX, minZ, maxX, maxZ, generatedRoot);
                    }
                    foreach (var factionOwner in castle.GetComponents<IFactionOwner>())
                    {
                        factionOwner.Faction = (Faction)row;
                    }

                    // Dense corner sections
                    int[] densed = { 0, 2, 6, 8 };
                    Shuffle(rng, densed);

                    for (int i = 0; i < 3; i++)
                    {
                        int sectionIndex = densed[i];
                        GetSectionBounds(regionOriginX, regionOriginZ, k, sectionBaseSize,
                            sectionIndex, out int minX, out int minZ, out int maxX, out int maxZ);

                        switch (i)
                        {
                            case 0:
                                SpawnRandomInBounds(rng, foodTreePrefab, foodTreeCount, minX, minZ, maxX, maxZ,
                                    generatedRoot);
                                break;
                            case 1:
                                SpawnRandomInBounds(rng, treePrefab, treeCount, minX, minZ, maxX, maxZ, generatedRoot);
                                break;
                            case 2:
                                SpawnRandomInBounds(rng, rockPrefab, rockCount, minX, minZ, maxX, maxZ, generatedRoot);
                                break;
                        }
                    }
                }
                else
                {
                    // In regions that doesn't have castle, any section have 25% chance to be a minor-dense section. A minor-dense section spawn 1 of 3 resource type with 1/3 density with dense section.
                    // NON-STARTER REGION: minor-dense logic
                    for (int section = 0; section < 9; section++)
                    {
                        // 25% chance
                        if (rng.NextDouble() > 0.25)
                            continue;

                        GetSectionBounds(regionOriginX, regionOriginZ, k, sectionBaseSize,
                            section, out int minX, out int minZ, out int maxX, out int maxZ);

                        int resourceType = rng.Next(3);
                        int minorCount;

                        switch (resourceType)
                        {
                            case 0:
                                minorCount = foodTreeCount / 3;
                                SpawnRandomInBounds(rng, foodTreePrefab, minorCount, minX, minZ, maxX, maxZ,
                                    generatedRoot);
                                break;
                            case 1:
                                minorCount = treeCount / 3;
                                SpawnRandomInBounds(rng, treePrefab, minorCount, minX, minZ, maxX, maxZ, generatedRoot);
                                break;
                            default:
                                minorCount = rockCount / 3;
                                SpawnRandomInBounds(rng, rockPrefab, minorCount, minX, minZ, maxX, maxZ, generatedRoot);
                                break;
                        }
                    }
                }
            }
        }

        // Animals spawn anywhere
        SpawnRandomInBounds(rng, animalPrefab, animalCount, 0, 0, mapSize, mapSize, generatedRoot);

        if (!Application.isPlaying || firstCastle == null)
        {
            Debug.LogError("Something is wrong. No Castle placed");
            return;
        }
        FindAnyObjectByType<CameraFocusController>().Focus(firstCastle.gameObject, true);
    }


    private void EnsureGeneratedRoot()
    {
        // The root transform is the single "handle" used for cleanup and organization.
        if (generatedRoot != null)
        {
            return;
        }

        var go = new GameObject("GeneratedMap");
        go.transform.SetParent(transform, false);
        generatedRoot = go.transform;
    }

    private void ClearGeneratedRoot()
    {
        // Supports both edit-mode generation (Undo-aware) and runtime regeneration (Destroy).
        if (generatedRoot == null)
        {
            return;
        }

        for (int i = generatedRoot.childCount - 1; i >= 0; i--)
        {
            Transform child = generatedRoot.GetChild(i);
            if (child != null)
            {
                Destroy(child.gameObject);
            }
        }
    }

    private static void SpawnRandomInBounds(System.Random rng, GameObject prefab, int count, int minX, int minZ, int maxX, int maxZ, Transform parent)
    {
        // Spawns 'count' instances randomly within the provided axis-aligned bounds.
        if (prefab == null || count <= 0)
        {
            return;
        }

        int width = Mathf.Max(1, maxX - minX);
        int height = Mathf.Max(1, maxZ - minZ);

        for (int i = 0; i < count; i++)
        {
            float x = minX + rng.Next(0, width);
            float z = minZ + rng.Next(0, height);
            SpawnPrefab(prefab, new Vector3(x, 0f, z), prefab.name, parent);
        }
    }

    private static void GetSectionBounds(int regionOriginX, int regionOriginZ, int k, int sectionBaseSize, int sectionIndex, out int minX, out int minZ, out int maxX, out int maxZ)
    {
        // Section index mapping:
        // 6 7 8
        // 3 4 5
        // 0 1 2
        int sx = sectionIndex % 3;
        int sz = sectionIndex / 3;

        minX = regionOriginX + (sx * sectionBaseSize);
        minZ = regionOriginZ + (sz * sectionBaseSize);

        // The last section on each axis extends to the region edge to absorb remainder from integer division.
        maxX = sx == 2 ? regionOriginX + k : regionOriginX + ((sx + 1) * sectionBaseSize);
        maxZ = sz == 2 ? regionOriginZ + k : regionOriginZ + ((sz + 1) * sectionBaseSize);
    }

    private static void Shuffle(System.Random rng, int[] array)
    {
        // Fisher-Yates shuffle to randomize selections without bias.
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = rng.Next(0, i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }
    }

    /// <summary>
    /// Finds an N-Queens solution to place castles, used to select starter regions. Just a heuristical way to make players not too close to others.
    /// </summary>
    private bool TryPlaceCastles(System.Random rng, int n, out int[] colsByRow)
    {
        if (n <= 3)
        {
            colsByRow = new int[4];
            for (int i = 0; i < n; i++)
            {
                colsByRow[i] = i;
            }
            return true;
        }

        int[] castles = new int[n];
        Array.Fill(castles, -1);

        int[] colCandidates = new int[n];
        for (int i = 0; i < n; i++) colCandidates[i] = i;

        bool foundSolution = TryPlace(castles, 0, n);
        colsByRow = foundSolution ? castles : null;
        return foundSolution;

        bool TryPlace(int[] castlePositions, int row, int totalFaction)
        {
            if (row == totalFaction)
            {
                return true; // Placed all castles. Complete a solution
            }

            // Shuffle column candidates to ensure randomness in map generation
            Shuffle(rng, colCandidates);

            for (int i = 0; i < totalFaction; i++)
            {
                int column = colCandidates[i];
                if (IsValid(castlePositions, row, column))
                {
                    castlePositions[row] = column;

                    // Try to place next castle in next row
                    if (TryPlace(castlePositions, row + 1, totalFaction))
                    {
                        // Found a solution. Stop
                        return true;
                    }

                    // Backtrack
                    castlePositions[row] = -1;
                }
            }

            return false;
        }

        bool IsValid(int[] castlePositions, int row, int column)
        {
            for (int i = 0; i < row; i++)
            {
                if (castlePositions[i] == column)
                {
                    return false; // Same column
                }

                if (Math.Abs(castlePositions[i] - column) == row - i)
                {
                    return false; // Same diagonal
                }
            }

            return true;
        }
    }

    private static GameObject SpawnPrefab(GameObject prefab, Vector3 position, string name, Transform parent)
    {
        if (prefab == null)
        {
            return null;
        }

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            GameObject instance = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab);
            if (instance != null)
            {
                instance.transform.SetParent(parent, false);
                instance.transform.position = position;
                if (!string.IsNullOrWhiteSpace(name))
                {
                    instance.name = name;
                }
            }
            return instance;
        }
#endif

        GameObject runtimeInstance = Instantiate(prefab, position, Quaternion.identity, parent);
        if (!string.IsNullOrWhiteSpace(name))
        {
            runtimeInstance.name = name;
        }
        return runtimeInstance;
    }
}
