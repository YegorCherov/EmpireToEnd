using UnityEngine;

public enum BiomeType { Snow, Desert, Forest }

public class BiomeGenerator : MonoBehaviour
{
    public float[,] GenerateBiomeMap(float[,] heightMap)
    {
        float[,] biomeMap = new float[heightMap.GetLength(0), heightMap.GetLength(1)];
        for (int x = 0; x < heightMap.GetLength(0); x++)
        {
            for (int y = 0; y < heightMap.GetLength(1); y++)
            {
                float height = heightMap[x, y];
                // Implement biome logic based on height and moisture here
                // Example conditional to assign a biome type:
                if (height < 0.3f) biomeMap[x, y] = (float)BiomeType.Desert;
                // Add other conditions for different biomes
            }
        }
        return biomeMap;
    }
}