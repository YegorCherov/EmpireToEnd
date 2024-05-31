using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField]
    private Camera sceneCamera;
    [SerializeField]
    private LayerMask placementLayermask;

    public Vector2 GetSelectedMapPosition()
    {
        Vector2 mousePos = Input.mousePosition;
        Vector3 worldPos = sceneCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, sceneCamera.nearClipPlane));
        return new Vector2(worldPos.x, worldPos.y);
    }
}
