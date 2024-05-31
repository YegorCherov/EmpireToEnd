using System;
using UnityEngine;

public class AnimationPlayer : MonoBehaviour
{
    private Animator animator;
    private Movement movementScript;
    private Unit unit;


    void Start()
    {
        // Initialization movement script
        movementScript = GetComponent<Movement>();
        if (movementScript == null)
        {
            Debug.LogError("Movement component not found!");
        }

        // Get reference to the Animator component
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator component not found!");
        }

        unit = GetComponent<Unit>();
        if (unit == null)
        {
            Debug.LogError("Unit component not found!");
        }
    }

    void Update()
    {
        // Check for input or conditions to trigger animations
        // For demonstration purposes, I'm using Input.GetKeyDown(KeyCode.X)
        // You can replace this with your own conditions based on player input or other game events

        animator.SetInteger("Speed", movementScript.currentSpeed);

        if (Input.GetKeyDown(KeyCode.O))
        {
            TriggerOperate();
        }
        else if (Input.GetKeyDown(KeyCode.B))
        {
            TriggerBuild();
        }
    }

    // Methods to trigger each animation
    public void TriggerOperate()
    {
        animator.SetBool("isOperating", true);
    }

    public void TriggerBuild()
    {
        animator.SetBool("isBuilding", true);
    }
    public void TriggerAttack()
    {
        // Set the parameter "isWalking" if it exists
        animator.SetBool("isAttacking", true);
    }
    public void EndAttack()
    {
        // Set the parameter "isWalking" if it exists
        animator.SetBool("isAttacking", false);
    }

    // Custom method to check if a parameter exists in the Animator Controller
    private bool ParameterExists(string paramName)
    {
        // Check if the parameter exists by trying to get its value
        // If the parameter exists, GetBool will return the default value (false)
        // If the parameter doesn't exist, GetBool will throw an exception
        try
        {
            animator.GetBool(paramName);
            return true;
        }
        catch (System.ArgumentException)
        {
            return false;
        }
    }

    internal void TriggerHit()
    {
        throw new NotImplementedException();
    }
}
