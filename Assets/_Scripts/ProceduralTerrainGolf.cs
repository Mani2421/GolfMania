using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Terrain))]
public class ProceduralTerrainGolf : MonoBehaviour
{
    [Header("Terrain Settings")]
    public int heightmapResolution = 129;
    public float terrainSize = 20f;
    public float maxHeight = 1.5f;
    public float perlinScale = 3f;
    public int seed = 0; // 0 is random seed

    [Header("Golf Hole Prefabs")]
    public GameObject startPrefab;
    public GameObject holePrefab;
    
    [Header("Spawn Positions (Local)")]
    public Vector3 startPosition = new Vector3(2f, 0f, 2f);
    public Vector3 holePosition = new Vector3(18f, 0f, 18f);

    [Header("Obstacle Settings")]
    public GameObject obstaclePrefab;
    public int obstacleCount = 10;
    public float obstacleHeightOffset = 0.1f;
    public float minDistanceFromTargets = 2f;

    [Header("Terrain Chaining")]
    public bool isChained = false;
    public ProceduralTerrainGolf nextTerrain;
    
    [Header("Materials")]
    public List<Material> randomTerrainMaterialList;
    public Material terrainMaterial;

    private Terrain terrain;
    private TerrainData terrainData;
    private GameObject spawnedStart;
    private GameObject spawnedHole;
    private List<GameObject> spawnedObstacles = new List<GameObject>();

    void Awake()
    {
        terrain = GetComponent<Terrain>();
        
        // Create new terrain data
        terrainData = new TerrainData();
        terrainData.heightmapResolution = heightmapResolution;
        terrainData.size = new Vector3(terrainSize, maxHeight, terrainSize);
        
//        if (terrainMaterial != null)
//        {
            terrain.materialTemplate = GetRandomMaterials();
//        }

        terrain.terrainData = terrainData;
        
        // Add TerrainCollider component if not present
        TerrainCollider terrainCollider = GetComponent<TerrainCollider>();
        if (terrainCollider == null)
        {
            terrainCollider = gameObject.AddComponent<TerrainCollider>();
        }
        terrainCollider.terrainData = terrainData;

        GenerateTerrain();
    }

    void GenerateTerrain()
    {
        // Set random seed if it's set to 0, which is the default
        if (seed != 0)
        {
            Random.InitState(seed);
        }

        float[,] heights = new float[heightmapResolution, heightmapResolution];

        // Generate Perlin noise
        float offsetX = Random.Range(0f, 1000f);
        float offsetZ = Random.Range(0f, 1000f);

        for (int x = 0; x < heightmapResolution; x++)
        {
            for (int z = 0; z < heightmapResolution; z++)
            {
                float nx = (x / (float)heightmapResolution * perlinScale) + offsetX;
                float nz = (z / (float)heightmapResolution * perlinScale) + offsetZ;

                heights[x, z] = Mathf.PerlinNoise(nx, nz);
            }
        }
        
        // Blend edges if this terrain is chained
        if (isChained)
        {
            // Determine which edges to blend based on chain direction
            // This assumes terrains are chained horizontally to the right
            BlendEdges(ref heights, blendLeft: false, blendRight: true, blendForward: false, blendBack: false);
       }

        terrainData.SetHeights(0, 0, heights);

        // Spawn start and hole prefabs
        SpawnTargets();

        // Spawn obstacles
        PlaceObstacles();
    }

    void SpawnTargets()
    {
        // Clear previous spawns
        if (spawnedStart != null)
            Destroy(spawnedStart);
        if (spawnedHole != null)
            Destroy(spawnedHole);

        // Spawn start prefab
        if (startPrefab != null)
        {
            Vector3 worldStart = transform.position + startPosition;
            float startY = terrain.SampleHeight(worldStart) + transform.position.y;
            worldStart.y = startY;
            
            spawnedStart = Instantiate(startPrefab, worldStart, Quaternion.identity, transform);
        }

        // Spawn hole prefab
        if (holePrefab != null)
        {
            Vector3 worldHole = transform.position + holePosition;
            float holeY = terrain.SampleHeight(worldHole) + transform.position.y;
            worldHole.y = holeY;
            
            spawnedHole = Instantiate(holePrefab, worldHole, Quaternion.identity, transform);
        }
    }

