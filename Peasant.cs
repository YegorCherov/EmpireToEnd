using Unity.VisualScripting;
using UnityEngine;

public class Peasant : Unit
{
    public AudioClip upgradeSound;

    private void Start()
    {
        Awake();
        Update();
    }
}
