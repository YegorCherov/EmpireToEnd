using UnityEngine;

public class Beggar : Unit
{
    public GameObject peasantPrefab; // Assign this in the Inspector
    public AudioClip upgradeSound;
    MoneySystem moneySystem;
    private void Start()
    {
        moneySystem = GetComponent<MoneySystem>();
        Awake();
        Update();
    }
#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
    private void Update()
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword
    {
        base.Update(); // Call the base class Update method

        //Check for upgrade conditions
        if (moneySystem.GetCurrentCoinCount() >= 1) // For example, if 1 coin is collected
        {
            moneySystem.RemoveCoin();
            UpgradeUnit(peasantPrefab, upgradeSound); // Upgrade to Peasant
        }
    }

}
