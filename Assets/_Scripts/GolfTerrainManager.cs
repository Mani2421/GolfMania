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

    [Header("Dynamic Difficulty")]
    public int levelLength = 5; 
    public GameObject[] obstaclePrefabs; 
    [Range(0, 1)] public float obstacleChance = 0.3f;

    private List<ProceduralTerrainGolf> terrains = new List<ProceduralTerrainGolf>(); //

    void Start() 
    {
        GenerateRandomLayout();
        GenerateCourse();
    }

    // Creates a random path of tiles for the layout
    public void GenerateRandomLayout()
    {
        layout.Clear();
        Vector2Int currentPos = Vector2Int.zero;
        layout.Add(currentPos); //

        for (int i = 0; i < levelLength; i++) //
        {
            Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right }; //
            Vector2Int nextPos = currentPos + directions[Random.Range(0, directions.Length)]; //

            // Avoid tiles doubling back on themselves
            if (!layout.Contains(nextPos)) //
            {
                layout.Add(nextPos); //
                currentPos = nextPos; //
            }
            else
            {
                i--; // Retry the segment if tile is already occupied
            }
        }
    }

    [ContextMenu("Generate Course")]
    public void GenerateCourse()
    {
        ClearCourse(); //
        if (terrainPrefab == null) return; //

        // PASS 1: Placement
        for (int i = 0; i < layout.Count; i++) //
        {
            Vector3 pos = transform.position + new Vector3(layout[i].x * terrainSize, 0, layout[i].y * terrainSize); //
            GameObject obj = Instantiate(terrainPrefab, pos, Quaternion.identity, transform); //
            
            ProceduralTerrainGolf script = obj.GetComponent<ProceduralTerrainGolf>(); //
            script.gridPosition = layout[i]; //
            script.terrainSize = terrainSize; //

            ProceduralTerrainGolf.AllTerrains[script.gridPosition] = script; //
            terrains.Add(script); //
        }

        // PASS 2: Heights (Stitching occurs here)
        foreach (var t in terrains) t.GenerateTerrain(); //

        // PASS 3: Walls & Diagonal Corners
        foreach (var t in terrains) t.BuildWallsAndCorners(); //

        // PASS 4: Spawning (Start and Goal)
        if (terrains.Count > 0) //
        {
            terrains[0].SpawnAtCenter(startPrefab, "StartPoint"); //
            terrains[terrains.Count - 1].SpawnAtCenter(holePrefab, "GoalPoint"); //
        }

        // PASS 5: Obstacles (Dynamic spawning based on current chance)
        // Skip index 0 and Count-1 to keep Start and Goal areas clear
        for (int i = 1; i < terrains.Count - 1; i++) //
        {
            terrains[i].SpawnObstacles(obstaclePrefabs, obstacleChance, 3); //
        }
    }

    [ContextMenu("Clear Course")]
    public void ClearCourse()
    {
        for (int i = transform.childCount - 1; i >= 0; i--) //
            DestroyImmediate(transform.GetChild(i).gameObject); //

        terrains.Clear(); //
        ProceduralTerrainGolf.AllTerrains.Clear(); //
    }
}