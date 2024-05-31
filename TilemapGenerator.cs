using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System.Collections;
using System.Linq;

public class TilemapGenerator : MonoBehaviour
{
    [Header("Tilemap Settings")]
    public Tilemap groundTilemap;
    public Tilemap pathsTilemap;
    public Tilemap objectsTilemap;
    public Tilemap structuresTilemap;
    public Tilemap baseTilemap;  // Default tile for non-water base
    public Tilemap showGridTilemap;
    public TileBase[] terrainTiles;
    public TileBase[] vegetationTiles;
    public TileBase[] structureTiles;
    public Tile gridTile;

    [Header("Tile Placement Settings")]
    public float vegetationDensity = 0.1f;
    public float structureDensity = 0.05f;
    public float structureMinDistance = 5f;
    public float structureMaxDistance = 10f;
    public float structureMinElevation = 0.3f;
    public float structureMaxElevation = 0.7f;
    public float villageDensity = 0.01f; // Initial density for initiating village cluster
    public int minimumVillageSize = 5; // Minimum structures in a village
    public float waterLevel = 0.2f;

    private HashSet<Vector2Int> structurePositions = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> waterPositions = new HashSet<Vector2Int>();
    private Quadtree terrainQuadtree;

    public int chunkSize;
    private ChunkData[,] chunkGridCopy;
    private HashSet<Vector2Int> loadedChunks = new HashSet<Vector2Int>();
    private bool canRun = false;
    private bool force_chunk = false;

    private Queue<KeyValuePair<int, int>> chunkQueue = new Queue<KeyValuePair<int, int>>();
    private Coroutine generateTilemapCoroutine;

    [Header("Camera Settings")]
    public Camera mainCamera;
    public int cameraChunkRadius = 2;
    public int MaxChunksPerFrame = 5; // Adjust this value based on your performance requirements
    private Vector2Int currentCameraChunk;

    [Header("Progress Settings")]
    public float generationProgress;
    public int totalChunks;
    /// <summary>
    /// Clears the tilemap, removing all existing tiles.
    /// </summary>
    public void ClearTilemap()
    {
        groundTilemap.ClearAllTiles();
        pathsTilemap.ClearAllTiles();
        objectsTilemap.ClearAllTiles();
        structuresTilemap.ClearAllTiles();
        baseTilemap.ClearAllTiles();
        showGridTilemap.ClearAllTiles();
    }

    /// <summary>
    /// Generates the tilemap based on the provided chunk grid.
    /// </summary>
    /// <param name="chunkGrid">The 2D array representing the chunk grid.</param>
    public void GenerateTilemap(ChunkData[,] chunkGrid)
    {
        chunkGridCopy = chunkGrid;
        if (this == null)
        {
            Debug.LogError("TilemapGenerator object is null. Cannot generate tilemap.");
            return;
        }

        chunkQueue.Clear();
        totalChunks = 0;

        for (int chunkY = 0; chunkY < chunkGrid.GetLength(1); chunkY++)
        {
            for (int chunkX = 0; chunkX < chunkGrid.GetLength(0); chunkX++)
            {
                if (chunkGrid[chunkX, chunkY].isVisible)
                {
                    chunkQueue.Enqueue(new KeyValuePair<int, int>(chunkX, chunkY));
                    totalChunks++;
                }
            }
        }

        generationProgress = 0f;

        if (generateTilemapCoroutine == null)
        {
            generateTilemapCoroutine = StartCoroutine(LoadChunkCoroutine());
        }
    }


