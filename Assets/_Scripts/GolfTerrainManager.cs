using UnityEngine;
using System.Collections.Generic;

public class GolfTerrainManager : MonoBehaviour
{
    [Header("Terrain Prefab")]
    public GameObject terrainPrefab;

    [Header("Course Layout (Grid-Based)")]
    public TerrainStep[] layout;

    [Header("Course Settings")]
    public float terrainSpacing = 0f;

    [Header("Generation")]
    public bool generateOnStart = true;
    public int startSeed = 0; // 0 is random

    private List<ProceduralTerrainGolf> terrains = new List<ProceduralTerrainGolf>();

    [System.Serializable]
    public struct TerrainStep
    {
        public int x; // left/right
        public int z; // forward/back
    }

    void Start()
    {
        if (generateOnStart)
        {
            GenerateCourse();
        }
    }

    [ContextMenu("Generate Course")]
    public void GenerateCourse()
    {
        ClearCourse();

        if (terrainPrefab == null)
        {
            Debug.LogError("Terrain prefab is not assigned!");
            return;
        }

        ProceduralTerrainGolf terrainScript = terrainPrefab.GetComponent<ProceduralTerrainGolf>();
        if (terrainScript == null)
        {
            Debug.LogError("Terrain prefab must have ProceduralTerrainGolf component!");
            return;
        }

        float terrainSize = terrainScript.terrainSize;
        float spacing = terrainSize + terrainSpacing;

        for (int i = 0; i < layout.Length; i++)
        {
            TerrainStep step = layout[i];

            Vector3 position = transform.position +
                new Vector3(step.x * spacing, 0f, step.z * spacing);

            GameObject terrainObj = Instantiate(
                terrainPrefab,
                position,
                Quaternion.identity,
                transform
            );

            terrainObj.name = $"Terrain{i + 1}";

            // Layer used for camera collision
            int terrainLayer = LayerMask.NameToLayer("Terrain");
            terrainObj.layer = terrainLayer;

            ProceduralTerrainGolf terrain =
                terrainObj.GetComponent<ProceduralTerrainGolf>();

            terrains.Add(terrain);

            // Chain to previous terrain
            if (i > 0)
            {
                terrains[i - 1].ChainToTerrain(terrain);
            }
        }

        // Build walls AFTER chaining is complete
        foreach (var terrain in terrains)
        {
            terrain.BuildWalls();
        }

        Debug.Log($"Generated {layout.Length} terrain pieces for the golf course!");
    }

    

    [ContextMenu("Clear Course")]
    public void ClearCourse()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        terrains.Clear();
    }

    public ProceduralTerrainGolf GetTerrain(int index)
    {
        if (index >= 0 && index < terrains.Count)
            return terrains[index];

        return null;
    }

    // public Vector3 GetHoleStartPosition(int holeIndex)
    // {
    //     ProceduralTerrainGolf terrain = GetTerrain(holeIndex);
    //     if (terrain != null)
    //         return terrain.GetStartWorldPosition();

    //     return Vector3.zero;
    // }

    // public Vector3 GetHoleEndPosition(int holeIndex)
    // {
    //     ProceduralTerrainGolf terrain = GetTerrain(holeIndex);
    //     if (terrain != null)
    //         return terrain.GetHoleWorldPosition();

    //     return Vector3.zero;
    // }
}