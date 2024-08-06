```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    // Define the chunk size for better performance
    public const int CHUNK_SIZE = 16;

    private Chunk[,] chunks; // Store chunks in a 2D array for efficient access

    void Start()
    {
        // Initialize an empty array to store chunks
        chunks = new Chunk[Mathf.CeilToInt(Camera.main.orthographicSize / (float)CHUNK_SIZE), Mathf.CeilToInt(Camera.main.orthographicSize / (float)CHUNK_SIZE)];

        // Iterate over the camera's orthographic size and create chunks accordingly
        for (int x = 0; x < chunks.GetLength(0); x++)
        {
            for (int z = 0; z < chunks.GetLength(1); z++)
            {
                // Instantiate a new chunk at the current position
                Chunk chunk = Instantiate(Resources.Load<Chunk>("Prefabs/Chunk"));
                chunk.transform.position = new Vector3(x * CHUNK_SIZE, 0, z * CHUNK_SIZE);
                chunks[x, z] = chunk;
            }
        }
    }

    void Update()
    {
        // Update each chunk in the array
        for (int x = 0; x < chunks.GetLength(0); x++)
        {
            for (int z = 0; z < chunks.GetLength(1); z++)
            {
                // Get the current chunk and update its position
                Chunk chunk = chunks[x, z];
                chunk.transform.position += Camera.main.transform.forward * Time.deltaTime;
            }
        }
    }

    public void RemoveChunk(int x, int z)
    {
        // Destroy the chunk at the specified position
        if (chunks[x, z] != null)
        {
            Destroy(chunks[x, z].gameObject);
            chunks[x, z] = null;
        }
    }
}
```