    private IEnumerator LoadChunkCoroutine()
    {
        int chunksGenerated = 0;
        int chunksProcessedThisFrame = 0;

        while (chunkQueue.Count > 0)
        {
            if (chunksProcessedThisFrame >= MaxChunksPerFrame)
            {
                chunksProcessedThisFrame = 0;
                yield return null; // Yield control to allow other things to happen
            }
            else
            {
                try
                {
                    KeyValuePair<int, int> chunkCoords = chunkQueue.Dequeue();
                    int chunkX = chunkCoords.Key;
                    int chunkY = chunkCoords.Value;

                    ChunkData chunkData = chunkGridCopy[chunkX, chunkY];
                    if (chunkData.isVisible)
                    {
                        int offsetX = chunkX * chunkSize;
                        int offsetY = chunkY * chunkSize;
                        PlaceTerrainTiles(chunkData.chunkData, offsetX, offsetY);
                        PlaceVegetationTiles(chunkData.chunkData, offsetX, offsetY);
                        PlaceStructureTiles(chunkData.chunkData, offsetX, offsetY);
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Error generating tilemap for chunk: {ex.Message}\n{ex.StackTrace}");
                    StopAllCoroutines();
                    yield break;
                }

                chunksProcessedThisFrame++;
                chunksGenerated++;
                generationProgress = (float)chunksGenerated / totalChunks;
            }
        }

        generateTilemapCoroutine = null;
    }

    /// <summary>
    /// Places terrain tiles on the tilemap based on the terrain grid.
    /// </summary>
    /// <param name="terrainGrid">The 2D array representing the terrain grid.</param>
    private object tileLock = new object(); // Lock object for thread synchronization

    private void PlaceTerrainTiles(int[,] chunkData, int offsetX, int offsetY)
    {


        Stopwatch stopwatch = new Stopwatch();

        int totalTiles = chunkSize * chunkSize;
        TileBase[] groundTileArray = new TileBase[totalTiles];
        // Create the quadtree for the chunk
        Rect chunkBounds = new Rect(offsetX, offsetY, chunkSize, chunkSize);
        terrainQuadtree = new Quadtree(chunkBounds);

        int index = 0;
        for (int y = 0; y < chunkSize; y++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                int terrainIndex = chunkData[x, y];
                if (terrainIndex >= 0 && terrainIndex < terrainTiles.Length)
                {
                    TileBase tile = terrainTiles[terrainIndex];
                    groundTileArray[index] = tile;

                    // Add water tile positions to the waterPositions HashSet
                    if (terrainIndex == 0) // Assuming index 0 is water
                    {
                        waterPositions.Add(new Vector2Int(x + offsetX, y + offsetY));
                    }

                    // Ensure there is a land tile at every position except water
                    if (terrainIndex != 0)
                    {
                        terrainQuadtree.Insert(new Vector2Int(x + offsetX, y + offsetY));
                    }
                }
                else
                {
                    // Handle the case when terrainIndex is out of range
                    // You can assign a default tile or log an error message
                    //Debug.LogError($"Invalid terrain index at ({x + offsetX}, {y + offsetY}): {terrainIndex}");
                }

                index++;
            }
        }

        stopwatch.Start();

        BoundsInt tilemapChunkBounds = new BoundsInt(offsetX, offsetY, 0, chunkSize, chunkSize, 1);
        //Debug.LogError($"Tilemap Chunk Bounds: {tilemapChunkBounds}");

        // Use a coroutine for setting tiles asynchronously
        SetTiles(groundTilemap, tilemapChunkBounds, groundTileArray, stopwatch);

        // Set base tiles for the chunk
        SetBaseTiles(chunkData, offsetX, offsetY);

        // Set grid tiles for the chunk
        SetGridTiles(chunkSize, offsetX, offsetY);
    }
    private void SetBaseTiles(int[,] chunkData, int offsetX, int offsetY)
    {
        int totalTiles = chunkSize * chunkSize;
        TileBase[] baseTileArray = new TileBase[totalTiles];

        int index = 0;
        for (int y = 0; y < chunkSize; y++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                int terrainIndex = chunkData[x, y];
                if (terrainIndex != 0) // Assuming index 0 is water
                {
                    baseTileArray[index] = terrainTiles[3]; // 3 = forest
                }
                index++;
            }
        }

