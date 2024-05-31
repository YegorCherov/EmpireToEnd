using UnityEngine;
using System.Collections.Generic;

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance { get; private set; }

    private Dictionary<string, Queue<GameObject>> objectPools = new Dictionary<string, Queue<GameObject>>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public GameObject GetPooledObject(string tag)
    {
        if (!objectPools.ContainsKey(tag))
        {
            objectPools[tag] = new Queue<GameObject>();
        }

        if (objectPools[tag].Count > 0)
        {
            return objectPools[tag].Dequeue();
        }
        else
        {
            GameObject newObject = new GameObject(tag);
            newObject.SetActive(false);
            return newObject;
        }
    }

    public void ReturnObjectToPool(GameObject obj)
    {
        string tag = obj.name;
        if (!objectPools.ContainsKey(tag))
        {
            objectPools[tag] = new Queue<GameObject>();
        }

        obj.SetActive(false);
        objectPools[tag].Enqueue(obj);
    }
}