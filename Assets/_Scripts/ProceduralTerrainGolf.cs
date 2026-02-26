using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Terrain))]
public class ProceduralTerrainGolf : MonoBehaviour
{
    [Header("Terrain Settings")]
    public int heightmapResolution = 257; // Higher resolution for smoother terrain
    public float terrainSize = 20f;
    public float maxHeight = 1.5f;
    
    [Header("Noise Layers")]
    public NoiseLayer[] noiseLayers = new NoiseLayer[]
    {
        new NoiseLayer { scale = 3f, amplitude = 1f, octaves = 4 },      // Base terrain
        new NoiseLayer { scale = 10f, amplitude = 0.3f, octaves = 2 },   // Large features
        new NoiseLayer { scale = 20f, amplitude = 0.1f, octaves = 1 }    // Fine detail
    };
    
    [Header("Terrain Features")]
    public bool generateHills = true;
    [Range(0f, 1f)] public float hillDensity = 0.3f;
    public bool generateValleys = true;
    [Range(0f, 1f)] public float valleyDensity = 0.2f;
    public bool smoothTerrain = true;
    public int smoothingPasses = 2;
    
    [Header("Golf Course Settings")]
    public TerrainType terrainType = TerrainType.Fairway;
    public bool createGradualSlope = true; // Slope from start to hole
    [Range(-15f, 15f)] public float slopeAngle = 5f; // Degrees
    
    [Header("Golf Hole Prefabs")]
    public GameObject startPrefab;
    public GameObject holePrefab;
    
    [Header("Spawn Positions (Local)")]
    public Vector3 startPosition = new Vector3(2f, 0f, 2f);
    public Vector3 holePosition = new Vector3(18f, 0f, 18f);
    public float flattenRadius = 2f; // Flatten area around start/hole
    public AnimationCurve flattenCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    [Header("Hazards")]
    public bool generateBunkers = true;
    public int bunkerCount = 3;
    public float bunkerDepth = 0.3f;
    public float bunkerSize = 2f;
    public GameObject bunkerPrefab;
    
    public bool generateWaterHazards = false;
    public int waterHazardCount = 1;
    public float waterDepth = 0.5f;
    public GameObject waterPrefab;

    [Header("Obstacle Settings")]
    public GameObject[] obstaclePrefabs; // Array for variety
    public int obstacleCount = 10;
    public float obstacleHeightOffset = 0.1f;
    public float minDistanceFromTargets = 2f;
    public float minDistanceBetweenObstacles = 1.5f;

    [Header("Terrain Chaining")]
    public bool isChained = false;
    public ProceduralTerrainGolf nextTerrain;
    public ProceduralTerrainGolf previousTerrain;
    public int chainIndex = 0; // Which terrain in the chain (0, 1, 2, etc.)
    
    [Header("Materials")]
    public Material terrainMaterial;
    public PhysicMaterial terrainPhysicsMaterial;

    [Header("Debug")]
    public bool showDebugGizmos = true;
    public Color gizmoColor = Color.green;

    private Terrain terrain;
    private TerrainData terrainData;
    private GameObject spawnedStart;
    private GameObject spawnedHole;
    private List<GameObject> spawnedObjects = new List<GameObject>();
    private float[] rightEdgeHeights;
    private int seed;

    public enum TerrainType
    {
        Fairway,    // Gentle rolling hills
        Rough,      // More extreme terrain
        Links,      // Flat with occasional mounds
        Mountain    // Steep elevation changes
    }

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
        
        // Create new terrain data
        terrainData = new TerrainData();
        terrainData.heightmapResolution = heightmapResolution;
        terrainData.size = new Vector3(terrainSize, maxHeight, terrainSize);
        
        if (terrainMaterial != null)
        {
            terrain.materialTemplate = terrainMaterial;
        }

        terrain.terrainData = terrainData;
        
        // Add TerrainCollider component if not present
        TerrainCollider terrainCollider = GetComponent<TerrainCollider>();
        if (terrainCollider == null)
        {
            terrainCollider = gameObject.AddComponent<TerrainCollider>();
        }
        terrainCollider.terrainData = terrainData;
        
        if (terrainPhysicsMaterial != null)
        {
            terrainCollider.material = terrainPhysicsMaterial;
        }

