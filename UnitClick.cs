using UnityEngine;

public class UnitClick : MonoBehaviour
{
    private Camera myCam;
    public LayerMask clickable;
    public LayerMask ground;
    public GameObject flagPrefab; // Reference to the flag prefab
    public float groundY; // Y-coordinate of the ground

    private GameObject currentFlag; // Reference to the currently placed flag
    private GameObject lastClickedObject;
    private float lastClickTime;
    private const float doubleClickThreshold = 0.3f; // Adjust this value to set the double-click time threshold

    void Start()
    {
        myCam = Camera.main;
        if (myCam == null)
        {
            Debug.LogError("Main Camera not found!");
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Left mouse button clicked");

            Vector2 mousePos = myCam.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity, clickable);

            if (hit.collider != null)
            {
                Debug.Log("Raycast hit: " + hit.collider.gameObject.name);

                // Check for double-click
                if (hit.collider.gameObject == lastClickedObject && Time.time - lastClickTime < doubleClickThreshold)
                {
                    // Double-click detected
                    UnitSelections.Instance.SelectAllUnitsOfType(hit.collider.gameObject);
                    lastClickedObject = null; // Reset the last clicked object
                }
                else
                {
                    // If we hit a clickable object
                    if (Input.GetKey(KeyCode.LeftShift))
                    {
                        UnitSelections.Instance.ShiftClickSelect(hit.collider.gameObject);
                    }
                    else
                    {
                        UnitSelections.Instance.ClickSelect(hit.collider.gameObject);
                    }

                    lastClickedObject = hit.collider.gameObject;
                    lastClickTime = Time.time;
                }
            }
            else
            {
                Debug.Log("Raycast did not hit any clickable object");
                // If we didn't hit anything and we are not shift clicking
                if (!Input.GetKey(KeyCode.LeftShift))
                {
                    UnitSelections.Instance.DeselectAll();
                }

                lastClickedObject = null; // Reset the last clicked object
            }
        }
        if (Input.GetMouseButtonDown(1)) // Right mouse button
        {
            Vector2 mousePos = myCam.ScreenToWorldPoint(Input.mousePosition);
            float targetX = mousePos.x;
            // Clamp targetY within the specified range
            float targetY = Mathf.Clamp(mousePos.y, -10f, 10f);
            Vector2 targetPosition = new Vector2(targetX, targetY);



            // Remove the previous flag, if any
            if (currentFlag != null)
            {
                currentFlag.GetComponent<FlagAnimation>().FadeAndDestroy();
                currentFlag = null;
            }

            // Place the new flag at the target position
            Vector3 placePosition = new Vector3(targetX, targetY, 0f);
            currentFlag = Instantiate(flagPrefab, placePosition, Quaternion.identity);

            // Call the MoveSelectedUnits method in UnitGroupMovement script
            UnitGroupMovement.Instance.MoveSelectedUnits(targetPosition);
        }
    }
}