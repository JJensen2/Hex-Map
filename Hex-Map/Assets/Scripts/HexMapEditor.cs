using UnityEngine;
using UnityEngine.EventSystems;

public class HexMapEditor : MonoBehaviour {

    public Color[] colors;

    public HexGrid hexGrid;

    private Color activeColor;

    int activeElevation;
    int activeWaterLevel;
    // If a color should be applied to a cell
    bool applyColor;
    
    bool applyElevation = true;
    bool applyWaterLevel = true;
    // Toggles
    enum OptionalToggle
    {
        Ignore, Yes, No
    }
    OptionalToggle riverMode, roadMode;
    // Dragging
    bool isDrag;
    HexDirection dragDirection;
    HexCell previousCell;
    // Features
    int activeUrbanLevel, activeFarmLevel, activePlantLevel;
    bool applyUrbanLevel, applyFarmLevel, applyPlantLevel;

    int brushSize;

    void Awake()
    {
        SelectColor(0);
    }

    void Update()
    {
        if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            HandleInput();
        }
        else
        {
            previousCell = null;
        }
    }

    void HandleInput()
    {
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(inputRay, out hit))
        {
            HexCell currentCell = hexGrid.GetCell(hit.point);
            // Check that we are pointing to a cell and that it is not the previous cell
            if(previousCell && previousCell != currentCell)
            {
                ValidateDrag(currentCell);
            }
            else
            {
                isDrag = false;
            }
            EditCells(currentCell);
            previousCell = currentCell;
        }
        else  // not sure if this should actually be set to null
        {
            previousCell = null;
       }
    }

    void EditCells(HexCell center)
    {
        int centerX = center.coordinates.X;
        int centerZ = center.coordinates.Z;

        // Target cell and below
        for (int r = 0, z = centerZ - brushSize; z <= centerZ; z++, r++)
        {
            for (int x = centerX - r; x <= centerX + brushSize; x++)
            {
                EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
            }
        }
        // Target cell and above
        for (int r = 0, z = centerZ + brushSize; z > centerZ; z--, r++)
        {
            for (int x = centerX - brushSize; x <= centerX + r; x++)
            {
                EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
            }
        }
    }

    void EditCell(HexCell cell)
    {
        if (cell)
        {
            // Color
            if (applyColor)
            {
                cell.Color = activeColor;
            }
            // Elevation
            if (applyElevation)
            {
                cell.Elevation = activeElevation;
            }
            // Water
            if (applyWaterLevel)
            {
                cell.WaterLevel = activeWaterLevel; 
            }
            // Urban/City
            if (applyUrbanLevel)
            {
                cell.UrbanLevel = activeUrbanLevel;
            }
            // Farm
            if (applyFarmLevel)
            {
                cell.FarmLevel = activeFarmLevel;
            }
            // Plant
            if (applyPlantLevel)
            {
                cell.PlantLevel = activePlantLevel;
            }
            // River
            if (riverMode == OptionalToggle.No)
            {
                cell.RemoveRiver();
            }
            if (roadMode == OptionalToggle.No)
            {
                cell.RemoveRoads();
            }
            if (isDrag)
            {
                HexCell otherCell = cell.GetNeighbor(dragDirection.Opposite());
                if (otherCell)
                {
                    if(riverMode == OptionalToggle.Yes)
                    {
                       otherCell.SetOutgoingRiver(dragDirection);
                    }
                   if(roadMode == OptionalToggle.Yes)
                   {
                       otherCell.AddRoad(dragDirection);
                   }
                }
            }
        }
    }

    public void SelectColor (int index)
    {
        applyColor = index >= 0;
        if (applyColor)
        {
            activeColor = colors[index];
        }
    }

    public void SetElevation(float elevation)
    {
        activeElevation = (int)elevation;
    }

    public void SetApplyElevation (bool toggle)
    {
        applyElevation = toggle;
    }

    public void SetBrushSize (float size)
    {
        brushSize = (int)size; 
    }

    public void ShowUI(bool visible)
    {
        hexGrid.ShowUI(visible);
    }

    public void SetRiverMode (int mode)
    {
        riverMode = (OptionalToggle)mode;
    }

    void ValidateDrag(HexCell currentCell)
    {
        for (dragDirection = HexDirection.NE;
             dragDirection <= HexDirection.NW;
             dragDirection++
            )
        {
            if (previousCell.GetNeighbor(dragDirection) == currentCell)
            {
                isDrag = true;
                return;
            }
        }
        isDrag = false;
    }

    public void SetRoadMode(int mode)
    {
        roadMode = (OptionalToggle)mode;
    }

    public void SetApplyWaterLevel (bool toggle)
    {
        applyWaterLevel = toggle;
    }

    public void SetWaterLevel (float level)
    {
        activeWaterLevel = (int)level;
    }

    // Urban/City
    public void SetApplyUrbanLevel(bool toggle)
    {
        applyUrbanLevel = toggle;
    }
    public void SetUrbanLevel(float level)
    {
        activeUrbanLevel = (int)level;
    }

    // Farms
    public void SetApplyFarmLevel(bool toggle)
    {
        applyFarmLevel = toggle;
    }
    public void SetFarmLevel(float level)
    {
        activeFarmLevel = (int)level;
    }

    // Plants
    public void SetApplyPlantLevel(bool toggle)
    {
        applyPlantLevel = toggle;
    }
    public void SetPlantLevel(float level)
    {
        activePlantLevel = (int)level;
    }
}
