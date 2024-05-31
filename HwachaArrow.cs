using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal; // Correct namespace for Unity 2020.1 and later

public class HwachaArrow : MonoBehaviour
{
    public LayerMask targetLayer;
    public Sprite arrowUnlitSprite;
    public Sprite arrowLitSprite;
    public Sprite arrowHitSprite;
    public bool isPreparing = false;
    public AudioClip fireSound; // Assign the fire sound in the Inspector
    private Light2D myLight;

    private Rigidbody2D rb;
    private Animator arrowAnimator;
#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
    private Collider2D collider;
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        collider = GetComponent<Collider2D>();
        arrowAnimator = GetComponent<Animator>();
        myLight = GetComponentInChildren<Light2D>();
        // Set initial sprite to unlit
        arrowAnimator.enabled = false;
        myLight.enabled = false;
        collider.enabled = false;
        SetArrowSprite(arrowUnlitSprite);
    }

    void Update()
    {
        if (!rb.isKinematic)
        {
            AlignWithVelocity();
            collider.enabled = true;

        }
        if (isPreparing)
        {
            arrowAnimator.enabled = true;
            myLight.enabled = true;
        }
    }

    void AlignWithVelocity()
    {
        if (rb.velocity != Vector2.zero)
        {
            float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;

            // Adjust angle for leftward movement
            if (rb.velocity.x < 0)
            {
                angle += 180;
            }

            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
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
        // Check if the object is in the designated layer
        //if (((1 << other.gameObject.layer) & targetLayer) != 0)
        //{
        if (rb != null) rb.isKinematic = true; // Stop all physics simulation
        if (collider != null) collider.enabled = false; // Disable the collider

        StartCoroutine(ChangeColorBriefly(other.gameObject));

        StickToTarget(other.transform);

        //}
    }


    IEnumerator ChangeColorBriefly(GameObject target)
    {
        arrowAnimator.enabled = false;
        myLight.enabled = false;
        SetArrowSprite(arrowHitSprite);
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
        rb.isKinematic = true; // Stop the physics
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        transform.SetParent(target); // Stick to the collided object
    }

    public void playSound()
    {
        SoundManager.Instance.PlaySound(fireSound, transform);
    }
    void SetArrowSprite(Sprite sprite)
    {
        GetComponent<SpriteRenderer>().sprite = sprite;  
    }
}