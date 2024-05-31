using System.Collections;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    public LayerMask targetLayer;

    private Rigidbody2D rb;
    private bool hasCollided = false;
#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
    private Collider2D collider;
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        collider = GetComponent<Collider2D>();
    }

    void Update()
    {
        if (!hasCollided && !rb.isKinematic)
        {
            AlignWithVelocity();
        }
    }

    void AlignWithVelocity()
    {
        float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (rb != null) rb.isKinematic = true; // Stop all physics simulation
        if (collider != null) collider.enabled = false; // Disable the collider

        StartCoroutine(ChangeColorBriefly(collision.gameObject));

        StickToTarget(collision.transform);
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!hasCollided)
        {
            if (rb != null) rb.isKinematic = true; // Stop all physics simulation
            if (collider != null) collider.enabled = false; // Disable the collider

            StartCoroutine(ChangeColorBriefly(other.gameObject));

            StickToTarget(other.transform);

        }
    }


    IEnumerator ChangeColorBriefly(GameObject target)
    {
        if (target.GetComponent<SpriteRenderer>() != null)
        {
            SpriteRenderer renderer = target.GetComponent<SpriteRenderer>();
            Color originalColor = renderer.color;
            renderer.color = Color.black; // Change to white


            yield return new WaitForSeconds(0.2f); // Adjust the duration

            renderer.color = originalColor; // Revert to original color
            Destroy(target); // Destroy the target
        }
        yield return new WaitForSeconds(10f); // Adjust the duration
        Destroy(gameObject);
    }

    private void StickToTarget(Transform target)
    {
        hasCollided = true;
        rb.isKinematic = true; // Stop the physics
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        transform.SetParent(target); // Stick to the collided object
    }
}
