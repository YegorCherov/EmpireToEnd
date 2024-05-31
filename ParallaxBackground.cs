using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ParallaxLayer
{
    public GameObject prefab;
    public float parallaxSpeedX;
    public float parallaxSpeedY;

    public float BackgroundWidth
    {
        get
        {
            float totalWidth = 0f;
            foreach (SpriteRenderer spriteRenderer in prefab.GetComponentsInChildren<SpriteRenderer>())
            {
                totalWidth += spriteRenderer.bounds.size.x;
            }
            return totalWidth;
        }
    }
}


public class ParallaxBackground : MonoBehaviour
{
    public ParallaxLayer[] layers;
    public Transform player;
    private Dictionary<ParallaxLayer, List<GameObject>> layerObjects = new Dictionary<ParallaxLayer, List<GameObject>>();
    private Vector3 previousPlayerPosition;

    void Start()
    {
        previousPlayerPosition = player.position;
        foreach (var layer in layers)
        {
            List<GameObject> objects = new List<GameObject>();
            float halfWidth = layer.BackgroundWidth / 2;
            float startOffset = player.position.x - halfWidth;

            // Spawn initial set of backgrounds
            for (float totalWidth = -halfWidth; totalWidth < Camera.main.orthographicSize * 2; totalWidth += layer.BackgroundWidth)
            {
                Vector3 spawnPosition = new Vector3(startOffset + totalWidth, player.position.y, 0);
                GameObject bgObject = Instantiate(layer.prefab, spawnPosition, Quaternion.identity, transform);
                objects.Add(bgObject);
            }
            layerObjects[layer] = objects;
        }
    }

    void Update()
    {
        Vector3 playerDelta = player.position - previousPlayerPosition;

        foreach (var layer in layers)
        {
            List<GameObject> objects = layerObjects[layer];

            for (int i = 0; i < objects.Count; i++)
            {
                GameObject bgObject = objects[i];
                Vector3 newPosition = bgObject.transform.position + new Vector3(playerDelta.x * layer.parallaxSpeedX, playerDelta.y * layer.parallaxSpeedY, 0);
                bgObject.transform.position = newPosition;
            }

            // Handle spawning new background pieces and despawning old ones
            HandleBackgroundSpawningAndDespawning(layer);
        }

        previousPlayerPosition = player.position;
    }

    void HandleBackgroundSpawningAndDespawning(ParallaxLayer layer)
    {
        List<GameObject> objects = layerObjects[layer];
        Camera cam = Camera.main;
        float camHorizontalExtend = cam.orthographicSize * Screen.width / Screen.height;

        // Spawn new background piece on the right
        GameObject lastBg = objects[objects.Count - 1];
        if (lastBg.transform.position.x < player.position.x + camHorizontalExtend)
        {
            Vector3 spawnPosition = new Vector3(lastBg.transform.position.x + layer.BackgroundWidth - 1, player.position.y, 0);
            GameObject newBg = Instantiate(layer.prefab, spawnPosition, Quaternion.identity, transform);
            objects.Add(newBg);
        }

        // Spawn new background piece on the left
        GameObject firstBg = objects[0];
        if (firstBg.transform.position.x > player.position.x - camHorizontalExtend)
        {
            Vector3 spawnPosition = new Vector3(firstBg.transform.position.x - layer.BackgroundWidth + 1, player.position.y, 0);
            GameObject newBg = Instantiate(layer.prefab, spawnPosition, Quaternion.identity, transform);
            objects.Insert(0, newBg);
        }

        // Despawn background pieces out of view
        if (objects.Count > 2)
        {
            if (objects[0].transform.position.x < player.position.x - camHorizontalExtend - layer.BackgroundWidth + 1)
            {
                GameObject oldBg = objects[0];
                objects.RemoveAt(0);
                Destroy(oldBg);
            }
            if (objects[objects.Count - 1].transform.position.x > player.position.x + camHorizontalExtend + layer.BackgroundWidth - 1)
            {
                GameObject oldBg = objects[objects.Count - 1];
                objects.RemoveAt(objects.Count - 1);
                Destroy(oldBg);
            }
        }
    }
}
