// UnitGroupMovement.cs
using System.Collections.Generic;
using UnityEngine;

public class UnitGroupMovement : MonoBehaviour
{
    private static UnitGroupMovement _instance;
    public static UnitGroupMovement Instance { get { return _instance; } }

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    public void MoveSelectedUnits(Vector2 targetPosition)
    {
        List<GameObject> selectedUnits = UnitSelections.Instance.unitsSelected;
        if (selectedUnits.Count == 0)
            return;

        Vector2 minPosition = new Vector2(Mathf.Infinity, Mathf.Infinity);
        Vector2 maxPosition = new Vector2(Mathf.NegativeInfinity, Mathf.NegativeInfinity);

        foreach (GameObject unit in selectedUnits)
        {
            Vector2 unitPosition = unit.transform.position;
            minPosition = Vector2.Min(minPosition, unitPosition);
            maxPosition = Vector2.Max(maxPosition, unitPosition);
        }

        Vector2 groupSize = maxPosition - minPosition;
        Vector2 groupCenter = (minPosition + maxPosition) / 2f;

        Vector2 offset = targetPosition - groupCenter;

        foreach (GameObject unit in selectedUnits)
        {
            Vector2 unitPosition = unit.transform.position;
            Vector2 newPosition = unitPosition + offset;

            // Clamp the new position within the specified range
            newPosition.x = Mathf.Clamp(newPosition.x, float.NegativeInfinity, float.PositiveInfinity);
            newPosition.y = Mathf.Clamp(newPosition.y, -10f, 10f);

            Movement movement = unit.GetComponent<Movement>();
            if (movement != null)
            {
                movement.MoveToPoint(newPosition, false);
            }
        }
    }

}