        GenerateTerrain();
    }

    void GenerateTerrain()
    {
        // Generate unique seed based on position and chain index
        seed = GetInstanceID() + chainIndex * 1000;
        Random.InitState(seed);

        float[,] heights = new float[heightmapResolution, heightmapResolution];

        // Generate base noise with multiple layers
        heights = GenerateMultiLayerNoise();

        // Apply terrain type specific modifications
        heights = ApplyTerrainType(heights);

        // Create gradual slope from start to hole if enabled
        if (createGradualSlope)
        {
            heights = ApplyDirectionalSlope(heights);
        }

        // Generate terrain features
        if (generateHills)
        {
            heights = AddHills(heights);
        }

        if (generateValleys)
        {
            heights = AddValleys(heights);
        }

        // Add bunkers
        if (generateBunkers)
        {
            heights = AddBunkers(heights);
        }

        // Add water hazards
        if (generateWaterHazards)
        {
            heights = AddWaterHazards(heights);
        }

        // Flatten areas around start and hole
        heights = FlattenArea(heights, startPosition, flattenRadius);
        heights = FlattenArea(heights, holePosition, flattenRadius);

        // Smooth terrain for more natural look
        if (smoothTerrain)
        {
            for (int i = 0; i < smoothingPasses; i++)
            {
                heights = SmoothHeightmap(heights);
            }
        }

        // Store right edge heights for next terrain
        rightEdgeHeights = new float[heightmapResolution];
        for (int z = 0; z < heightmapResolution; z++)
        {
            rightEdgeHeights[z] = heights[heightmapResolution - 1, z];
        }

        // Blend left edge with previous terrain's right edge if chained
        if (previousTerrain != null && previousTerrain.rightEdgeHeights != null)
        {
            heights = BlendLeftEdge(heights, previousTerrain.rightEdgeHeights);
        }

        terrainData.SetHeights(0, 0, heights);

        // Spawn gameplay objects
        SpawnTargets();
        PlaceObstacles();
    }

    float[,] GenerateMultiLayerNoise()
    {
        float[,] heights = new float[heightmapResolution, heightmapResolution];

        foreach (NoiseLayer layer in noiseLayers)
        {
            float offsetX = Random.Range(0f, 1000f) + layer.offset.x;
            float offsetZ = Random.Range(0f, 1000f) + layer.offset.y;

            for (int x = 0; x < heightmapResolution; x++)
            {
                for (int z = 0; z < heightmapResolution; z++)
                {
                    float noiseValue = 0f;
                    float amplitude = layer.amplitude;
                    float frequency = 1f;

                    // Generate octaves
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
        }

        // Normalize heights
        return NormalizeHeights(heights);
    }

    float[,] ApplyTerrainType(float[,] heights)
    {
        switch (terrainType)
        {
            case TerrainType.Fairway:
                // Gentle, playable terrain
                for (int x = 0; x < heightmapResolution; x++)
                {
                    for (int z = 0; z < heightmapResolution; z++)
                    {
                        heights[x, z] = Mathf.Pow(heights[x, z], 1.5f) * 0.7f;
                    }
                }
                break;

            case TerrainType.Rough:
                // More extreme variations
                for (int x = 0; x < heightmapResolution; x++)
                {
                    for (int z = 0; z < heightmapResolution; z++)
                    {
                        heights[x, z] = Mathf.Pow(heights[x, z], 0.8f);
                    }
                }
                break;

            case TerrainType.Links:
                // Mostly flat with occasional mounds
                for (int x = 0; x < heightmapResolution; x++)
                {
                    for (int z = 0; z < heightmapResolution; z++)
                    {
                        heights[x, z] = heights[x, z] * 0.3f;
                    }
                }
                break;

            case TerrainType.Mountain:
                // Steep elevation changes
                for (int x = 0; x < heightmapResolution; x++)
                {
                    for (int z = 0; z < heightmapResolution; z++)
                    {
                        heights[x, z] = Mathf.Pow(heights[x, z], 0.6f);
                    }
                }
                break;
        }

        return heights;
    }

    float[,] ApplyDirectionalSlope(float[,] heights)
    {
        // Calculate slope direction from start to hole
        Vector3 direction = (holePosition - startPosition).normalized;
        float slopeHeight = Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * terrainSize;

        for (int x = 0; x < heightmapResolution; x++)
        {
            for (int z = 0; z < heightmapResolution; z++)
            {
                // Get normalized position (0-1)
                float normalizedX = x / (float)(heightmapResolution - 1);
                float normalizedZ = z / (float)(heightmapResolution - 1);

                // Calculate position along slope direction
                Vector3 currentPos = new Vector3(normalizedX * terrainSize, 0, normalizedZ * terrainSize);
                Vector3 fromStart = currentPos - startPosition;
                float distanceAlongSlope = Vector3.Dot(fromStart, direction);
                float maxDistance = Vector3.Distance(startPosition, holePosition);

                // Add slope height
                float slopeContribution = (distanceAlongSlope / maxDistance) * slopeHeight / maxHeight;
                heights[x, z] += slopeContribution;
                heights[x, z] = Mathf.Clamp01(heights[x, z]);
            }
        }

        return heights;
    }

    float[,] AddHills(float[,] heights)
    {
        int hillCount = Mathf.RoundToInt(hillDensity * 5);
        
        for (int i = 0; i < hillCount; i++)
        {
            int centerX = Random.Range(heightmapResolution / 4, 3 * heightmapResolution / 4);
            int centerZ = Random.Range(heightmapResolution / 4, 3 * heightmapResolution / 4);
            float hillHeight = Random.Range(0.1f, 0.3f);
            int hillRadius = Random.Range(heightmapResolution / 8, heightmapResolution / 4);

            for (int x = 0; x < heightmapResolution; x++)
            {
                for (int z = 0; z < heightmapResolution; z++)
                {
                    float distance = Vector2.Distance(
                        new Vector2(x, z),
                        new Vector2(centerX, centerZ)
                    );

                    if (distance < hillRadius)
                    {
                        float falloff = 1f - (distance / hillRadius);
                        falloff = Mathf.SmoothStep(0f, 1f, falloff);
                        heights[x, z] += hillHeight * falloff;
                    }
                }
            }
        }

        return heights;
    }

    float[,] AddValleys(float[,] heights)
    {
        int valleyCount = Mathf.RoundToInt(valleyDensity * 3);

        for (int i = 0; i < valleyCount; i++)
        {
            int centerX = Random.Range(heightmapResolution / 4, 3 * heightmapResolution / 4);
            int centerZ = Random.Range(heightmapResolution / 4, 3 * heightmapResolution / 4);
            float valleyDepth = Random.Range(0.1f, 0.2f);
            int valleyRadius = Random.Range(heightmapResolution / 10, heightmapResolution / 5);

            for (int x = 0; x < heightmapResolution; x++)
            {
                for (int z = 0; z < heightmapResolution; z++)
                {
                    float distance = Vector2.Distance(
                        new Vector2(x, z),
                        new Vector2(centerX, centerZ)
                    );

                    if (distance < valleyRadius)
                    {
                        float falloff = 1f - (distance / valleyRadius);
                        falloff = Mathf.SmoothStep(0f, 1f, falloff);
                        heights[x, z] -= valleyDepth * falloff;
                        heights[x, z] = Mathf.Max(0f, heights[x, z]);
                    }
                }
            }
        }

        return heights;
    }

    float[,] AddBunkers(float[,] heights)
    {
        for (int i = 0; i < bunkerCount; i++)
        {
            Vector3 bunkerPos = GetRandomPositionAwayFromTargets();
            int bunkerX = Mathf.RoundToInt((bunkerPos.x / terrainSize) * (heightmapResolution - 1));
            int bunkerZ = Mathf.RoundToInt((bunkerPos.z / terrainSize) * (heightmapResolution - 1));
            int radius = Mathf.RoundToInt((bunkerSize / terrainSize) * heightmapResolution);

            for (int x = 0; x < heightmapResolution; x++)
            {
                for (int z = 0; z < heightmapResolution; z++)
                {
                    float distance = Vector2.Distance(
                        new Vector2(x, z),
                        new Vector2(bunkerX, bunkerZ)
                    );

                    if (distance < radius)
                    {
                        float falloff = 1f - (distance / radius);
                        falloff = Mathf.SmoothStep(0f, 1f, falloff);
                        heights[x, z] -= (bunkerDepth / maxHeight) * falloff;
                        heights[x, z] = Mathf.Max(0f, heights[x, z]);
                    }
                }
            }

            // Spawn bunker prefab if available
            if (bunkerPrefab != null)
            {
                Vector3 worldPos = transform.position + bunkerPos;
                float y = terrain.SampleHeight(worldPos) + transform.position.y;
                GameObject bunker = Instantiate(bunkerPrefab, new Vector3(worldPos.x, y, worldPos.z), Quaternion.identity, transform);
                spawnedObjects.Add(bunker);
            }
        }

        return heights;
    }

    float[,] AddWaterHazards(float[,] heights)
    {
        for (int i = 0; i < waterHazardCount; i++)
        {
            Vector3 waterPos = GetRandomPositionAwayFromTargets();
            int waterX = Mathf.RoundToInt((waterPos.x / terrainSize) * (heightmapResolution - 1));
            int waterZ = Mathf.RoundToInt((waterPos.z / terrainSize) * (heightmapResolution - 1));
            int radius = Mathf.RoundToInt((3f / terrainSize) * heightmapResolution);

            for (int x = 0; x < heightmapResolution; x++)
            {
                for (int z = 0; z < heightmapResolution; z++)
                {
                    float distance = Vector2.Distance(
                        new Vector2(x, z),
                        new Vector2(waterX, waterZ)
                    );

                    if (distance < radius)
                    {
                        heights[x, z] = Mathf.Max(0f, heights[x, z] - (waterDepth / maxHeight));
                    }
                }
            }

            // Spawn water prefab if available
            if (waterPrefab != null)
            {
                Vector3 worldPos = transform.position + waterPos;
                GameObject water = Instantiate(waterPrefab, new Vector3(worldPos.x, transform.position.y, worldPos.z), Quaternion.identity, transform);
                spawnedObjects.Add(water);
            }
        }

        return heights;
    }

    float[,] FlattenArea(float[,] heights, Vector3 localPos, float radius)
    {
        int centerX = Mathf.RoundToInt((localPos.x / terrainSize) * (heightmapResolution - 1));
        int centerZ = Mathf.RoundToInt((localPos.z / terrainSize) * (heightmapResolution - 1));
        int radiusInSamples = Mathf.RoundToInt((radius / terrainSize) * heightmapResolution);

        // Get target height (average of the area)
        float targetHeight = 0f;
        int sampleCount = 0;

        for (int x = -radiusInSamples; x <= radiusInSamples; x++)
        {
            for (int z = -radiusInSamples; z <= radiusInSamples; z++)
            {
                int sampleX = Mathf.Clamp(centerX + x, 0, heightmapResolution - 1);
                int sampleZ = Mathf.Clamp(centerZ + z, 0, heightmapResolution - 1);
                
                float distance = Vector2.Distance(Vector2.zero, new Vector2(x, z));
                if (distance <= radiusInSamples)
                {
                    targetHeight += heights[sampleX, sampleZ];
                    sampleCount++;
                }
            }
        }

        targetHeight /= sampleCount;

        // Apply flattening with smooth falloff
        for (int x = -radiusInSamples; x <= radiusInSamples; x++)
        {
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
        }

        return heights;
    }

    float[,] SmoothHeightmap(float[,] heights)
    {
        float[,] smoothed = new float[heightmapResolution, heightmapResolution];

        for (int x = 0; x < heightmapResolution; x++)
        {
            for (int z = 0; z < heightmapResolution; z++)
            {
                float sum = heights[x, z];
                int count = 1;

                // Average with neighbors
                for (int nx = -1; nx <= 1; nx++)
                {
                    for (int nz = -1; nz <= 1; nz++)
                    {
                        if (nx == 0 && nz == 0) continue;

                        int sampleX = Mathf.Clamp(x + nx, 0, heightmapResolution - 1);
                        int sampleZ = Mathf.Clamp(z + nz, 0, heightmapResolution - 1);

                        sum += heights[sampleX, sampleZ];
                        count++;
                    }
                }

                smoothed[x, z] = sum / count;
            }
        }

        return smoothed;
    }

    float[,] NormalizeHeights(float[,] heights)
    {
        float min = float.MaxValue;
        float max = float.MinValue;

        // Find min and max
        for (int x = 0; x < heightmapResolution; x++)
        {
            for (int z = 0; z < heightmapResolution; z++)
            {
                if (heights[x, z] < min) min = heights[x, z];
                if (heights[x, z] > max) max = heights[x, z];
            }
        }

        // Normalize to 0-1
        float range = max - min;
        if (range > 0.001f)
        {
            for (int x = 0; x < heightmapResolution; x++)
            {
                for (int z = 0; z < heightmapResolution; z++)
                {
                    heights[x, z] = (heights[x, z] - min) / range;
                }
            }
        }

        return heights;
    }

    float[,] BlendLeftEdge(float[,] heights, float[] previousRightEdge)
    {
        int blendWidth = Mathf.RoundToInt(heightmapResolution * 0.1f); // 10% of terrain width

        for (int z = 0; z < heightmapResolution; z++)
        {
            float targetHeight = previousRightEdge[z];

            for (int x = 0; x < blendWidth; x++)
            {
                float blend = x / (float)(blendWidth - 1);
                blend = Mathf.SmoothStep(0f, 1f, blend);
                heights[x, z] = Mathf.Lerp(targetHeight, heights[x, z], blend);
            }
        }

        return heights;
    }

    void SpawnTargets()
    {
        // Clear previous spawns
        if (spawnedStart != null) Destroy(spawnedStart);
        if (spawnedHole != null) Destroy(spawnedHole);

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
        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null) Destroy(obj);
        }
        spawnedObjects.Clear();

        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0) return;

        Vector3 worldStart = transform.position + startPosition;
        Vector3 worldHole = transform.position + holePosition;
        List<Vector3> placedPositions = new List<Vector3>();

        int attempts = 0;
        int maxAttempts = obstacleCount * 20;

        while (spawnedObjects.Count < obstacleCount && attempts < maxAttempts)
        {
            attempts++;

            Vector3 localPos = new Vector3(
                Random.Range(2f, terrainSize - 2f),
                0,
                Random.Range(2f, terrainSize - 2f)
            );
            Vector3 worldPos = transform.position + localPos;

            // Check distance from start and hole
            if (Vector3.Distance(worldPos, worldStart) < minDistanceFromTargets ||
                Vector3.Distance(worldPos, worldHole) < minDistanceFromTargets)
            {
                continue;
            }

            // Check distance from other obstacles
            bool tooClose = false;
            foreach (Vector3 placed in placedPositions)
            {
                if (Vector3.Distance(worldPos, placed) < minDistanceBetweenObstacles)
                {
                    tooClose = true;
                    break;
                }
            }

            if (tooClose) continue;

            float y = terrain.SampleHeight(worldPos) + transform.position.y;
            Vector3 spawnPos = new Vector3(worldPos.x, y + obstacleHeightOffset, worldPos.z);

            // Random obstacle from array
            GameObject prefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
            GameObject obstacle = Instantiate(prefab, spawnPos, Quaternion.Euler(0, Random.Range(0f, 360f), 0), transform);
            spawnedObjects.Add(obstacle);
            placedPositions.Add(worldPos);
        }
    }

    Vector3 GetRandomPositionAwayFromTargets()
    {
        Vector3 position;
        int attempts = 0;

        do
        {
            position = new Vector3(
                Random.Range(3f, terrainSize - 3f),
                0,
                Random.Range(3f, terrainSize - 3f)
            );
            attempts++;
        }
        while ((Vector3.Distance(position, startPosition) < minDistanceFromTargets * 2 ||
                Vector3.Distance(position, holePosition) < minDistanceFromTargets * 2) &&
                attempts < 100);

        return position;
    }

    public Vector3 GetStartWorldPosition()
    {
        Vector3 worldStart = transform.position + startPosition;
        float y = terrain.SampleHeight(worldStart) + transform.position.y;
        return new Vector3(worldStart.x, y, worldStart.z);
    }

    public Vector3 GetHoleWorldPosition()
    {
        Vector3 worldHole = transform.position + holePosition;
        float y = terrain.SampleHeight(worldHole) + transform.position.y;
        return new Vector3(worldHole.x, y, worldHole.z);
    }

    public void ChainToTerrain(ProceduralTerrainGolf next)
    {
        nextTerrain = next;
        next.previousTerrain = this;
        next.isChained = true;
        next.chainIndex = chainIndex + 1;
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
        Debug.Log($"Seed: {seed}");
    }

    void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;

        Gizmos.color = gizmoColor;

        // Draw start position
        Vector3 startWorld = transform.position + startPosition;
        Gizmos.DrawWireSphere(startWorld, flattenRadius);
        Gizmos.DrawLine(startWorld, startWorld + Vector3.up * 2f);

        // Draw hole position
        Vector3 holeWorld = transform.position + holePosition;
        Gizmos.DrawWireSphere(holeWorld, flattenRadius);
        Gizmos.DrawLine(holeWorld, holeWorld + Vector3.up * 2f);

        // Draw line from start to hole
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(startWorld, holeWorld);

        // Draw terrain bounds
        Gizmos.color = Color.cyan;
        Vector3 center = transform.position + new Vector3(terrainSize / 2f, 0, terrainSize / 2f);
        Gizmos.DrawWireCube(center, new Vector3(terrainSize, 0.1f, terrainSize));
    }
}