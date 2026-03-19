using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Terrain))]
public class ProceduralTerrainGolf : MonoBehaviour
{
    [Header("Terrain Settings")]
    public int heightmapResolution = 257;
    public float terrainSize = 20f;
    public float maxHeight = 1.5f;
    
    [Header("Noise Layers")]
    public NoiseLayer[] noiseLayers = new NoiseLayer[]
    {
        new NoiseLayer { scale = 3f, amplitude = 1f, octaves = 4 },
        new NoiseLayer { scale = 10f, amplitude = 0.3f, octaves = 2 },
        new NoiseLayer { scale = 20f, amplitude = 0.1f, octaves = 1 }
    };
    
    [Header("Terrain Processing")]
    public bool smoothTerrain = true;
    public int smoothingPasses = 2;
    
    [Header("Golf Hole Prefabs")]
    public GameObject startPrefab;
    public GameObject holePrefab;
    
    [Header("Spawn Positions (Local)")]
    public Vector3 startPosition = new Vector3(2f, 0f, 2f);
    public Vector3 holePosition = new Vector3(18f, 0f, 18f);
    public float flattenRadius = 2f;
    public AnimationCurve flattenCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    [Header("Obstacle Settings")]
    public GameObject[] obstaclePrefabs;
    public int obstacleCount = 10;
    public float obstacleHeightOffset = 0.1f;
    public float minDistanceFromTargets = 2f;
    public float minDistanceBetweenObstacles = 1.5f;

    [Header("Terrain Chaining")]
    public bool isChained = false;
    public ProceduralTerrainGolf nextTerrain;
    public ProceduralTerrainGolf previousTerrain;
    public int chainIndex = 0;
    
    [Header("Materials")]
    public Material terrainMaterial;
    public PhysicMaterial terrainPhysicsMaterial;

    [Header("Boundary Walls")]
    public bool generateWalls = true;
    public float wallHeight = 2f;
    public float wallThickness = 0.5f;
    public Material wallMaterial;

    [Header("Debug")]
    public bool showDebugGizmos = true;
    public Color gizmoColor = Color.green;

    private Terrain terrain;
    private TerrainData terrainData;
    private float[] rightEdgeHeights;
    private int seed;

    [System.Serializable]
    public class NoiseLayer
    {
        public float scale = 3f;
        public float amplitude = 1f;
        public int octaves = 1;
        [Range(0f, 1f)] public float persistence = 0.5f;
        public Vector2 offset;
    }

    void Awake()
    {
        terrain = GetComponent<Terrain>();
        
        terrainData = new TerrainData();
        terrainData.heightmapResolution = heightmapResolution;
        terrainData.size = new Vector3(terrainSize, maxHeight, terrainSize);
        
        if (terrainMaterial != null)
            terrain.materialTemplate = terrainMaterial;

        terrain.terrainData = terrainData;
        
        TerrainCollider terrainCollider = GetComponent<TerrainCollider>();
        if (terrainCollider == null)
            terrainCollider = gameObject.AddComponent<TerrainCollider>();

        terrainCollider.terrainData = terrainData;
        
        if (terrainPhysicsMaterial != null)
            terrainCollider.material = terrainPhysicsMaterial;

        GenerateTerrain();
    }

    void GenerateTerrain()
    {
        seed = GetInstanceID() + chainIndex * 1000;
        Random.InitState(seed);

        float[,] heights = GenerateMultiLayerNoise();
        heights = FlattenArea(heights, startPosition, flattenRadius);
        heights = FlattenArea(heights, holePosition, flattenRadius);

        if (smoothTerrain)
            for (int i = 0; i < smoothingPasses; i++)
                heights = SmoothHeightmap(heights);

        rightEdgeHeights = new float[heightmapResolution];
        for (int z = 0; z < heightmapResolution; z++)
            rightEdgeHeights[z] = heights[heightmapResolution - 1, z];

        if (previousTerrain != null && previousTerrain.rightEdgeHeights != null)
            heights = BlendLeftEdge(heights, previousTerrain.rightEdgeHeights);

        terrainData.SetHeights(0, 0, heights);
    }

    void GenerateWalls()
    {
        if (!generateWalls) return;

        float size = terrainSize;

        // Forward wall — only if NO next terrain
        if (nextTerrain == null)
        {
            CreateWall(
                new Vector3(size / 2, wallHeight / 2, size),
                new Vector3(size, wallHeight, wallThickness)
            );
        }

        // Back wall — only if NO previous terrain
        if (previousTerrain == null)
        {
            CreateWall(
                new Vector3(size / 2, wallHeight / 2, 0),
                new Vector3(size, wallHeight, wallThickness)
            );
        }

        // Left wall — always (no neighbor tracking)
        CreateWall(
            new Vector3(0, wallHeight / 2, size / 2),
            new Vector3(wallThickness, wallHeight, size)
        );

        // Right wall — always
        CreateWall(
            new Vector3(size, wallHeight / 2, size / 2),
            new Vector3(wallThickness, wallHeight, size)
        );
    }