    void PlaceObstacles()
    {
        // Clear previous obstacles
        foreach (GameObject obstacle in spawnedObstacles)
        {
            if (obstacle != null)
                Destroy(obstacle);
        }
        spawnedObstacles.Clear();

        if (!obstaclePrefab) return;

        Vector3 worldStart = transform.position + startPosition;
        Vector3 worldHole = transform.position + holePosition;

        int attempts = 0;
        int maxAttempts = obstacleCount * 10;

        while (spawnedObstacles.Count < obstacleCount && attempts < maxAttempts)
        {
            attempts++;

            float x = Random.Range(0f, terrainSize);
            float z = Random.Range(0f, terrainSize);
            Vector3 localPos = new Vector3(x, 0, z);
            Vector3 worldPos = transform.position + localPos;

            // Check distance from start and hole
            if (Vector3.Distance(worldPos, worldStart) < minDistanceFromTargets ||
                Vector3.Distance(worldPos, worldHole) < minDistanceFromTargets)
            {
                continue;
            }

            float y = terrain.SampleHeight(worldPos) + transform.position.y;
            Vector3 spawnPos = new Vector3(worldPos.x, y + obstacleHeightOffset, worldPos.z);

            GameObject obstacle = Instantiate(obstaclePrefab, spawnPos, Quaternion.identity, transform);
            spawnedObstacles.Add(obstacle);
        }
    }

    private Material GetRandomMaterials()
    {
        int randomMaterial = Random.Range(0, randomTerrainMaterialList.Count);

        terrainMaterial = randomTerrainMaterialList[randomMaterial];

        return terrainMaterial;
    }

    // Get the world position of the start
    public Vector3 GetStartWorldPosition()
    {
        Vector3 worldStart = transform.position + startPosition;
        float y = terrain.SampleHeight(worldStart) + transform.position.y;
        return new Vector3(worldStart.x, y, worldStart.z);
    }

    // Get the world position of the hole
    public Vector3 GetHoleWorldPosition()
    {
        Vector3 worldHole = transform.position + holePosition;
        float y = terrain.SampleHeight(worldHole) + transform.position.y;
        return new Vector3(worldHole.x, y, worldHole.z);
    }
    
    // TODO: FIX THIS
    void BlendEdges(ref float[,] heights, bool blendRight, bool blendLeft, bool blendForward, bool blendBack)
    {
        int res = heightmapResolution;
        int blendWidth = 100; // Number of samples to blend

        for (int i = 0; i < blendWidth; i++)
        {
            float blend = i / (float)blendWidth; // 0 to 1
        
            if (blendRight)
            {
                Debug.Log("Blended?");
                for (int z = 0; z < res; z++)
                    heights[res - 1 - i, z] = Mathf.Lerp(0.5f, heights[res - 1 - i, z], blend);
            }
        }
    }

    // Chain this terrain to another
    public void ChainToTerrain(ProceduralTerrainGolf next)
    {
        nextTerrain = next;
        isChained = true;
    }

    [ContextMenu("Regenerate Terrain")]
    public void Regenerate()
    {
        GenerateTerrain();
    }

    [ContextMenu("Print Terrain Info")]
    public void PrintTerrainInfo()
    {
        Debug.Log($"Terrain Position: {transform.position}");
        Debug.Log($"Terrain Size: {terrainData.size}");
        Debug.Log($"Start World Position: {GetStartWorldPosition()}");
        Debug.Log($"Hole World Position: {GetHoleWorldPosition()}");
        Debug.Log($"TerrainCollider Present: {GetComponent<TerrainCollider>() != null}");
    }
}