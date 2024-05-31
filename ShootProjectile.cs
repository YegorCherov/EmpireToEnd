using System.Collections;
using UnityEngine;

public class ShootProjectile : MonoBehaviour
{
    public GameObject ProjectilePrefab;
    public float forceMagnitude = 20f; // Constant force magnitude applied to the Projectile
    public float angleVariance = 3f; // Variance in angle
    public float forceVariance = 3f; // Variance in force
    public float ProjectileLifetime = 5f; // Time after which the Projectile gets destroyed
    public float finalForce = 20f;

    private const float gravity = 9.81f; // Gravity constant
    private Transform firePoint; // Point from where the Projectile is fired
    private Transform target; // The target the Archer is aiming at

    /*private bool IsObstacleBetweenTarget()
    {
        RaycastHit2D hit = Physics2D.Raycast(firePoint.position, target.position - firePoint.position, Vector2.Distance(firePoint.position, target.position), obstacleLayer);
        return hit.collider != null;
    }*/

    private void Awake()
    {
        firePoint = transform;
    }
    private void Update()
    {
        if (GetComponent<Archer>())
        {
            target = GetComponent<Archer>().target;
        }
    }
    private float CalculateFiringAngle(float distanceX, float distanceY)
    {
        // Calculate firing angle based on distance and constant force
        // Adjust calculation to account for Y position
        float angleRad = Mathf.Asin((gravity * distanceX) / Mathf.Pow(finalForce, 2)) / 2.0f;
        // Adjust for Y position difference
        angleRad += Mathf.Atan2(distanceY, distanceX);
        return angleRad * Mathf.Rad2Deg;
    }


    public void FireProjectile()
    {
        if (target == null) return;

        Vector3 spawnPosition = new Vector3(firePoint.position.x, firePoint.position.y, 0.15f);
        GameObject Projectile = Instantiate(ProjectilePrefab, spawnPosition, Quaternion.identity);
        Arrow arrowScript = Projectile.GetComponent<Arrow>();
        if (arrowScript != null)
        {
            arrowScript.targetLayer = GetComponent<FindTarget>().targetLayer;
        }
        Rigidbody2D ProjectileRb = Projectile.GetComponent<Rigidbody2D>();

        Vector2 toTarget = target.position - firePoint.position;
        float distanceX = Mathf.Abs(toTarget.x);
        float distanceY = toTarget.y;

        finalForce = forceMagnitude + Random.Range(-forceVariance, forceVariance);

        //bool obstacleDetected = IsObstacleBetweenTarget();
        float firingAngle = CalculateFiringAngle(distanceX, distanceY/*, obstacleDetected*/) + Random.Range(-angleVariance, angleVariance);

        Vector2 forceDirection = new Vector2(Mathf.Sign(toTarget.x), 0).normalized;
        if (transform.localScale.x < 0) // Assuming negative scale means facing left
        {
            // Flip the angle if archer is facing left
            firingAngle = -firingAngle;
        }
        ProjectileRb.AddForce(Quaternion.Euler(0, 0, firingAngle) * forceDirection * finalForce, ForceMode2D.Impulse);

        DestroyProjectileAfterTime(Projectile, ProjectileLifetime);
    }

    private void DestroyProjectileAfterTime(GameObject Projectile, float delay)
    {
        Destroy(Projectile, delay);
    }

}