    void CreateWall(Vector3 localPosition, Vector3 scale)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);

        wall.name = "BoundaryWall";
        wall.transform.SetParent(transform);
        wall.transform.localPosition = localPosition;
        wall.transform.localScale = scale;

        if (wallMaterial != null)
            wall.GetComponent<MeshRenderer>().material = wallMaterial;

        wall.layer = gameObject.layer;
    }

    public void BuildWalls()
    {
        if (!generateWalls) return;

        float size = terrainSize;

        // Forward wall — only if NO next terrain
        if (nextTerrain == null)
        {
            CreateWall(
                new Vector3(size / 2, wallHeight / 2, size),
                new Vector3(size, wallHeight, wallThickness)
            );
        }

        // Back wall — only if NO previous terrain
        if (previousTerrain == null)
        {
            CreateWall(
                new Vector3(size / 2, wallHeight / 2, 0),
                new Vector3(size, wallHeight, wallThickness)
            );
        }

        // Left wall — always
        CreateWall(
            new Vector3(0, wallHeight / 2, size / 2),
            new Vector3(wallThickness, wallHeight, size)
        );

        // Right wall — always
        CreateWall(
            new Vector3(size, wallHeight / 2, size / 2),
            new Vector3(wallThickness, wallHeight, size)
        );
    }

    float[,] GenerateMultiLayerNoise()
    {
        float[,] heights = new float[heightmapResolution, heightmapResolution];

        foreach (NoiseLayer layer in noiseLayers)
        {
            float offsetX = Random.Range(0f, 1000f) + layer.offset.x;
            float offsetZ = Random.Range(0f, 1000f) + layer.offset.y;

            for (int x = 0; x < heightmapResolution; x++)
            for (int z = 0; z < heightmapResolution; z++)
            {
                float noiseValue = 0f;
                float amplitude = layer.amplitude;
                float frequency = 1f;

                for (int octave = 0; octave < layer.octaves; octave++)
                {
                    float nx = (x / (float)heightmapResolution * layer.scale * frequency) + offsetX;
                    float nz = (z / (float)heightmapResolution * layer.scale * frequency) + offsetZ;

                    noiseValue += Mathf.PerlinNoise(nx, nz) * amplitude;

                    amplitude *= layer.persistence;
                    frequency *= 2f;
                }

                heights[x, z] += noiseValue;
            }
        }

        return NormalizeHeights(heights);
    }

    float[,] FlattenArea(float[,] heights, Vector3 localPos, float radius)
    {
        int centerX = Mathf.RoundToInt((localPos.x / terrainSize) * (heightmapResolution - 1));
        int centerZ = Mathf.RoundToInt((localPos.z / terrainSize) * (heightmapResolution - 1));
        int radiusInSamples = Mathf.RoundToInt((radius / terrainSize) * heightmapResolution);

        float targetHeight = 0f;
        int sampleCount = 0;

        for (int x = -radiusInSamples; x <= radiusInSamples; x++)
        for (int z = -radiusInSamples; z <= radiusInSamples; z++)
        {
            int sx = Mathf.Clamp(centerX + x, 0, heightmapResolution - 1);
            int sz = Mathf.Clamp(centerZ + z, 0, heightmapResolution - 1);

            if (Vector2.Distance(Vector2.zero, new Vector2(x, z)) <= radiusInSamples)
            {
                targetHeight += heights[sx, sz];
                sampleCount++;
            }
        }

        targetHeight /= sampleCount;

        for (int x = -radiusInSamples; x <= radiusInSamples; x++)
        for (int z = -radiusInSamples; z <= radiusInSamples; z++)
        {
            int hx = Mathf.Clamp(centerX + x, 0, heightmapResolution - 1);
            int hz = Mathf.Clamp(centerZ + z, 0, heightmapResolution - 1);

            float distance = Vector2.Distance(Vector2.zero, new Vector2(x, z));

            if (distance <= radiusInSamples)
            {
                float t = distance / radiusInSamples;
                float blend = flattenCurve.Evaluate(t);
                heights[hx, hz] = Mathf.Lerp(targetHeight, heights[hx, hz], blend);
            }
        }

        return heights;
    }

    float[,] SmoothHeightmap(float[,] heights)
    {
        float[,] smoothed = new float[heightmapResolution, heightmapResolution];

        for (int x = 0; x < heightmapResolution; x++)
        for (int z = 0; z < heightmapResolution; z++)
        {
            float sum = heights[x, z];
            int count = 1;

            for (int nx = -1; nx <= 1; nx++)
            for (int nz = -1; nz <= 1; nz++)
            {
                if (nx == 0 && nz == 0) continue;

                int sx = Mathf.Clamp(x + nx, 0, heightmapResolution - 1);
                int sz = Mathf.Clamp(z + nz, 0, heightmapResolution - 1);

                sum += heights[sx, sz];
                count++;
            }

            smoothed[x, z] = sum / count;
        }

        return smoothed;
    }

    float[,] NormalizeHeights(float[,] heights)
    {
        float min = float.MaxValue;
        float max = float.MinValue;

        for (int x = 0; x < heightmapResolution; x++)
        for (int z = 0; z < heightmapResolution; z++)
        {
            if (heights[x, z] < min) min = heights[x, z];
            if (heights[x, z] > max) max = heights[x, z];
        }

        float range = max - min;

        if (range > 0.001f)
            for (int x = 0; x < heightmapResolution; x++)
            for (int z = 0; z < heightmapResolution; z++)
                heights[x, z] = (heights[x, z] - min) / range;

        return heights;
    }

    float[,] BlendLeftEdge(float[,] heights, float[] previousRightEdge)
    {
        int blendWidth = Mathf.RoundToInt(heightmapResolution * 0.1f);

        for (int z = 0; z < heightmapResolution; z++)
        {
            float targetHeight = previousRightEdge[z];

            for (int x = 0; x < blendWidth; x++)
            {
                float blend = Mathf.SmoothStep(0f, 1f, x / (float)(blendWidth - 1));
                heights[x, z] = Mathf.Lerp(targetHeight, heights[x, z], blend);
            }
        }

        return heights;
    }
    

    public void ChainToTerrain(ProceduralTerrainGolf next)
    {
        nextTerrain = next;
        next.previousTerrain = this;
        next.isChained = true;
        next.chainIndex = chainIndex + 1;
    }
}