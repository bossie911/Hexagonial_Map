using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum HexDirection
{
    NE, E, SE, SW, W, NW
}

public class HexCell : MonoBehaviour
{
    public HexCell[] neighbors;

    public HexCell GetNeighbor(HexDirection direction)
    {
        return neighbors[(int)direction];
    }

    public void SetNeighbor(HexDirection direction, HexCell cell)
    {
        neighbors[(int)direction] = cell;
    }
}
