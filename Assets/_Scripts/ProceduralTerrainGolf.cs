using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Terrain), typeof(TerrainCollider))]
public class ProceduralTerrainGolf : MonoBehaviour
{
    [HideInInspector] public Vector2Int gridPosition;
    [HideInInspector] public float terrainSize;
    
    [Header("Visuals")]
    public Material wallMaterial;
    public float wallHeight = 2f;
    public float wallThickness = 0.4f;

    [Header("Terrain Detail")]
    public int resolution = 129;
    public float maxHeight = 1.5f;

    public static Dictionary<Vector2Int, ProceduralTerrainGolf> AllTerrains = new Dictionary<Vector2Int, ProceduralTerrainGolf>();
    [HideInInspector] public float[] edgeTop, edgeBottom, edgeLeft, edgeRight;
    private bool hasGenerated = false;

    public void GenerateTerrain()
    {
        Terrain terrain = GetComponent<Terrain>();
        TerrainData data = new TerrainData { heightmapResolution = resolution, size = new Vector3(terrainSize, maxHeight, terrainSize) };

        float[,] heights = new float[resolution, resolution];
        for (int z = 0; z < resolution; z++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float worldX = (gridPosition.x * (resolution - 1) + x) / (float)resolution;
                float worldZ = (gridPosition.y * (resolution - 1) + z) / (float)resolution;
                heights[z, x] = Mathf.PerlinNoise(worldX * 4f, worldZ * 4f) * 0.5f;
            }
        }

        heights = StitchToNeighbors(heights);
        data.SetHeights(0, 0, heights);
        terrain.terrainData = data;
        GetComponent<TerrainCollider>().terrainData = data;
        CaptureEdges(heights);
        hasGenerated = true;
    }

    public void BuildWallsAndCorners()
    {
        float s = terrainSize;
        float h = wallHeight;
        float t = wallThickness;

        // Neighbor check
        bool N = AllTerrains.ContainsKey(gridPosition + Vector2Int.up);
        bool S = AllTerrains.ContainsKey(gridPosition + Vector2Int.down);
        bool W = AllTerrains.ContainsKey(gridPosition + Vector2Int.left);
        bool E = AllTerrains.ContainsKey(gridPosition + Vector2Int.right);

        // 1. Straight Walls
        if (!N) CreateWall("Wall_N", new Vector3(s/2, h/2, s), new Vector3(s, h, t), 0);
        if (!S) CreateWall("Wall_S", new Vector3(s/2, h/2, 0), new Vector3(s, h, t), 0);
        if (!W) CreateWall("Wall_W", new Vector3(0, h/2, s/2), new Vector3(t, h, s), 0);
        if (!E) CreateWall("Wall_E", new Vector3(s, h/2, s/2), new Vector3(t, h, s), 0);

        // 2. Diagonal "Backwards Triangle" Corners
        // Scale for the diagonal: wallThickness * 2 ensures it covers the gap
        float diagScale = t * 3f; 
        Vector3 cornerScale = new Vector3(diagScale, h, diagScale);

        if (!N && !W) CreateWall("Corner_NW", new Vector3(0, h/2, s), cornerScale, 45);
        if (!N && !E) CreateWall("Corner_NE", new Vector3(s, h/2, s), cornerScale, 45);
        if (!S && !W) CreateWall("Corner_SW", new Vector3(0, h/2, 0), cornerScale, 45);
        if (!S && !E) CreateWall("Corner_SE", new Vector3(s, h/2, 0), cornerScale, 45);
    }

    private void CreateWall(string n, Vector3 lp, Vector3 ls, float rotY)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = n;
        cube.transform.SetParent(transform);
        cube.transform.localPosition = lp;
        cube.transform.localScale = ls;
        cube.transform.localRotation = Quaternion.Euler(0, rotY, 0);
        if (wallMaterial) cube.GetComponent<MeshRenderer>().material = wallMaterial;
    }

    public void SpawnAtCenter(GameObject prefab, string name)
    {
        if (prefab == null) return;
        Instantiate(prefab, transform.position + new Vector3(terrainSize/2, 0.2f, terrainSize/2), Quaternion.identity, transform).name = name;
    }

    private float[,] StitchToNeighbors(float[,] heights)
    {
        int bWidth = Mathf.RoundToInt(resolution * 0.15f);
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        for (int i = 0; i < 4; i++)
        {
            if (AllTerrains.TryGetValue(gridPosition + dirs[i], out ProceduralTerrainGolf nb) && nb.hasGenerated)
            {
                for (int j = 0; j < resolution; j++)
                {
                    float targetH = (i == 0) ? nb.edgeBottom[j] : (i == 1) ? nb.edgeTop[j] : (i == 2) ? nb.edgeRight[j] : nb.edgeLeft[j];
                    for (int step = 0; step < bWidth; step++)
                    {
                        float blend = Mathf.SmoothStep(0, 1, (float)step / (bWidth - 1));
                        int z = (i == 0) ? (resolution - 1) - step : (i == 1) ? step : j;
                        int x = (i == 2) ? step : (i == 3) ? (resolution - 1) - step : j;
                        heights[z, x] = Mathf.Lerp(targetH, heights[z, x], blend);
                    }
                }
            }
        }
        return heights;
    }

    private void CaptureEdges(float[,] heights)
    {
        edgeBottom = new float[resolution]; edgeTop = new float[resolution];
        edgeLeft = new float[resolution];   edgeRight = new float[resolution];
        for (int i = 0; i < resolution; i++) {
            edgeBottom[i] = heights[0, i]; edgeTop[i] = heights[resolution - 1, i];
            edgeLeft[i] = heights[i, 0];   edgeRight[i] = heights[i, resolution - 1];
        }
    }
}