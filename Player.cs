using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float edgeBoundary = 10f;

    [Header("Zoom Settings")]
    public Camera cam;
    public float zoomSpeed = 1f;
    public float minZoom = 2f;
    public float maxZoom = 10f;

    [Header("Screen Movement Limits")]
    public Vector2 screenMoveLimitsX = new Vector2(-10f, 10f);
    public Vector2 screenMoveLimitsY = new Vector2(-10f, 10f);

    private Transform parentTransform;

    private void Start()
    {
        cam = cam ?? Camera.main;
        parentTransform = transform.parent;
    }

    private void Update()
    {
        HandleZoom();
        HandleKeyboardMovement();
        HandleEdgeMovement();

    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - scroll * zoomSpeed, minZoom, maxZoom);
    }

    private void HandleEdgeMovement()
    {
        Vector3 moveDirection = Vector3.zero;

        if (Input.mousePosition.x >= Screen.width - edgeBoundary) moveDirection.x += moveSpeed;
        if (Input.mousePosition.x <= edgeBoundary) moveDirection.x -= moveSpeed;
        if (Input.mousePosition.y >= Screen.height - edgeBoundary) moveDirection.y += moveSpeed;
        if (Input.mousePosition.y <= edgeBoundary) moveDirection.y -= moveSpeed;

        moveDirection *= Time.deltaTime;
        transform.Translate(moveDirection, Space.World);
    }

    private void HandleKeyboardMovement()
    {
        float moveSpeedModifier = Input.GetKey(KeyCode.LeftShift) ? 2f : 1f;
        Vector3 moveIncrement = new Vector3(
            Input.GetAxis("Horizontal") * moveSpeed * moveSpeedModifier * Time.deltaTime,
            Input.GetAxis("Vertical") * moveSpeed * moveSpeedModifier * Time.deltaTime,
            0);

        if (parentTransform != null)
        {
            parentTransform.position = ClampPosition(parentTransform.position + moveIncrement);
        }

        transform.position = ClampPosition(transform.position + moveIncrement);
    }

    private Vector3 ClampPosition(Vector3 position)
    {
        position.x = Mathf.Clamp(position.x, screenMoveLimitsX.x, screenMoveLimitsX.y);
        position.y = Mathf.Clamp(position.y, screenMoveLimitsY.x, screenMoveLimitsY.y);
        return position;
    }
}