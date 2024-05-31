using UnityEngine;
using System.Collections.Generic;

public class FormationManager : MonoBehaviour
{
    public float unitSpacing = 1f;
    public string flagTag = "Flag";

    private List<GameObject> selectedUnits = new List<GameObject>();
    private GameObject flag;

    private void Update()
    {
        selectedUnits = UnitSelections.Instance.unitsSelected;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            FindFlag();
            ArrangeInSkeinFormation();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            FindFlag();
            ArrangeInShieldFormation();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            FindFlag();
            ArrangeInSquareFormation();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            FindFlag();
            ArrangeInCircleFormation();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            FindFlag();
            ArrangeInStringFormation("G A Y");
        }
    }

    private void FindFlag()
    {
        flag = GameObject.FindGameObjectWithTag(flagTag);
    }

    private void ArrangeInSkeinFormation()
    {
        if (flag != null && selectedUnits.Count > 0)
        {
            Vector3 center = CalculateFormationCenter();
            int totalUnits = selectedUnits.Count;
            int directionalFlag = GetFlagDirection(center);
            // Calculate the direction from the flag to the center
            //Vector3 flagDirection = (center - flag.transform.position).normalized;

            // Define initial positions for the first unit
            Vector3 startPosition = flag.transform.position;

            // Starting index for the units
            int startIndex = 1;
            float verticalSpacing = 0f;
            float horizontalSpacing = 0f;

            // Define the current x and y positions for the row
            float currentX = startPosition.x;
            float currentY = startPosition.y;

            selectedUnits[0].GetComponent<Movement>().MoveToPoint(startPosition, true);
            // Iterate through rows
            for (int row = 0; startIndex < totalUnits; row++)
            {

                // Calculate the horizontal spacing between units in the row
                horizontalSpacing = horizontalSpacing + unitSpacing * directionalFlag;
                verticalSpacing = verticalSpacing + unitSpacing / 2 * directionalFlag;


                // Position the units in the current row Mathf.Clamp(currentY + verticalSpacing, -10, 10);
                Debug.Log(currentY);
                // Move the unit to its position
                selectedUnits[startIndex].GetComponent<Movement>().MoveToPoint(new Vector3(currentX + horizontalSpacing * -1, Mathf.Clamp(currentY + verticalSpacing, -10, 10), startPosition.z), true);
                if(startIndex + 1 < totalUnits)
                    selectedUnits[startIndex + 1].GetComponent<Movement>().MoveToPoint(new Vector3(currentX + horizontalSpacing * -1, Mathf.Clamp(currentY - verticalSpacing, -10, 10), startPosition.z), true);


                // Update the starting index for the next row
                startIndex += 2;
            }
        }
    }

    private void ArrangeInShieldFormation()
    {
        if (flag != null && selectedUnits.Count > 0)
        {
            Vector3 center = CalculateFormationCenter();
            int totalUnits = selectedUnits.Count;
            int directionalFlag = GetFlagDirection(center);

            // Calculate the direction from the flag to the center
            Vector3 flagDirection = (center - flag.transform.position).normalized;

            // Define initial positions for the first unit
            Vector3 startPosition = flag.transform.position;

            // Calculate the number of rows needed based on the total units
            int unitsPerColumn = Mathf.CeilToInt(20f / unitSpacing);

            int numOfColumns = Mathf.CeilToInt((float)totalUnits / (float)unitsPerColumn);

            Debug.Log("numOfColumns: "+numOfColumns);
            Debug.Log("totalUnits: " + totalUnits);
            Debug.Log("unitsPerColumn: " + unitsPerColumn);
            // Starting index for the units
            int startIndex = 0;

            float verticalSpacing;
            float horizontalSpacing = unitSpacing * -directionalFlag;

            // Define the current x and y positions for the row
            float currentX = startPosition.x;
            float currentY = 10;

            // Iterate through rows
            for (int column = 0; column < numOfColumns; column++)
            {
                // Position the units in the current row
                for (int i = 0; i < unitsPerColumn && startIndex < totalUnits; i++)
                {
                    verticalSpacing = unitSpacing * i;
                    // Move the unit to its position
                    selectedUnits[startIndex].GetComponent<Movement>().MoveToPoint(new Vector3(currentX, currentY - verticalSpacing, startPosition.z), true);


                    // Update the starting index for the next unit
                    startIndex++;
                }

                // Move to the next unit position
                currentX += horizontalSpacing;
            }
        }
    }

    private void ArrangeInSquareFormation()
    {
        if (flag != null && selectedUnits.Count > 0)
        {
            Vector3 flagPosition = flag.transform.position;
            int totalUnits = selectedUnits.Count;
            int squareSize = Mathf.CeilToInt(Mathf.Sqrt(totalUnits));
            int unitsPerSide = Mathf.CeilToInt((float)totalUnits / 4f);

            float sideLength = (unitsPerSide - 1) * unitSpacing;
            Vector3 bottomLeft = flagPosition - new Vector3(sideLength / 2f, sideLength / 2f, 0f);

            int startIndex = 0;

            // Position units along each side of the square
            for (int side = 0; side < 4; side++)
            {
                Vector3 sideStart;
                Vector3 sideDirection;

                switch (side)
                {
                    case 0: // Bottom side
                        sideStart = bottomLeft;
                        sideDirection = Vector3.right;
                        break;
                    case 1: // Right side
                        sideStart = bottomLeft + new Vector3(sideLength, 0f, 0f);
                        sideDirection = Vector3.up;
                        break;
                    case 2: // Top side
                        sideStart = bottomLeft + new Vector3(sideLength, sideLength, 0f);
                        sideDirection = Vector3.left;
                        break;
                    case 3: // Left side
                        sideStart = bottomLeft + new Vector3(0f, sideLength, 0f);
                        sideDirection = Vector3.down;
                        break;
                    default:
                        sideStart = bottomLeft;
                        sideDirection = Vector3.zero;
                        break;
                }

                // Position units along the current side
                for (int i = 0; i < unitsPerSide && startIndex < totalUnits; i++)
                {
                    Vector3 unitPosition = sideStart + sideDirection * (i * unitSpacing);
                    unitPosition.y = Mathf.Clamp(unitPosition.y, -10f, 10f);
                    selectedUnits[startIndex].GetComponent<Movement>().MoveToPoint(unitPosition, true);
                    startIndex++;
                }
            }

            // If there are remaining units, position them inside the square
            if (startIndex < totalUnits)
            {
                float innerSideLength = sideLength - unitSpacing * 2f;
                Vector3 innerBottomLeft = flagPosition - new Vector3(innerSideLength / 2f, innerSideLength / 2f, 0f);

                // Position remaining units inside the square
                for (int i = startIndex; i < totalUnits; i++)
                {
                    float x = innerBottomLeft.x + (i % squareSize) * unitSpacing;
                    float y = innerBottomLeft.y + (i / squareSize) * unitSpacing;
                    y = Mathf.Clamp(y, -10f, 10f);
                    Vector3 unitPosition = new Vector3(x, y, flagPosition.z);
                    selectedUnits[i].GetComponent<Movement>().MoveToPoint(unitPosition, true);
                }
            }
        }
    }

    private void ArrangeInCircleFormation()
    {
        if (flag != null && selectedUnits.Count > 0)
        {
            Vector3 flagPosition = flag.transform.position;
            int totalUnits = selectedUnits.Count;
            float radius = (totalUnits - 1) * unitSpacing / (2 * Mathf.PI);

            float angleIncrement = 360f / totalUnits;
            float currentAngle = 0f;

            for (int i = 0; i < totalUnits; i++)
            {
                float angle = currentAngle * Mathf.Deg2Rad;
                float x = flagPosition.x + radius * Mathf.Cos(angle);
                float y = flagPosition.y + radius * Mathf.Sin(angle);
                y = Mathf.Clamp(y, -10f, 10f);

                Vector3 unitPosition = new Vector3(x, y, flagPosition.z);
                selectedUnits[i].GetComponent<Movement>().MoveToPoint(unitPosition, true);

                currentAngle += angleIncrement;
            }
        }
    }

    private void ArrangeInStringFormation(string input)
    {
        if (flag != null && selectedUnits.Count > 0)
        {
            Vector3 flagPosition = flag.transform.position;
            int totalUnits = selectedUnits.Count;
            input = input.ToUpper();

            float characterWidth = unitSpacing * 8f; // Adjust character width as needed
            float totalWidth = input.Length * characterWidth;

            Vector3 startPosition = flagPosition - new Vector3(totalWidth / 2f, 0f, 0f);

            int unitIndex = 0;

            for (int i = 0; i < input.Length; i++)
            {
                char character = input[i];

                if (character == ' ')
                {
                    startPosition.x += characterWidth;
                    continue;
                }

                float x = startPosition.x;
                float y = flagPosition.y;
                y = Mathf.Clamp(y, -10f, 10f);

                Vector3[] characterPositions = GetCharacterPositions(character, x, y);

                foreach (Vector3 position in characterPositions)
                {
                    if (position != Vector3.zero && unitIndex < totalUnits)
                    {
                        selectedUnits[unitIndex].GetComponent<Movement>().MoveToPoint(position, true);
                        unitIndex++;
                    }
                }

                startPosition.x += characterWidth;
            }
        }
    }

    private Vector3[] GetCharacterPositions(char character, float x, float y)
    {
        Vector3[] positions = new Vector3[50];

        switch (character)
        {
            case 'Y':
                positions[0] = new Vector3(x - unitSpacing * 2f, y + unitSpacing * 4f, 0f);
                positions[1] = new Vector3(x + unitSpacing * 2f, y + unitSpacing * 4f, 0f);
                positions[2] = new Vector3(x - unitSpacing, y + unitSpacing * 3f, 0f);
                positions[3] = new Vector3(x + unitSpacing, y + unitSpacing * 3f, 0f);
                positions[4] = new Vector3(x, y + unitSpacing * 2f, 0f);
                positions[5] = new Vector3(x, y + unitSpacing, 0f);
                positions[6] = new Vector3(x, y, 0f);
                positions[7] = new Vector3(x, y - unitSpacing, 0f);
                positions[8] = new Vector3(x, y - unitSpacing * 2f, 0f);
                positions[9] = new Vector3(x, y - unitSpacing * 3f, 0f);
                positions[10] = new Vector3(x, y - unitSpacing * 4f, 0f);
                break;
            case 'A':
                positions[0] = new Vector3(x, y + unitSpacing * 4f, 0f);
                positions[1] = new Vector3(x - unitSpacing, y + unitSpacing * 3f, 0f);
                positions[2] = new Vector3(x + unitSpacing, y + unitSpacing * 3f, 0f);
                positions[3] = new Vector3(x - unitSpacing * 2f, y + unitSpacing * 2f, 0f);
                positions[4] = new Vector3(x + unitSpacing * 2f, y + unitSpacing * 2f, 0f);
                positions[5] = new Vector3(x - unitSpacing * 3f, y + unitSpacing, 0f);
                positions[6] = new Vector3(x + unitSpacing * 3f, y + unitSpacing, 0f);
                positions[7] = new Vector3(x - unitSpacing * 4f, y, 0f);
                positions[8] = new Vector3(x + unitSpacing * 4f, y, 0f);
                positions[9] = new Vector3(x - unitSpacing * 4f, y - unitSpacing, 0f);
                positions[10] = new Vector3(x - unitSpacing * 3f, y - unitSpacing, 0f);
                positions[11] = new Vector3(x - unitSpacing * 2f, y - unitSpacing, 0f);
                positions[12] = new Vector3(x - unitSpacing, y - unitSpacing, 0f);
                positions[13] = new Vector3(x, y - unitSpacing, 0f);
                positions[14] = new Vector3(x + unitSpacing, y - unitSpacing, 0f);
                positions[15] = new Vector3(x + unitSpacing * 2f, y - unitSpacing, 0f);
                positions[16] = new Vector3(x + unitSpacing * 3f, y - unitSpacing, 0f);
                positions[17] = new Vector3(x + unitSpacing * 4f, y - unitSpacing, 0f);
                positions[18] = new Vector3(x - unitSpacing * 5f, y - unitSpacing, 0f);
                positions[19] = new Vector3(x - unitSpacing * 6f, y - unitSpacing * 2f, 0f);
                positions[20] = new Vector3(x - unitSpacing * 7f, y - unitSpacing * 3f, 0f);
                positions[21] = new Vector3(x + unitSpacing * 7f, y - unitSpacing * 3f, 0f);
                positions[22] = new Vector3(x + unitSpacing * 6f, y - unitSpacing * 2f, 0f);
                positions[23] = new Vector3(x + unitSpacing * 5f, y - unitSpacing, 0f);
                break;
            case 'G':
                positions[0] = new Vector3(x + unitSpacing * 4f, y + unitSpacing * 3f, 0f);
                positions[1] = new Vector3(x + unitSpacing * 3f, y + unitSpacing * 4f, 0f);
                positions[2] = new Vector3(x + unitSpacing * 2f, y + unitSpacing * 4f, 0f);
                positions[3] = new Vector3(x + unitSpacing, y + unitSpacing * 4f, 0f);
                positions[4] = new Vector3(x, y + unitSpacing * 4f, 0f);
                positions[5] = new Vector3(x - unitSpacing, y + unitSpacing * 4f, 0f);
                positions[6] = new Vector3(x - unitSpacing * 2f, y + unitSpacing * 4f, 0f);
                positions[7] = new Vector3(x - unitSpacing * 3f, y + unitSpacing * 3f, 0f);
                positions[8] = new Vector3(x - unitSpacing * 4f, y + unitSpacing * 2f, 0f);
                positions[9] = new Vector3(x - unitSpacing * 4f, y + unitSpacing, 0f);
                positions[10] = new Vector3(x - unitSpacing * 4f, y, 0f);
                positions[11] = new Vector3(x - unitSpacing * 4f, y - unitSpacing, 0f);
                positions[12] = new Vector3(x - unitSpacing * 4f, y - unitSpacing * 2f, 0f);
                positions[13] = new Vector3(x - unitSpacing * 3f, y - unitSpacing * 3f, 0f);
                positions[14] = new Vector3(x - unitSpacing * 2f, y - unitSpacing * 4f, 0f);
                positions[15] = new Vector3(x - unitSpacing, y - unitSpacing * 4f, 0f);
                positions[16] = new Vector3(x, y - unitSpacing * 4f, 0f);
                positions[17] = new Vector3(x + unitSpacing, y - unitSpacing * 4f, 0f);
                positions[18] = new Vector3(x + unitSpacing * 2f, y - unitSpacing * 4f, 0f);
                positions[19] = new Vector3(x + unitSpacing * 3f, y - unitSpacing * 3f, 0f);
                positions[20] = new Vector3(x + unitSpacing * 4f, y - unitSpacing * 2f, 0f);
                positions[21] = new Vector3(x + unitSpacing * 3f, y - unitSpacing, 0f);
                positions[22] = new Vector3(x + unitSpacing * 2f, y, 0f);
                positions[23] = new Vector3(x + unitSpacing, y, 0f);
                positions[24] = new Vector3(x, y, 0f);
                break;
            default:
                // Handle unknown characters
                positions[0] = new Vector3(x, y, 0f);
                break;
        }

        return positions;
    }

    private Vector3 GetPositionRelativeToFlag(float x, float y)
    {
        if (flag != null)
        {
            // Calculate the position relative to the flag's position
            Vector3 flagPosition = flag.transform.position;
            Vector3 relativePosition = flagPosition + new Vector3(x, 0f, y);
            return relativePosition;
        }
        else
        {
            // If the flag is not found, return the origin as a fallback
            Debug.LogWarning("Flag not found. Returning origin as position.");
            return Vector3.zero;
        }
    }
    private int GetFlagDirection(Vector3 unitPosition)
    {
        if (flag != null)
        {
            // Get the x-coordinate of the flag
            float flagX = flag.transform.position.x;

            // Get the x-coordinate of the unit
            float unitX = unitPosition.x;

            // Compare the x-coordinates and return 1 if unit is on the right, -1 if on the left
            return -Mathf.RoundToInt(Mathf.Sign(unitX - flagX));
        }
        else
        {
            // If the flag is not found, return 0 as a fallback
            Debug.LogWarning("Flag not found. Returning 0 as direction.");
            return 0;
        }
    }


    private Vector3 CalculateFormationCenter()
    {
        Vector3 center = Vector3.zero;
        foreach (GameObject unit in selectedUnits)
        {
            center += unit.transform.position;
        }
        center /= selectedUnits.Count;
        return center;
    }


}