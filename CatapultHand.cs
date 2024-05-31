using UnityEngine;
using System.Collections;

public class CatapultHand : MonoBehaviour
{
    public float attackDelay = 5f;
    public float loadingTime = 3f;
    public AudioClip loadingSound;
    public AudioClip firingSound;
    public GameObject boulderPrefab;
    public Transform boulderSpawnPosition;
    public float launchForce = 10f;
    public float angleAdjustment = 0f;
    public float minAngle = 1f;
    public float maxAngle = 30f;

    private float nextAttackTime;
    private bool isLoaded;
    private GameObject currentBoulder;
    private Animator animator;
    private Transform currentTarget;

    private void Start()
    {
        animator = GetComponent<Animator>();
        nextAttackTime = Time.time;
        isLoaded = true;
        SpawnBoulder();
    }

    public void Attack()
    {
        if (isLoaded && Time.time >= nextAttackTime)
        {
            animator.SetBool("isAttacking", true);
        }
    }

    public void AttackEnd()
    {
        animator.SetBool("isAttacking", false);
        nextAttackTime = Time.time + attackDelay;
        isLoaded = false;
    }

    private void SpawnBoulder()
    {
        currentBoulder = Instantiate(boulderPrefab, boulderSpawnPosition.position, boulderSpawnPosition.rotation, transform);
        currentBoulder.GetComponent<Rigidbody2D>().isKinematic = true;
    }

    public void SetTarget(Transform target)
    {
        currentTarget = target;
    }

    public void ClearTarget()
    {
        currentTarget = null;
    }

    public void FireProjectile()
    {
        if (currentBoulder != null && currentTarget != null)
        {
            // Detach the boulder from the catapult hand
            currentBoulder.transform.SetParent(null);

            // Get the Rigidbody component of the boulder
            Rigidbody2D boulderRigidbody = currentBoulder.GetComponent<Rigidbody2D>();

            if (boulderRigidbody != null)
            {
                // Set the boulder's Rigidbody to dynamic
                boulderRigidbody.isKinematic = false;

                // Launch the boulder towards the target
                currentBoulder.GetComponent<CatapultStone>().Launch(currentTarget, launchForce, angleAdjustment, minAngle, maxAngle);
                currentBoulder = null;
                SoundManager.Instance.PlaySound(firingSound, transform);
                StartCoroutine(Reload());
            }
            else
            {
                Debug.LogError("Rigidbody2D component is missing on the boulder prefab.");
            }
        }
    }

    private IEnumerator Reload()
    {
        yield return new WaitForSeconds(loadingTime);
        SoundManager.Instance.PlaySound(loadingSound, transform);
        SpawnBoulder();
        isLoaded = true;
    }
}