        BoundsInt baseBounds = new BoundsInt(offsetX, offsetY, 0, chunkSize, chunkSize, 1);
        SetTiles(baseTilemap, baseBounds, baseTileArray, null);
    }

    private void SetGridTiles(int chunkSize, int offsetX, int offsetY)
    {
        int totalTiles = chunkSize * chunkSize;
        TileBase[] gridTileArray = new TileBase[totalTiles];

        for (int i = 0; i < totalTiles; i++)
        {
            gridTileArray[i] = gridTile;
        }

        BoundsInt gridBounds = new BoundsInt(offsetX, offsetY, 0, chunkSize, chunkSize, 1);
        SetTiles(showGridTilemap, gridBounds, gridTileArray, null);
    }
    private void SetTiles(Tilemap tilemap, BoundsInt bounds, TileBase[] tileArray, Stopwatch stopwatch)
    {
        lock (tileLock)
        {
            //Debug.LogError($"Setting tiles - Bounds: {bounds}, Tile Array Length: {tileArray.Length}");
            tilemap.SetTilesBlock(bounds, tileArray);
        }

        if (stopwatch != null)
        {
            stopwatch.Stop();
            //Debug.Log($"Tilemap setting time: {stopwatch.ElapsedMilliseconds} ms");
        }
    }


    /// <summary>
    /// Places vegetation tiles on the tilemap based on the terrain grid and vegetation density.
    /// </summary>
    /// <param name="terrainGrid">The 2D array representing the terrain grid.</param>
    private void PlaceVegetationTiles(int[,] chunkData, int offsetX, int offsetY)
    {
        // Clear existing vegetation tiles in the chunk bounds
        BoundsInt chunkBounds = new BoundsInt(offsetX, offsetY, 0, chunkSize, chunkSize, 1);

        List<TileBase> tileList = new List<TileBase>();
        List<Vector3Int> positionList = new List<Vector3Int>();

        for (int y = 0; y < chunkSize; y++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                if (Random.value < vegetationDensity)
                {
                    int terrainIndex = chunkData[x, y];
                    if (terrainIndex != 0)
                    {
                        TileBase tile = vegetationTiles[Random.Range(0, vegetationTiles.Length)];
                        tileList.Add(tile);
                        positionList.Add(new Vector3Int(x + offsetX, y + offsetY, 0));
                    }
                }
            }
        }

        if (tileList.Count > 0)
        {
            objectsTilemap.SetTiles(positionList.ToArray(), tileList.ToArray());
        }
    }

    /// <summary>
    /// Places structure tiles on the tilemap based on the terrain grid, structure density, and placement rules.
    /// </summary>
    /// <param name="terrainGrid">The 2D array representing the terrain grid.</param>
    private void PlaceStructureTiles(int[,] chunkData, int offsetX, int offsetY)
    {
        HashSet<Vector2Int> chunkStructurePositions = new HashSet<Vector2Int>();

        for (int y = 0; y < chunkSize; y++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                int globalX = offsetX + x;
                int globalY = offsetY + y;

                if (Random.value < villageDensity && IsValidForVillage(globalX, globalY, chunkData, offsetX, offsetY, chunkStructurePositions))
                {
                    CreateVillage(globalX, globalY, chunkData, offsetX, offsetY, chunkStructurePositions);
                }
            }
        }
    }

    private void CreateVillage(int centerX, int centerY, int[,] chunkData, int offsetX, int offsetY, HashSet<Vector2Int> chunkStructurePositions)
    {
        int structuresPlaced = 0;
        Queue<Vector2Int> positionsToCheck = new Queue<Vector2Int>();
        positionsToCheck.Enqueue(new Vector2Int(centerX, centerY));

        List<Vector3Int> tilePositions = new List<Vector3Int>();
        List<TileBase> tiles = new List<TileBase>();

        while (positionsToCheck.Count > 0 && structuresPlaced < minimumVillageSize)
        {
            Vector2Int pos = positionsToCheck.Dequeue();
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    int nx = pos.x + dx;
                    int ny = pos.y + dy;
                    Vector2Int localPosition = new Vector2Int(nx, ny);
                    if (nx >= offsetX && nx < offsetX + chunkSize && ny >= offsetY && ny < offsetY + chunkSize && Random.value < structureDensity)
                    {
                        if (!chunkStructurePositions.Contains(localPosition) && IsValidForVillage(nx, ny, chunkData, offsetX, offsetY, chunkStructurePositions))
                        {
                            TileBase tile = structureTiles[Random.Range(0, structureTiles.Length)];
                            tilePositions.Add(new Vector3Int(localPosition.x, localPosition.y, 0));
                            tiles.Add(tile);
                            chunkStructurePositions.Add(localPosition);
                            positionsToCheck.Enqueue(new Vector2Int(nx, ny));
                            structuresPlaced++;
                        }
                    }
                }
            }
        }

        if (tilePositions.Count > 0)
        {
            SetTiles(structuresTilemap, tilePositions.ToArray(), tiles.ToArray());
        }
    }

    private void SetTiles(Tilemap tilemap, Vector3Int[] positions, TileBase[] tiles)
    {
        tilemap.SetTiles(positions, tiles);
    }

    private bool IsValidForVillage(int x, int y, int[,] chunkData, int offsetX, int offsetY, HashSet<Vector2Int> chunkStructurePositions)
    {
        int localX = x - offsetX;
        int localY = y - offsetY;
        if (localX >= 0 && localX < chunkSize && localY >= 0 && localY < chunkSize)
        {
            int terrainIndex = chunkData[localX, localY];
            if (terrainIndex != 0) // Assuming index 0 is water
            {
                float elevation = terrainIndex / (float)terrainTiles.Length;
                return elevation >= structureMinElevation && elevation <= structureMaxElevation && !IsNearWater(x, y, chunkData, offsetX, offsetY) && !IsNearStructure(x, y, offsetX, offsetY, chunkStructurePositions);
            }
        }
        return false;
    }
    /// <summary>
    /// Checks if a given position is near an existing structure tile.
    /// </summary>
    /// <param name="x">The x-coordinate of the position.</param>
    /// <param name="y">The y-coordinate of the position.</param>
    /// <returns>True if the position is near a structure, false otherwise.</returns>
    private bool IsNearStructure(int x, int y, int offsetX, int offsetY, HashSet<Vector2Int> chunkStructurePositions)
    {
        int radius = Mathf.RoundToInt(structureMinDistance);
        for (int dy = -radius; dy <= radius; dy++)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                int sampleX = x + dx;
                int sampleY = y + dy;
                Vector2Int localPosition = new Vector2Int(sampleX - offsetX, sampleY - offsetY);

                if (sampleX >= offsetX && sampleX < offsetX + chunkSize && sampleY >= offsetY && sampleY < offsetY + chunkSize)
                {
                    if (chunkStructurePositions.Contains(localPosition))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }


    /// <summary>
    /// Checks if a given position is near water based on the terrain grid.
    /// </summary>
    /// <param name="x">The x-coordinate of the position.</param>
    /// <param name="y">The y-coordinate of the position.</param>
    /// <param name="terrainGrid">The 2D array representing the terrain grid.</param>
    /// <returns>True if the position is near water, false otherwise.</returns>
    private bool IsNearWater(int x, int y, int[,] chunkData, int offsetX, int offsetY)
    {
        int radius = 1;
        for (int dy = -radius; dy <= radius; dy++)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                int sampleX = x + dx;
                int sampleY = y + dy;

                if (sampleX >= offsetX && sampleX < offsetX + chunkSize && sampleY >= offsetY && sampleY < offsetY + chunkSize)
                {
                    if (waterPositions.Contains(new Vector2Int(sampleX, sampleY)))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    /// Camera functions.
    /// 
    public void SetCameraToCenter(ChunkData[,] chunkGrid)
    {
        chunkGridCopy = chunkGrid;
        int gridWidth = chunkGrid.GetLength(0) * chunkSize;
        int gridHeight = chunkGrid.GetLength(1) * chunkSize;
        Vector3 centerPosition = new Vector3(gridWidth / 2f + chunkSize, gridHeight / 2f + chunkSize, mainCamera.transform.position.z);
        mainCamera.transform.position = centerPosition;
        canRun = true;
    }
    public void SetCameraToOffset()
    {
        mainCamera.transform.position = new Vector3(-999, -999, mainCamera.transform.position.z);
    }
    private void Update()
    {
        if (canRun) {
            UpdateVisibleChunks(chunkGridCopy);
            UpdateCameraChunkRadius();
        }
    }
    private void UpdateCameraChunkRadius()
    {
        float cameraSize = mainCamera.orthographicSize;
        int newChunkRadius = Mathf.RoundToInt(cameraSize / (0.5f * chunkSize));

        if (newChunkRadius != cameraChunkRadius)
        {
            cameraChunkRadius = newChunkRadius;
            force_chunk = true;
        }
    }
    public void UpdateVisibleChunks(ChunkData[,] chunkGrid)
    {
        if (chunkGrid == null)
        {
            Debug.LogError("ChunkGrid is null. Cannot update visible chunks.");
            return;
        }

        Vector3 cameraPosition = mainCamera.transform.position;
        int cameraChunkX = Mathf.FloorToInt(cameraPosition.x / chunkSize);
        int cameraChunkY = Mathf.FloorToInt(cameraPosition.y / chunkSize);

        // Check if the camera chunk has changed
        if (cameraChunkX != currentCameraChunk.x || cameraChunkY != currentCameraChunk.y || force_chunk)
        {
            force_chunk = false;
            // Calculate the new visible chunks based on the camera position
            (int minX, int maxX, int minY, int maxY) = GetVisibleChunkBounds(cameraChunkX, cameraChunkY, cameraChunkRadius, chunkGrid);

            // Create a copy of the loaded chunks to avoid modifying the collection while iterating
            HashSet<Vector2Int> loadedChunksCopy = new HashSet<Vector2Int>(loadedChunks);

            // Unload chunks that are no longer visible
            foreach (var loadedChunk in loadedChunksCopy)
            {
                if (loadedChunk.x < minX || loadedChunk.x > maxX || loadedChunk.y < minY || loadedChunk.y > maxY)
                {
                    if (IsValidChunkCoordinate(loadedChunk.x, loadedChunk.y, chunkGrid))
                    {
                        chunkGrid[loadedChunk.x, loadedChunk.y].isVisible = false;
                        UnloadChunk(loadedChunk.x, loadedChunk.y);
                    }
                    loadedChunks.Remove(loadedChunk);
                }
            }

            // Load chunks that are now visible
            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    Vector2Int chunk = new Vector2Int(x, y);
                    if (!loadedChunks.Contains(chunk) && IsValidChunkCoordinate(x, y, chunkGrid))
                    {
                        chunkGrid[x, y].isVisible = true;
                        loadedChunks.Add(chunk);
                        chunkQueue.Enqueue(new KeyValuePair<int, int>(x, y));
                    }
                }
            }

            currentCameraChunk = new Vector2Int(cameraChunkX, cameraChunkY);

            if (generateTilemapCoroutine == null)
            {
                generateTilemapCoroutine = StartCoroutine(LoadChunkCoroutine());
            }
        }
    }

    private bool IsValidChunkCoordinate(int x, int y, ChunkData[,] chunkGrid)
    {
        return x >= 0 && x < chunkGrid.GetLength(0) && y >= 0 && y < chunkGrid.GetLength(1);
    }

    private (int minX, int maxX, int minY, int maxY) GetVisibleChunkBounds(int cameraChunkX, int cameraChunkY, int radius, ChunkData[,] chunkGrid)
    {
        int minX = Mathf.Max(cameraChunkX - radius, 0);
        int maxX = Mathf.Min(cameraChunkX + radius, chunkGrid.GetLength(0) - 1);
        int minY = Mathf.Max(cameraChunkY - radius, 0);
        int maxY = Mathf.Min(cameraChunkY + radius, chunkGrid.GetLength(1) - 1);

        return (minX, maxX, minY, maxY);
    }

    private void UnloadChunk(int chunkX, int chunkY)
    {
        int offsetX = chunkX * chunkSize;
        int offsetY = chunkY * chunkSize;
        BoundsInt chunkBounds = new BoundsInt(offsetX, offsetY, 0, chunkSize, chunkSize, 1);
        int totalChunkTiles = chunkSize * chunkSize;
        TileBase[] nullTiles = new TileBase[totalChunkTiles];

        // Clear the tiles in the chunk for each tilemap
        groundTilemap.SetTilesBlock(chunkBounds, nullTiles);
        pathsTilemap.SetTilesBlock(chunkBounds, nullTiles);
        objectsTilemap.SetTilesBlock(chunkBounds, nullTiles);
        structuresTilemap.SetTilesBlock(chunkBounds, nullTiles);
        baseTilemap.SetTilesBlock(chunkBounds, nullTiles);
        showGridTilemap.SetTilesBlock(chunkBounds, nullTiles);
    }

}

[System.Serializable]
public struct BiomeTileset
{
    public string name;
    public TileBase[] groundTiles;
    public TileBase[] objectTiles;
}
