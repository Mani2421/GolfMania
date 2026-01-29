using UnityEngine;

[RequireComponent(typeof(Terrain))]
public class ProceduralTerrainGolf : MonoBehaviour
{
    [Header("Terrain Settings")]
    public int heightmapResolution = 129;
    public float terrainSize = 20f;
    public float maxHeight = 1.5f;       // Max elevation
    public float perlinScale = 3f;       // Noise scale

    [Header("Golf Hole Settings")]
    public Transform ballStart;
    public Transform holeTarget;
    public int flatRadius = 3;           // Tiles to flatten around start/hole

    [Header("Obstacle Settings")]
    public GameObject obstaclePrefab;
    public int obstacleCount = 10;
    public float obstacleHeightOffset = 0.1f;

    private Terrain terrain;
    private TerrainData terrainData;
    public Material terrainMaterial;


    void Awake()
    {
        terrain = GetComponent<Terrain>();
        terrainData = new TerrainData();

        terrainData.heightmapResolution = heightmapResolution;
        terrainData.size = new Vector3(terrainSize, maxHeight, terrainSize);
        
        if (terrainMaterial != null)
        {
            terrain.materialTemplate = terrainMaterial;
        }

        terrain.terrainData = terrainData;

        GenerateTerrain();
    }

    void GenerateTerrain()
    {
        float[,] heights = new float[heightmapResolution, heightmapResolution];

        // Generate Perlin noise
        for (int x = 0; x < heightmapResolution; x++)
        {
            for (int z = 0; z < heightmapResolution; z++)
            {
                float nx = x / (float)heightmapResolution * perlinScale;
                float nz = z / (float)heightmapResolution * perlinScale;

                heights[x, z] = Mathf.PerlinNoise(nx, nz) * 0.5f;
            }
        }

        // Flatten Start and Hole areas
        FlattenArea(ref heights, ballStart.position, flatRadius);
        FlattenArea(ref heights, holeTarget.position, flatRadius);

        terrainData.SetHeights(0, 0, heights);

        // Spawn Obstacles
        PlaceObstacles();
    }

    void FlattenArea(ref float[,] heights, Vector3 worldPos, int radius)
    {
        Vector3 terrainSizeVec = terrainData.size;
        int hmWidth = terrainData.heightmapResolution;
        int hmHeight = terrainData.heightmapResolution;

        // Convert world position to heightmap coordinates
        int centerX = Mathf.RoundToInt((worldPos.x / terrainSizeVec.x) * (hmWidth - 1));
        int centerZ = Mathf.RoundToInt((worldPos.z / terrainSizeVec.z) * (hmHeight - 1));

        for (int x = -radius; x <= radius; x++)
        {
            for (int z = -radius; z <= radius; z++)
            {
                int hx = Mathf.Clamp(centerX + x, 0, hmWidth - 1);
                int hz = Mathf.Clamp(centerZ + z, 0, hmHeight - 1);

                heights[hx, hz] = 0.01f; // small flat value
            }
        }
    }

    void PlaceObstacles()
    {
        if (!obstaclePrefab) return;

        for (int i = 0; i < obstacleCount; i++)
        {
            float x = Random.Range(0f, terrainSize);
            float z = Random.Range(0f, terrainSize);

            // Avoid start and hole
            if (Vector3.Distance(new Vector3(x, 0, z), ballStart.position) < 2f ||
                Vector3.Distance(new Vector3(x, 0, z), holeTarget.position) < 2f)
                continue;

            float y = terrain.SampleHeight(new Vector3(x, 0, z));
            Vector3 pos = new Vector3(x, y + obstacleHeightOffset, z);

            Instantiate(obstaclePrefab, pos, Quaternion.identity);
        }
    }
    
    

    // regenerate terrain at runtime
    [ContextMenu("Regenerate Terrain")]
    public void Regenerate()
    {
        GenerateTerrain();
    }
}
