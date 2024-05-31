using System.Collections;
using UnityEngine;

public class Coin : MonoBehaviour
{
    public float pickupSpeed = 9.0f;
    public int Value = 1; // Default value for a coin
    public bool IsRare = false; // Indicates if the coin is rare

    private Transform target;
    private MoneySystem moneySystem;

    void OnTriggerEnter2D(Collider2D collider)
    {
        Unit unit = collider.gameObject.GetComponent<Unit>();
        if (unit != null && !unit.isAttacking)
        {
            target = collider.transform;
            moneySystem = target.GetComponent<MoneySystem>();
            StartCoroutine(PickupCoin());
        }
    }

    IEnumerator PickupCoin()
    {
        while (target != null && Vector2.Distance(transform.position, target.position) > 0.1f)
        {
            transform.position = Vector2.MoveTowards(transform.position, target.position, pickupSpeed * Time.deltaTime);
            yield return null;
        }

        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.enabled = false;
        }
        moneySystem.AddCoin();

        Destroy(gameObject);
    }


}
