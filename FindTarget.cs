using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindTarget : MonoBehaviour
{
    public float maxDetectionRange = 50f; // Max range for detecting targets and obstacles
    public LayerMask targetLayer; // Layer on which targets are located
    public LayerMask obstacleLayer; // Layer on which obstacles are located
    public Transform target;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        target = findTarget(targetLayer);
    }

    public Transform findTarget(LayerMask layerOfTarget)
    {
        Transform rTarget;
        // Raycast to the right
        RaycastHit2D hitRight = Physics2D.Raycast(rb.position, Vector2.right, maxDetectionRange, layerOfTarget);
        Debug.DrawLine(rb.position, hitRight.point, Color.green, 2f); // Green right line if a target is hit
        if (hitRight.collider != null)
        {
            rTarget = hitRight.transform;
            return rTarget;
        }

        // Raycast to the left
        RaycastHit2D hitLeft = Physics2D.Raycast(rb.position, Vector2.left, maxDetectionRange, layerOfTarget);
        Debug.DrawLine(rb.position, hitLeft.point, Color.red, 2f); // Red left line if a target is hit
        if (hitLeft.collider != null)
        {
            rTarget = hitLeft.transform;
            return rTarget;
        }

        // If no target found
        rTarget = null;
        return rTarget;
    }

}
