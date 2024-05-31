// Existing MoneySystem.cs content...

using UnityEngine;
using UnityEngine.Audio;

public class MoneySystem : MonoBehaviour
{
    public AudioClip pickupSound;
    public int maxCoinStorage = 3;
    public int currentCoinCount = 0;
    public GameObject coinPrefab;

    private void Start()
    {
    }
    public bool AddCoin()
    {
        if (currentCoinCount < maxCoinStorage)
        {
            AudioSource.PlayClipAtPoint(pickupSound, gameObject.transform.position);
            currentCoinCount++;
            return true;
        }
        return false;
    }

    public bool RemoveCoin()
    {
        if (currentCoinCount > 0)
        {
            currentCoinCount--;
            return true;
        }
        return false;
    }

    public void DropCoins()
    {
        if (currentCoinCount > 0)
        {
            for (int i = 0; i < GetCurrentCoinCount(); i++)
            {
                RemoveCoin();
            }
        }
        // Logic to drop coins
        // This may involve instantiating coin prefabs and updating the unit's coin count
    }

    public int GetCurrentCoinCount()
    {
        return currentCoinCount;
    }

}
