using System.Diagnostics;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using Debug = UnityEngine.Debug;
using static Noise;
using System.Text;
using System.IO;
using static TerrainGenerator;
using System;
public class MapGenerator : MonoBehaviour
{
    [Header("Map Settings")]
    [Tooltip("The width of the terrain map.\n" +
             "Higher values result in a wider terrain, while lower values create a narrower terrain.")]
    [Range(1, 10000)]
    public int mapWidth = 100;
    [Tooltip("The height of the terrain map.\n" +
             "Higher values result in a taller terrain, while lower values create a shorter terrain.")]
    [Range(1, 10000)]
    public int mapHeight = 100;
    public int seed = 0;
    public bool autoUpdate = false;
    public bool viewFullMap = false;

    [Header("Chunk Settings")]
    public int chunkSize = 16;

    [Header("Noise Settings")]
    public NoiseSettings[] noiseSettings;

    [Header("References")]
    public TerrainGenerator terrainGenerator;
    public TilemapGenerator tilemapGenerator;
    public MapExporter mapExporter;

    private ChunkData[,] chunkGrid;

    private void Start()
    {
        if (autoUpdate)
        {
            GenerateMap();
        }
    }
    private void OnValidate()
    {
        mapWidth = Mathf.Max(1, mapWidth);
        mapHeight = Mathf.Max(1, mapHeight);

        if (autoUpdate)
        {
            GenerateMap();
        }
    }
    public void SaveTerrainGridAndNoiseMapsToFile(int[,] terrainGrid, Dictionary<string, float[,]> noiseMaps, string filePath)
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine("Terrain Grid:");
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                sb.Append(terrainGrid[x, y]);
                sb.Append(",");
            }
            sb.AppendLine();
        }
        sb.AppendLine();


        // Save noise maps
        foreach (var noiseMap in noiseMaps)
        {
            SaveMapToStringBuilder(sb, noiseMap.Key, noiseMap.Value);
            sb.AppendLine();
        }

        File.WriteAllText(filePath, sb.ToString());

        // Open the file with the default text editor
        Process.Start(filePath);
    }

    private void SaveMapToStringBuilder(StringBuilder sb, string mapName, float[,] map)
    {
        sb.AppendLine(mapName + ":");
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                sb.Append(map[x, y]);
                sb.Append(",");
            }
            sb.AppendLine();
        }
    }

    /// <summary>
    /// Generates a new map by orchestrating the terrain generation, tilemap generation, map enhancement, and map export.
    /// </summary>
    public async void GenerateMap()
    {
        Stopwatch overallStopwatch = Stopwatch.StartNew();
        Stopwatch overallNoiseMap = Stopwatch.StartNew();
        terrainGenerator.seed = seed;
        terrainGenerator.mapWidth = mapWidth;
        terrainGenerator.mapHeight = mapHeight;
        tilemapGenerator.chunkSize = chunkSize;

        var noiseMaps = new Dictionary<string, float[,]>();
        var noiseMapTasks = new List<Task<KeyValuePair<string, float[,]>>>();
        int seed_offset = seed;
        foreach (var noiseSetting in noiseSettings)
        {
            int currentSeed = seed_offset;
            if (noiseSetting.name == "Blend Map")
            {
                currentSeed = seed; // Use the same seed as the Height Map
            }
            noiseMapTasks.Add(Task.Run(() => GenerateNoiseMapAsync(noiseSetting.name, noiseSetting, currentSeed)));
            seed_offset++;
        }

        await Task.WhenAll(noiseMapTasks);
        overallNoiseMap.Stop();
        Debug.Log($"Overall noise map generation time: {overallNoiseMap.ElapsedMilliseconds} ms");
        foreach (var noiseMapTask in noiseMapTasks)
        {
            var result = noiseMapTask.Result;
            noiseMaps[result.Key] = result.Value;
        }
        foreach (var noiseMap in noiseMaps)
        {
            DisplayNoiseMap(noiseMap.Value, noiseMap.Key);
        }
        var terrainGridStopwatch = Stopwatch.StartNew();

        var generateTerrainGridStopwatch = Stopwatch.StartNew();
        int[,] terrainGrid = GenerateTerrainGrid(noiseMaps["Height Map"], noiseMaps["Moisture Map"], noiseMaps["Temperature Map"],
            noiseMaps["Ocean Map"], noiseMaps["River Map"]);
        generateTerrainGridStopwatch.Stop();
        Debug.Log($"GenerateTerrainGrid time: {generateTerrainGridStopwatch.ElapsedMilliseconds} ms");

        //SaveTerrainGridAndNoiseMapsToFile(terrainGrid, noiseMaps, "terrain_grid_and_maps.txt");

        var sliceTerrainGridStopwatch = Stopwatch.StartNew();
        SliceTerrainGridIntoChunks(terrainGrid);
        sliceTerrainGridStopwatch.Stop();
        Debug.Log($"SliceTerrainGridIntoChunks time: {sliceTerrainGridStopwatch.ElapsedMilliseconds} ms");

        terrainGridStopwatch.Stop();
        Debug.Log($"Terrain Grid generation time: {terrainGridStopwatch.ElapsedMilliseconds} ms");


        overallStopwatch.Stop();
        Debug.Log($"Overall map generation time: {overallStopwatch.ElapsedMilliseconds} ms");

        tilemapGenerator.ClearTilemap();
        if (viewFullMap)
            tilemapGenerator.GenerateTilemap(chunkGrid);
        tilemapGenerator.SetCameraToOffset();
        tilemapGenerator.UpdateVisibleChunks(chunkGrid);
        tilemapGenerator.SetCameraToCenter(chunkGrid);
        tilemapGenerator.UpdateVisibleChunks(chunkGrid);

        /*
        var exportStopwatch = Stopwatch.StartNew();
        mapExporter.ExportMap(noiseMaps["Base Terrain Height Map"], terrainGrid, tilemapGenerator.groundTilemap);
        exportStopwatch.Stop();
        Debug.Log($"Map export time: {exportStopwatch.ElapsedMilliseconds} ms");
        */
    }
    public static void DisplayNoiseMap(float[,] noiseMap, string mapName)
    {
        int mapWidth = noiseMap.GetLength(0);
        int mapHeight = noiseMap.GetLength(1);

        Texture2D texture = new Texture2D(mapWidth, mapHeight);
        Color[] colors = new Color[mapWidth * mapHeight];

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float value = noiseMap[x, y];
                colors[y * mapWidth + x] = new Color(value, value, value);
            }
        }

        texture.SetPixels(colors);
        texture.Apply();

        // Save the texture to a file for external viewing
        byte[] bytes = texture.EncodeToPNG();
        System.IO.File.WriteAllBytes(Application.dataPath + "/" + mapName + ".png", bytes);
    }
    private KeyValuePair<string, float[,]> GenerateNoiseMapAsync(string mapName, NoiseSettings settings, int map_seed)
    {
        float[,] noiseMap;
        if (mapName == "Blend Map")
        {
            noiseMap = terrainGenerator.GenerateNoiseMap(mapWidth * chunkSize, mapHeight * chunkSize, settings, map_seed);
        }
        else
            noiseMap = terrainGenerator.GenerateNoiseMap(mapWidth, mapHeight, settings, map_seed);

        return new KeyValuePair<string, float[,]>(mapName, noiseMap);
    }

    public enum BiomeType
    {
        Water,
        Desert,
        Grassland,
        Forest,
        Tundra,
        Mountain
    }

    private int[,] GenerateTerrainGrid(float[,] heightMap, float[,] moistureMap, float[,] temperatureMap, float[,] oceanMap, float[,] riverMap)
    {
        int mapWidth = heightMap.GetLength(0);
        int mapHeight = heightMap.GetLength(1);
        int[,] terrainGrid = new int[mapWidth, mapHeight];
        
        // First pass: Assign biomes based on thresholds
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float height = heightMap[x, y];
                float moisture = moistureMap[x, y];
                float temperature = temperatureMap[x, y];
                float ocean = oceanMap[x, y];
                float river = riverMap[x, y];

                if (ocean <= 0.3f || river > 1f)
                {
                    terrainGrid[x, y] = (int)BiomeType.Water;
                }
                else
                {
                    if (height < 0.3f)
                    {
                        if (moisture < 0.4f && temperature > 0.5f)
                            terrainGrid[x, y] = (int)BiomeType.Desert;
                        else if (temperature < 0.2f)
                            terrainGrid[x, y] = (int)BiomeType.Tundra;
                        else
                            terrainGrid[x, y] = (int)BiomeType.Grassland;
                    }
                    else if (height < 0.6f)
                    {
                        if (moisture < 0.4f)
                        {
                            if (temperature < 0.2f)
                                terrainGrid[x, y] = (int)BiomeType.Tundra;
                            else if (temperature > 0.5f && moisture < 0.4f)
                                terrainGrid[x, y] = (int)BiomeType.Desert;
                            else
                                terrainGrid[x, y] = (int)BiomeType.Grassland;
                        }
                        else
                        {
                            terrainGrid[x, y] = (int)BiomeType.Forest;
                        }
                    }
                    else
                    {
                        if (temperature < 0.2f)
                            terrainGrid[x, y] = (int)BiomeType.Tundra;
                        else if (moisture < 0.5f)
                            terrainGrid[x, y] = (int)BiomeType.Mountain;
                        else
                            terrainGrid[x, y] = (int)BiomeType.Tundra;
                    }
                }
            }
        }
        return terrainGrid;
    }

    private int GetBiomeSize(int[,] terrainGrid, int x, int y, int biomeIndex)
    {
        int size = 0;
        bool[,] visited = new bool[terrainGrid.GetLength(0), terrainGrid.GetLength(1)];

        Queue<(int, int)> queue = new Queue<(int, int)>();
        queue.Enqueue((x, y));
        visited[x, y] = true;

        while (queue.Count > 0)
        {
            (int currentX, int currentY) = queue.Dequeue();
            size++;

            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    int neighborX = currentX + i;
                    int neighborY = currentY + j;

                    if (neighborX >= 0 && neighborX < terrainGrid.GetLength(0) &&
                        neighborY >= 0 && neighborY < terrainGrid.GetLength(1) &&
                        !visited[neighborX, neighborY] &&
                        terrainGrid[neighborX, neighborY] == biomeIndex)
                    {
                        queue.Enqueue((neighborX, neighborY));
                        visited[neighborX, neighborY] = true;
                    }
                }
            }
        }

        return size;
    }

    private void ExpandBiome(int[,] terrainGrid, int x, int y, int biomeIndex, int targetSize)
    {
        int size = 0;
        Queue<(int, int)> queue = new Queue<(int, int)>();
        queue.Enqueue((x, y));

        while (queue.Count > 0 && size < targetSize)
        {
            (int currentX, int currentY) = queue.Dequeue();
            size++;

            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    int neighborX = currentX + i;
                    int neighborY = currentY + j;

                    if (neighborX >= 0 && neighborX < terrainGrid.GetLength(0) &&
                        neighborY >= 0 && neighborY < terrainGrid.GetLength(1) &&
                        terrainGrid[neighborX, neighborY] != biomeIndex)
                    {
                        terrainGrid[neighborX, neighborY] = biomeIndex;
                        queue.Enqueue((neighborX, neighborY));
                    }
                }
            }
        }
    }
    private void SliceTerrainGridIntoChunks(int[,] terrainGrid)
    {
        int numChunksX = terrainGrid.GetLength(0);
        int numChunksY = terrainGrid.GetLength(1);
        chunkGrid = new ChunkData[numChunksX, numChunksY];

        Parallel.For(0, numChunksY, chunkY =>
        {
            for (int chunkX = 0; chunkX < numChunksX; chunkX++)
            {
                int biomeType = terrainGrid[chunkX, chunkY];
                int[,] chunkData = new int[chunkSize, chunkSize];

                for (int y = 0; y < chunkSize; y++)
                {
                    for (int x = 0; x < chunkSize; x++)
                    {
                        chunkData[x, y] = biomeType;
                    }
                }

                // Blend the chunk data based on neighboring chunks
                //BlendChunkData(chunkData, terrainGrid, chunkX, chunkY);

                chunkGrid[chunkX, chunkY] = new ChunkData(chunkData, viewFullMap);
            }
        });
    }

    private void BlendChunkData(int[,] chunkData, int[,] terrainGrid, int chunkX, int chunkY)
    {
        int blendRange = chunkSize / 2; // Adjust the blending range as needed
        System.Random random = new System.Random();

        // Create a copy of the original chunkData for reference
        int[,] originalChunkData = (int[,])chunkData.Clone();

        for (int y = 0; y < chunkSize; y++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                int biomeType = originalChunkData[x, y];

                // Check neighboring chunks for different biomes
                for (int dy = -1; dy <= 1; dy++)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        if (dx == 0 && dy == 0) continue;

                        int neighborChunkX = chunkX + dx;
                        int neighborChunkY = chunkY + dy;

                        if (neighborChunkX >= 0 && neighborChunkX < terrainGrid.GetLength(0) &&
                            neighborChunkY >= 0 && neighborChunkY < terrainGrid.GetLength(1))
                        {
                            int neighborBiomeType = terrainGrid[neighborChunkX, neighborChunkY];

                            if (neighborBiomeType != biomeType && neighborBiomeType != (int)BiomeType.Desert && neighborBiomeType != (int)BiomeType.Tundra && biomeType != (int)BiomeType.Water)
                            {
                                // Calculate the Manhattan distance from the current tile to the neighboring chunk
                                int distanceX = Mathf.Abs(x - (dx < 0 ? 0 : chunkSize - 1));
                                int distanceY = Mathf.Abs(y - (dy < 0 ? 0 : chunkSize - 1));
                                int distance = distanceX + distanceY;

                                // Apply blending based on distance and a random factor
                                if (distance <= blendRange)
                                {
                                    float blendFactor = 1f - (distance / (float)blendRange);
                                    float randomFactor = (float)random.NextDouble();
                                    blendFactor = Mathf.Lerp(blendFactor, randomFactor, 0.5f);

                                    if (blendFactor >= 0.5f)
                                    {
                                        chunkData[x, y] = neighborBiomeType;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}

public class ChunkData
{
    public int[,] chunkData;
    public bool isVisible;

    public ChunkData(int[,] data, bool visible)
    {
        chunkData = data;
        isVisible = visible;
    }
}