using UnityEngine;

public class HexCell : MonoBehaviour {

    public HexCoordinates coordinates;
    public Color color;

    private int elevation;

    // RectTransform of the ui label
    public RectTransform uiRect;

    [SerializeField]
    HexCell[] neighbors;

    // Return a neighboring cell by direction
    public HexCell GetNeighbor (HexDirection direction)
    {
        return neighbors[(int)direction];
    }
    
    // Set a neighboring cell 
    public void SetNeighbor (HexDirection direction, HexCell cell)
    {
        neighbors[(int)direction] = cell;
        cell.neighbors[(int)direction.Opposite()] = this;
    }

    // Get/Set the elevaton
    public int Elevation
    {
        get
        {
            return elevation;
        }
        set
        {
            elevation = value;
            Vector3 position = transform.localPosition;
            position.y = value * HexMetrics.elevationStep;
            transform.localPosition = position;

            // The canvas is rotated therefore the labels must be moved in the -Z direction
            Vector3 uiPosition = uiRect.localPosition;
            uiPosition.z = elevation * -HexMetrics.elevationStep;
            uiRect.localPosition = uiPosition;
        }
    }

    public HexMetrics.HexEdgeType GetEdgeType(HexDirection direction)
    {
        return HexMetrics.GetEdgeType(elevation, neighbors[(int)direction].elevation);
    }

    // Determine Slope Between Two Cells
    public HexMetrics.HexEdgeType GetEdgeType(HexCell otherCell)
    {
        return HexMetrics.GetEdgeType(elevation, otherCell.elevation);
    }
}
