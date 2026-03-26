using UnityEngine;
using System.Collections.Generic;

public class GolfTerrainManager : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject terrainPrefab;
    public GameObject startPrefab;
    public GameObject holePrefab;

    [Header("Course Layout")]
    public List<Vector2Int> layout = new List<Vector2Int>();
    public float terrainSize = 20f;

    private List<ProceduralTerrainGolf> terrains = new List<ProceduralTerrainGolf>();

    void Start() => GenerateCourse();

    [ContextMenu("Generate Course")]
    public void GenerateCourse()
    {
        ClearCourse();
        if (terrainPrefab == null) return;

        // PASS 1: Placement
        for (int i = 0; i < layout.Count; i++)
        {
            Vector3 pos = transform.position + new Vector3(layout[i].x * terrainSize, 0, layout[i].y * terrainSize);
            GameObject obj = Instantiate(terrainPrefab, pos, Quaternion.identity, transform);
            
            ProceduralTerrainGolf script = obj.GetComponent<ProceduralTerrainGolf>();
            script.gridPosition = layout[i];
            script.terrainSize = terrainSize;

            ProceduralTerrainGolf.AllTerrains[script.gridPosition] = script;
            terrains.Add(script);
        }

        // PASS 2: Heights
        foreach (var t in terrains) t.GenerateTerrain();

        // PASS 3: Walls & Diagonal Corners
        foreach (var t in terrains) t.BuildWallsAndCorners();

        // PASS 4: Spawning
        if (terrains.Count > 0)
        {
            terrains[0].SpawnAtCenter(startPrefab, "StartPoint");
            terrains[terrains.Count - 1].SpawnAtCenter(holePrefab, "GoalPoint");
        }
    }

    [ContextMenu("Clear Course")]
    public void ClearCourse()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);

        terrains.Clear();
        ProceduralTerrainGolf.AllTerrains.Clear();
    }
}