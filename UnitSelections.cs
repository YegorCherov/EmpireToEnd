using System.Collections.Generic;
using UnityEngine;

public class UnitSelections : MonoBehaviour
{
    public List<GameObject> unitList = new List<GameObject>();
    public List<GameObject> unitsSelected = new List<GameObject>();

    private static UnitSelections _instance;
    public static UnitSelections Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<UnitSelections>();
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject();
                    _instance = singletonObject.AddComponent<UnitSelections>();
                    singletonObject.name = typeof(UnitSelections).ToString() + " (Singleton)";
                    DontDestroyOnLoad(singletonObject);
                }
            }
            return _instance;
        }
    }


    void Awake()
    {
        //if an instance of this already exists and it isn't this one
        if (_instance != null && _instance != this)
        {
            //we destroy this instance
            Destroy(this.gameObject);
        }
        else
        {
            //make this the instance
            _instance = this;
        }
    }

    public void ClickSelect(GameObject unitToAdd)
    {
        
        DeselectAll();
        unitsSelected.Add(unitToAdd);
        unitToAdd.transform.GetChild(0).gameObject.SetActive(true);
    }

    public void ShiftClickSelect(GameObject unitToAdd)
    {
        
        if (!unitsSelected.Contains(unitToAdd))
        {
            unitsSelected.Add(unitToAdd);
            unitToAdd.transform.GetChild(0).gameObject.SetActive(true);
        }
        else
        {
            unitToAdd.transform.GetChild(0).gameObject.SetActive(false);
            unitsSelected.Remove(unitToAdd);
        }
    }

    public void DragSelect(GameObject unitToAdd)
    {
        if (!unitsSelected.Contains(unitToAdd))
        {
            unitsSelected.Add(unitToAdd);
            unitToAdd.transform.GetChild (0).gameObject.SetActive(true);
        }
    }

    public void DeselectAll()
    {
        foreach (GameObject unitToRemove in unitsSelected)
        {
            unitToRemove.transform.GetChild(0).gameObject.SetActive(false);
        }
        unitsSelected.Clear();
    }

    public void SelectAllUnitsOfType(GameObject unitClicked)
    {
        string unitType = unitClicked.GetComponent<Unit>().GetType().ToString();

        bool wasSelected = unitsSelected.Contains(unitClicked);

        if (!wasSelected)
        {
            DeselectAll();
            DragSelect(unitClicked);
        }

        foreach (GameObject unit in unitList)
        {
            if (unit != unitClicked && unit.GetComponent<Unit>().GetType().ToString() == unitType)
            {
                DragSelect(unit);
            }
        }
    }

    public void Deselect(GameObject unitToDeselect)
    {
        
    }
}