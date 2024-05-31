using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float maxScale = 1f;
    public float minScale = 0.7f;
    public float minY = -5f; // Assuming this is the lowest point in your scene
    public float maxY = 5f;  // Assuming this is the highest point in your scene

    private void Update()
    {
        // Player movement
        float moveX = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;
        float moveY = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;
        transform.Translate(moveX, moveY, 0);
    }
}
