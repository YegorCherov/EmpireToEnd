using UnityEngine;

public class CatapultStone : MonoBehaviour
{
    public float explosionRadius = 3f;
    public float explosionForce = 500f;
    public float damage = 50f;
    public GameObject explosionEffectPrefab;
    public LayerMask damageLayer;
    public AudioClip explosionSound;

    private Rigidbody2D rb;
    private const float gravity = 9.81f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Launch(Transform target, float launchForce, float angleAdjustment, float minAngle, float maxAngle)
    {
        rb.isKinematic = false;
        Vector2 toTarget = target.position - transform.position;
        float distanceX = Mathf.Abs(toTarget.x);
        float distanceY = toTarget.y;
        float angle = CalculateFiringAngle(distanceX, distanceY, launchForce, angleAdjustment, minAngle, maxAngle);
        Debug.Log(angle);
        float radianAngle = angle * Mathf.Deg2Rad;
        Vector2 launchDirection = new Vector2(Mathf.Cos(radianAngle), Mathf.Sin(radianAngle));
        rb.velocity = launchDirection * launchForce;
    }


    private float CalculateFiringAngle(float distanceX, float distanceY, float finalForce, float angleAdjustment, float minAngle, float maxAngle)
    {
        // Check for invalid input values
        if (distanceX <= 0f || distanceY == 0f || finalForce <= 0f)
        {
            Debug.LogWarning("Invalid input values for CalculateFiringAngle method.");
            return 30f;
        }

        // Calculate firing angle based on distance and constant force
        // Adjust calculation to account for Y position
        float angleRad = Mathf.Asin((gravity * distanceX) / Mathf.Pow(finalForce, 2)) / 2.0f;

        // Check for NaN or infinity values
        if (float.IsNaN(angleRad) || float.IsInfinity(angleRad))
        {
            Debug.LogWarning("Invalid angle calculation in CalculateFiringAngle method.");
            return 30f;
        }

        // Adjust for Y position difference
        angleRad += Mathf.Atan2(distanceY, distanceX);

        float angle = angleRad * Mathf.Rad2Deg;
        angle += angleAdjustment;
        angle = Mathf.Clamp(angle, minAngle, maxAngle);
        return angle;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Explode();
    }

    private void Explode()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius, damageLayer);

        foreach (Collider collider in colliders)
        {
            Unit unit = collider.GetComponent<Unit>();
            if (unit != null)
            {
                unit.Hit(damage);
            }

            Rigidbody2D rb2 = collider.GetComponent<Rigidbody2D>();
            if (rb2 != null)
            {
                //rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            }
        }

        GameObject explosionEffect = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        SoundManager.Instance.PlaySound(explosionSound, transform);
        Destroy(explosionEffect, 2f);

        Destroy(gameObject);
    }
}