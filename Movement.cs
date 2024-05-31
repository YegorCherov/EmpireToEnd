using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public int currentSpeed = 0;
    public int walkSpeed = 2;
    public int runSpeed = 5;
    public Vector2 currentDestination;
    public bool isMoving = false;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // Check if the point is reached and stop movement if it is
        if (Vector2.Distance(transform.position, currentDestination) < 0.5f && currentDestination != Vector2.zero) // Threshold value
        {
            StopMovement();
            currentDestination = Vector2.zero;
        }
        isMoving = currentSpeed > 0;
    }
    public void MoveTowardsTarget(Transform target)
    {
        // Calculate a point towards the target
        Vector2 targetPoint = target.position;
        float distanceToTarget = Vector2.Distance(transform.position, target.position);

        // Stop moving if close enough to the coin
        if (distanceToTarget <= 1f) // Adjust this value as needed
        {
            // Stop the unit's movement
            // Assuming you have a method in Movement.cs to stop the unit
            StopMovement();
        }
        else
        {
            // Continue moving towards the coin
            MoveToPoint(targetPoint, true);
        }
    }

    public void MoveAwayFromTarget(Transform target)
    {
        Vector2 direction = (transform.position - target.position).normalized;
        Vector2 awayPoint = (Vector2)transform.position + direction * 2.0f;
        MoveToPoint(awayPoint, true);
    }

    public void MoveToPoint(Vector2 point, bool Emergency)
    {
        currentDestination = point;
        Vector2 direction = (point - (Vector2)transform.position).normalized;

        // Start with initial speed if currently stationary
        currentSpeed = Emergency ? runSpeed : walkSpeed;

        rb.velocity = direction * currentSpeed;
        FlipTowardsDirection(direction.x);
    }


    public void StopMovement()
    {
        currentSpeed = 0;
        rb.velocity = Vector2.zero;
    }


    public void FlipTowardsDirection(float direction)
    {
        // Flip sprite to face direction
        if ((direction < 0 && transform.localScale.x > 0) || (direction > 0 && transform.localScale.x < 0))
        {
            Flip();
        }
    }

    public void FlipTowardsTarget(Transform target)
    {
        float directionToTarget = target.position.x - transform.position.x;
        if ((directionToTarget < 0 && transform.localScale.x > 0) || (directionToTarget > 0 && transform.localScale.x < 0))
        {
            Flip();
        }
    }

    private void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}
