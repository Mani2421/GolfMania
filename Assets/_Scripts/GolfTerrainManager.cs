using UnityEngine;
using System.Collections.Generic;

public class GolfTerrainManager : MonoBehaviour
{
    [Header("Terrain Prefab")]
    public GameObject terrainPrefab;

    [Header("Course Settings")]
    public int numberOfTerrains = 3;
    public float terrainSpacing = 0f;
    public Vector3 chainDirection = Vector3.forward; // Direction to place next terrain

    [Header("Generation")]
    public bool generateOnStart = true;
    public int startSeed = 0; // 0 is random

    private List<ProceduralTerrainGolf> terrains = new List<ProceduralTerrainGolf>();

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
        Vector3 offset = chainDirection.normalized * (terrainSize + terrainSpacing);

        for (int i = 0; i < numberOfTerrains; i++)
        {
            Vector3 position = transform.position + (offset * i);
            GameObject terrainObj = Instantiate(terrainPrefab, position, Quaternion.identity, transform);
            terrainObj.name = $"Terrain{i + 1}";
            // Layer used for cam collision
            int terrainLayer = LayerMask.NameToLayer("Terrain");
            terrainObj.layer = terrainLayer;

            ProceduralTerrainGolf terrain = terrainObj.GetComponent<ProceduralTerrainGolf>();
            
            // if (startSeed != 0)
            // {
            //     terrain.seed = startSeed + i;
            // }

            terrains.Add(terrain);

            // Chain to previous terrain
            if (i > 0)
            {
                terrains[i - 1].ChainToTerrain(terrain);
            }
        }

        Debug.Log($"Generated {numberOfTerrains} terrain pieces for the golf course!");
    }

    [ContextMenu("Clear Course")]
    public void ClearCourse()
    {
        // Destroy all child terrain objects
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
        terrains.Clear();
    }

    public ProceduralTerrainGolf GetTerrain(int index)
    {
        if (index >= 0 && index < terrains.Count)
        {
            return terrains[index];
        }
        return null;
    }
    
    public Vector3 GetHoleStartPosition(int holeIndex)
    {
        ProceduralTerrainGolf terrain = GetTerrain(holeIndex);
        if (terrain != null)
        {
            return terrain.GetStartWorldPosition();
        }
        return Vector3.zero;
    }

    public Vector3 GetHoleEndPosition(int holeIndex)
    {
        ProceduralTerrainGolf terrain = GetTerrain(holeIndex);
        if (terrain != null)
        {
            return terrain.GetHoleWorldPosition();
        }
        return Vector3.zero;
    }
}