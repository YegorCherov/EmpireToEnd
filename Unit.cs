using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class Unit : MonoBehaviour
{
    public Transform target;
    public float attackDistanceMin = 0.0f;
    public float attackDistanceMax = 1.0f;
    public float attackAnimationDuration = 1f;
    public bool canAttack = false;
    public bool isAttacking = false;
    public AudioClip upgradeSound;


    private float attackTimer = 0.0f;
    private Movement movement;
    private MoneySystem moneySystem;
    private AnimationPlayer animationPlayer;
    private Animator animator;

    private float proximityTimer = 0f;
    private const float requiredProximityTime = 1f; // Time in seconds
    //private GameObject player; // Reference to the player GameObject

    void Start()
    {
        Debug.Log(gameObject);
        Debug.Log(UnitSelections.Instance);
        if (!UnitSelections.Instance.unitList.Contains(this.gameObject))
        {
            UnitSelections.Instance.unitList.Add(this.gameObject);
        }
    }
    void OnDestroy()
    {
        if (UnitSelections.Instance.unitList.Contains(this.gameObject))
        {
            UnitSelections.Instance.unitList.Remove(this.gameObject);
        }
    }
    public void Awake()
    {
        Start();
        //player = GameObject.FindGameObjectWithTag("Player");
        moneySystem = GetComponent<MoneySystem>();
        movement = GetComponent<Movement>();
        animationPlayer = GetComponent<AnimationPlayer>();
        animator = GetComponent<Animator>();
        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == "Attack")
            {
                attackAnimationDuration = clip.length;
                break;
            }
        }
    }

    public void Update()
    {
        if (isAttacking)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0.0f)
            {
                isAttacking = false;
            }
            return;
        }
        /*if (IsPlayerClose())
        {
            proximityTimer += Time.deltaTime;
            if (proximityTimer >= requiredProximityTime)
            {
                moneySystem.DropCoins();
                proximityTimer = 0f; // Reset the timer
            }
        }*/
        else
        {
            proximityTimer = 0f; // Reset the timer if the player moves away
        }


        target = GetComponent<FindTarget>().target;

        if (target != null && canAttack)
        {
            float distanceToTarget = Mathf.Abs(transform.position.x - target.position.x);

            if (distanceToTarget < attackDistanceMin)
            {
                movement.MoveAwayFromTarget(target);
            }
            else if (distanceToTarget <= attackDistanceMax)
            {
                StartAttack();
            }
            else
            {
                movement.MoveTowardsTarget(target);
            }
        }
        else if (target != null)
        {
            movement.MoveTowardsTarget(target);
            float distanceToTarget = Vector2.Distance(transform.position, target.position);

            // Stop moving if close enough to the coin
            if (distanceToTarget <= 0.5f) // Adjust this value as needed
            {
                // Stop the unit's movement
                // Assuming you have a method in Movement.cs to stop the unit
                movement.StopMovement();
            }
            else
            {
                // Continue moving towards the coin
                movement.MoveTowardsTarget(target);
            }
        }

    }

    private void StartAttack()
    {
        isAttacking = true;
        movement.StopMovement();
        movement.FlipTowardsTarget(target);
        animationPlayer.TriggerAttack();
        attackTimer = attackAnimationDuration;
    }

    //private bool IsPlayerClose()
    //{
        // Check if the player is within a certain distance
    //   return Vector2.Distance(transform.position, player.transform.position) < 1;
    //}
    public void UpgradeUnit(GameObject newUnitPrefab, AudioClip upgradeSound)
    {
        GameObject newUnit = Instantiate(newUnitPrefab, transform.position, Quaternion.identity);
        SoundManager.Instance.PlaySound(upgradeSound, transform);

        // Start the coroutine to reset coins after 1 second
        //StartCoroutine(ResetCoinsAfterDelay(newUnit));
        Debug.Log("SHOULD WORK 1");
        Destroy(gameObject);
    }

    internal void Hit(float damage)
    {
        throw new NotImplementedException();
    }
}
