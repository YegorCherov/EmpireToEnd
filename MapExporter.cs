using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;

public class MapExporter : MonoBehaviour
{
    [Header("Export Settings")]
    public string exportFolderPath = "Maps";
    public string exportFileName = "Map";

    /// <summary>
    /// Exports the generated map data to a text file.
    /// </summary>
    /// <param name="heightMap">The 2D array representing the height map.</param>
    /// <param name="terrainGrid">The 2D array representing the terrain grid.</param>
    /// <param name="tilemap">The tilemap containing the generated map.</param>
    public void ExportMap(float[,] heightMap, int[,] terrainGrid, Tilemap tilemap)
    {
        string exportPath = Path.Combine(exportFolderPath, exportFileName + ".txt");
        // Create the export folder if it doesn't exist
        Directory.CreateDirectory(exportFolderPath);

        using (StreamWriter writer = new StreamWriter(exportPath))
        {
            // Write height map data
            int width = heightMap.GetLength(0);
            int height = heightMap.GetLength(1);
            writer.WriteLine($"Height Map ({width}x{height}):");
            for (int y = height - 1; y >= 0; y--)
            {
                for (int x = 0; x < width; x++)
                {
                    writer.Write(heightMap[x, y].ToString("0.00") + " ");
                }
                writer.WriteLine();
            }

            // Write terrain grid data
            writer.WriteLine($"Terrain Grid ({width}x{height}):");
            for (int y = height - 1; y >= 0; y--)
            {
                for (int x = 0; x < width; x++)
                {
                    writer.Write(terrainGrid[x, y] + " ");
                }
                writer.WriteLine();
            }

            // Write tilemap data
            writer.WriteLine("Tilemap Data:");
            BoundsInt bounds = tilemap.cellBounds;
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                for (int x = bounds.xMin; x < bounds.xMax; x++)
                {
                    TileBase tile = tilemap.GetTile(new Vector3Int(x, y, 0));
                    writer.Write(tile != null ? "1" : "0");
                }
                writer.WriteLine();
            }
        }

        Debug.Log($"Map exported to: {exportPath}");
    }
}