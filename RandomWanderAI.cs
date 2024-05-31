using UnityEngine;

public class RandomWanderAI : MonoBehaviour
{
    public float directionChangeInterval = 3.0f;
    public float maxIdleTime = 2.0f;
    public float maxWanderRange = 10.0f;
    public float minWanderRange = 5.0f;

    private Movement movement;
    private Vector2 targetPoint;
    private bool isMoving;
    private Unit unitComponent;
    private bool wasAttacking = false;


    private void Start()
    {
        movement = GetComponent<Movement>();
        unitComponent = GetComponent<Unit>();
        ChooseNewAction();
    }

    void Update()
    {
        if (unitComponent != null && unitComponent.isAttacking)
        {
            if (isMoving)
            {
                movement.StopMovement();
                isMoving = false;
            }
            wasAttacking = true;
            return;
        }
        else
        {
            // Resume wandering if there was an attack in the previous frame and now there's no target
            if (wasAttacking && unitComponent.target == null && !isMoving)
            {
                ChooseNewAction();
                wasAttacking = false; // Reset the flag
            }
        }
    }


    public void ChooseNewAction()
    {
        if (unitComponent != null && unitComponent.isAttacking) return;

        if (Random.Range(0, 2) == 0) // 50% chance to move or idle
        {
            if (!isMoving)
            {
                targetPoint = GenerateRandomPoint();
                movement.MoveToPoint(targetPoint, false);
                isMoving = true;
            }
        }
        else
        {
            movement.StopMovement();
            isMoving = false;
        }

        float waitTime = isMoving ? directionChangeInterval : Random.Range(2f, maxIdleTime);
        Invoke("ChooseNewAction", waitTime);
    }

    private Vector2 GenerateRandomPoint()
    {
        float randomRange = Random.Range(minWanderRange, maxWanderRange);
        float randomX;

        if (Random.Range(0, 2) == 0) // 50% chance to pick a negative range
        {
            randomX = transform.position.x - randomRange;
        }
        else
        {
            randomX = transform.position.x + randomRange;
        }

        float randomY = transform.position.y; // Keeping Y constant for 2D movement
        return new Vector2(randomX, randomY);
    }

}
