using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hwacha : MonoBehaviour
{
    public GameObject arrowPrefab;
    public Transform minFirePoint;
    public Transform maxFirePoint;
    public float fireAngle = 36f;
    public int arrowCount = 0;
    public int totalArrowCount = 72; // Total number of arrows to load
    public float reloadTime = 10f;
    public float fireDelay = 0.05f;
    public float fireSoundDelay = 11.7f;
    public AudioClip fireSound;
    public AudioClip reloadSound;
    public float minAttackRange = 40f;
    public float maxAttackRange = 50f;
    public Transform wheelTransform;

    private bool hasPlayedSound = false;
    public Builder currentBuilder;
    private Movement movementScript;
    public FindTarget findTargetScript;
    public bool isOperated = false;
    private bool isReloading = false;
    public List<GameObject> arrows = new List<GameObject>();
    public enum HwachaState
    {
        Idle,
        Moving,
        Firing,
        Reloading
    }
    private HwachaState currentState = HwachaState.Idle;

    void Start()
    {
        // Initialization...
        movementScript = GetComponent<Movement>();
        findTargetScript = GetComponent<FindTarget>();
        // Set the min and max fire points
        minFirePoint = new GameObject("MinFirePoint").transform;
        minFirePoint.parent = transform;
        minFirePoint.localPosition = new Vector3(0.3f, 0.3f, 0.15f);

        maxFirePoint = new GameObject("MaxFirePoint").transform;
        maxFirePoint.parent = transform;
        maxFirePoint.localPosition = new Vector3(-0.1f, 0.8f, 0.15f);
    }

    void Update()
    {
        /*Debug.Log("currentState: " + currentState);
        Debug.Log("isMoving: " + currentBuilder.GetComponent<Movement>().isMoving);
        Debug.Log("isOperated: " + isOperated);
        Debug.Log("currentBuilder: " + currentBuilder);
        Debug.Log("arrowCount: " + arrowCount);*/
        switch (currentState)
        {
            case HwachaState.Moving:
                if (currentBuilder != null && currentBuilder.GetComponent<Movement>().isMoving)
                {
                    FollowBuilder();
                }
                else
                {
                    ChangeState(HwachaState.Idle);
                }
                break;

            case HwachaState.Idle:
                if (findTargetScript.target != null && isOperated && arrowCount == totalArrowCount)
                {
                    OrientHwacha(findTargetScript.target.position); // Orient the Hwacha before firing
                    ChangeState(HwachaState.Firing);
                }
                else if (isOperated && currentBuilder != null && arrowCount == 72 && currentBuilder.GetComponent<Movement>().isMoving)
                {
                    ChangeState(HwachaState.Moving);
                }
                else if (isOperated && currentBuilder != null && !currentBuilder.GetComponent<Movement>().isMoving)
                {
                    if (arrowCount == 0 && !isReloading)
                    {
                        ChangeState(HwachaState.Reloading);
                    }
                }
                break;

            case HwachaState.Reloading:
                if (!isReloading)
                    StartCoroutine(Reload());
                break;

            case HwachaState.Firing:
                if (!isReloading && !hasPlayedSound) // Check if not reloading and sound not played yet
                {
                    StartCoroutine(FireArrows());
                }
                break;
        }
    }

    public void OperateHwacha(Builder builder)
    {
        if (!isOperated)
        {
            currentBuilder = builder;
            isOperated = true;

            // Transfer movement control to Builder
            if (movementScript != null)
            {
                movementScript.enabled = false;
            }

            // Position the builder at the handling spot
            currentBuilder.transform.position = transform.position + new Vector3(-1.0f, 0, 0); // Example offset
            currentBuilder.OperateHwacha(this);
        }
    }

    public void StopOperation()
    {
        if (isOperated)
        {
            isOperated = false;
            if (currentBuilder != null)
            {
                // Return movement control to Builder
                if (movementScript != null)
                {
                    movementScript.enabled = true;
                }

                currentBuilder.StopOperatingHwacha();
                currentBuilder = null;
            }
        }
    }

    IEnumerator Reload()
    {
        isReloading = true;
        // Play reload sound if uncommented
        // SoundManager.Instance.PlaySound(reloadSound, transform.position);

        // Calculate time interval between arrow instantiations
        float arrowSpawnInterval = reloadTime / totalArrowCount;

        arrowCount = 0; // Reset the arrow count

        for (int i = 0; i < totalArrowCount; i++)
        {
            Vector3 firePosition = Vector3.Lerp(minFirePoint.position, maxFirePoint.position, (float)i / totalArrowCount);

            // Use the absolute value of fireAngle to ensure it's always positive
            float adjustedFireAngle = Mathf.Abs(fireAngle);

            // If the Hwacha is flipped, negate the angle
            if (transform.localScale.x < 0)
            {
                adjustedFireAngle *= -1;
            }

            GameObject arrowObject = Instantiate(arrowPrefab, firePosition, Quaternion.Euler(0, 0, adjustedFireAngle), transform);
            Rigidbody2D rb = arrowObject.GetComponent<Rigidbody2D>();
            if (rb != null && !rb.isKinematic)
            {
                rb.isKinematic = true; // Freeze the arrow
            }
            arrowCount++;
            arrows.Add(arrowObject);
            yield return new WaitForSeconds(arrowSpawnInterval);
        }

        isReloading = false;
        ChangeState(HwachaState.Idle);
    }


    IEnumerator FireArrows()
    {
        if (!hasPlayedSound)
        {
            SoundManager.Instance.PlaySound(fireSound, transform);
            hasPlayedSound = true; // Set the flag to indicate that the sound has been played
        }

        foreach (GameObject arrow in arrows)
        {
            arrow.GetComponent<HwachaArrow>().isPreparing = true;
        }
        yield return new WaitForSeconds(fireSoundDelay);

        // Create a copy of the arrows list to avoid modifying it while iterating
        List<GameObject> arrowsCopy = new List<GameObject>(arrows);

        // Unfreeze and fire each arrow with a delay
        foreach (GameObject arrow in arrowsCopy)
        {
            arrowCount--;
            arrows.Remove(arrow);
            arrow.GetComponent<HwachaArrow>().isPreparing = false;
            FireArrow(arrow);
            yield return new WaitForSeconds(fireDelay);
        }
        arrows.Clear();
        ChangeState(HwachaState.Idle);
        hasPlayedSound = false;
    }


    void FireArrow(GameObject arrow)
    {
        Rigidbody2D rb = arrow.GetComponent<Rigidbody2D>();
        rb.isKinematic = false; // Unfreeze the arrow

        // Calculate the force magnitude between 20f and 25f
        float forceMagnitude = Random.Range(minAttackRange/2, maxAttackRange/2);

        // Adjust the fire direction based on Hwacha's orientation
        float adjustedFireAngle = transform.localScale.x > 0 ? fireAngle : 180 - fireAngle;
        Vector2 forceDirection = Quaternion.Euler(0, 0, adjustedFireAngle) * Vector2.right;

        // Apply force to the rigidbody
        rb.AddForce(forceDirection * forceMagnitude, ForceMode2D.Impulse);

        // Play Sound
        arrow.GetComponent<HwachaArrow>().playSound();
    }



    void FollowBuilder()
    {
        // Move towards the builder
        Vector2 builderPosition = new Vector2(currentBuilder.transform.position.x, transform.position.y);
        movementScript.MoveToPoint(builderPosition, false);

        // Check if moving with speed 2
        if (movementScript.currentSpeed != 0)
        {
            RotateWheel();
        }
    }

    void RotateWheel()
    {
        // Set rotation speed for the wheel
        float rotationSpeed = -100f; // Adjust this value as needed

        // Determine rotation direction based on scale
        float rotationDirection = Mathf.Sign(transform.localScale.x);
        rotationSpeed *= rotationDirection;
        //Debug.Log("rotationSpeed: " + rotationSpeed);
        //Debug.Log("rotationDirection: " + rotationDirection);

        // Rotate the wheel
        wheelTransform.Rotate(new Vector3(0, 0, rotationDirection) * rotationSpeed * Time.deltaTime);
    }

    void OrientHwacha(Vector3 targetPosition)
    {
        // Determine if the target is to the left or right of the Hwacha
        bool isTargetLeft = targetPosition.x < transform.position.x;

        // If the Hwacha is not facing the target, flip it
        if ((isTargetLeft && transform.localScale.x > 0) || (!isTargetLeft && transform.localScale.x < 0))
        {
            FlipHwacha();
        }
    }

    void FlipHwacha()
    {
        // Flip the Hwacha by reversing the x scale
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;

        // Optionally, adjust other components (like fire points) if needed
    }

    // Public method to be called by Builder to change states
    public void ChangeState(HwachaState newState)
    {
        currentState = newState;
    }

}