using UnityEngine;

public class HexCell : MonoBehaviour {

    public HexCoordinates coordinates;
    private Color color;

    private int elevation = int.MinValue;

    public HexGridChunk chunk;

    // RectTransform of the ui label
    public RectTransform uiRect;

    // Rivers
    bool hasIncomingRiver, hasOutgoingRiver;
    HexDirection incomingRiver, outgoingRiver;

    [SerializeField]
    HexCell[] neighbors;

    public static Texture2D noiseSource;

    public Vector3 Position
    {
        get {
            return transform.localPosition;
            }
    }

    public float RiverSurfaceY
    {
        get
        {
            return (elevation + HexMetrics.riverSurfaceElevationOffset) * HexMetrics.elevationStep;
        }
    }
    public Color Color
    {
        get
        {
            return color;
        }
        set
        {
            if (color == value)
            {
                return;
            }
            color = value;
            Refresh();
        }
    }

    public float StreamBedY
    {
        get
        {
            return (elevation + HexMetrics.streamBedElevationOffset) * HexMetrics.elevationStep;
        }
    }

    void RefreshSelfOnly()
    {
        chunk.Refresh();
    }
    void Refresh()
    {
        if (chunk)
        {
            chunk.Refresh();
            for (int i = 0; i < neighbors.Length; i++)
            {
                HexCell neighbor = neighbors[i];
                if (neighbor != null && neighbor.chunk != chunk)
                {
                    neighbor.chunk.Refresh();
                }
            }
        }
    }
    
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
            if(elevation == value)
            {
                return;
            }
            elevation = value;
            Vector3 position = transform.localPosition;
            position.y = value * HexMetrics.elevationStep;
            position.y += (HexMetrics.SampleNoise(position).y * 2f - 1f) * HexMetrics.elevationPerturbStrength;
            transform.localPosition = position;

            // The canvas is rotated therefore the labels must be moved in the -Z direction
            Vector3 uiPosition = uiRect.localPosition;
            uiPosition.z = -position.y;
            uiRect.localPosition = uiPosition;

            // Prevent uphill rivers
            if (hasOutgoingRiver && elevation < GetNeighbor(outgoingRiver).elevation)
            {
                RemoveOutgoingRiver();
            }
            if(hasIncomingRiver && elevation > GetNeighbor(incomingRiver).elevation)
            {
                RemoveIncomingRiver();
            }

            Refresh();
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

    // Rivers
    public bool HasIncomingRiver
    {
        get
        {
            return hasIncomingRiver;
        }
    }
    public bool HasOutgoingRiver
    {
        get
        {
            return hasOutgoingRiver;
        }
    }
    public HexDirection IncomingRiver
    {
        get
        {
            return incomingRiver;
        }
    }
    public HexDirection OutgoingRiver
    {
        get
        {
            return outgoingRiver;
        }
    }
    public bool HasRiver
    {
        get
        {
            return hasIncomingRiver || hasOutgoingRiver;
        }
    }
    public bool HasRiverBeginOrEnd
    {
        get
        {
            return hasIncomingRiver != hasOutgoingRiver;
        }
    }
    public bool HasRiverThroughEdge(HexDirection direction)
    {
        return hasIncomingRiver && incomingRiver == direction ||
               hasOutgoingRiver && outgoingRiver == direction;
    }
    // Remove Rivers
    public void RemoveOutgoingRiver()
    {
        if (!hasOutgoingRiver)
        {
            return;
        }
        hasOutgoingRiver = false;
        RefreshSelfOnly();

        // Remove source of the outgoing river
        HexCell neighbor = GetNeighbor(outgoingRiver);
        neighbor.hasIncomingRiver = false;
        neighbor.RefreshSelfOnly();
    }
    public void RemoveIncomingRiver()
    {
        if (!hasIncomingRiver)
        {
            return;
        }
        hasIncomingRiver = false;
        RefreshSelfOnly();

        HexCell neighbor = GetNeighbor(incomingRiver);
        neighbor.hasOutgoingRiver = false;
        neighbor.RefreshSelfOnly();
    }
    public void RemoveRiver()
    {
        RemoveOutgoingRiver();
        RemoveIncomingRiver();
    }
    // Adding Rivers
    public void SetOutgoingRiver(HexDirection direction)
    {
        if (hasOutgoingRiver && outgoingRiver == direction)
        {
            return;
        }
        HexCell neighbor = GetNeighbor(direction);
        if (!neighbor || elevation < neighbor.elevation)
        {
            return;
        }
        // Clear previous river
        RemoveOutgoingRiver();
        if (hasIncomingRiver && IncomingRiver == direction)
        {
            RemoveIncomingRiver();
        }
        // Set outgoing river
        hasOutgoingRiver = true;
        outgoingRiver = direction;
        RefreshSelfOnly();
        // Set incoming river
        neighbor.RemoveIncomingRiver();
        neighbor.hasIncomingRiver = true;
        neighbor.incomingRiver = direction.Opposite();
        neighbor.RefreshSelfOnly();
    }
}
