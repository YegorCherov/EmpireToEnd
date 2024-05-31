using System;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    private Animator animator;
    private PlayerMovement playerScript;

    void Start()
    {
        // Get reference to the Animator component
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator component not found!");
        }
        playerScript = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        // Calculate current speed based on change in position
        animator.SetInteger("Speed", playerScript.currentSpeed);

        // Check for input or conditions to trigger animations
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
        animator.SetBool("isAttacking", true);
    }

    public void EndAttack()
    {
        animator.SetBool("isAttacking", false);
    }
    internal void TriggerHit()
    {
        throw new NotImplementedException();
    }

    // Custom method to check if a parameter exists in the Animator Controller
    private bool ParameterExists(string paramName)
    {
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
}
