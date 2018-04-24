using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : IHeapItem<Node>
{
    public bool Walkable;
    public Vector3 WorldPosition;
    public int gCost,hCost;
    public Coordinate Grid;
    public Node Parent;
    public int MovementPenalty;
    public Node(bool walkable, Vector3 worldPosition, Coordinate  grid, int movementPenalty)
    {
        Walkable = walkable;
        WorldPosition = worldPosition;
        Grid = grid;
        MovementPenalty = movementPenalty;
    }

    public int fCost
    {
        get { return gCost + hCost; }
    }

    public int CompareTo(Node nodeToCompare)
    {
        int compare = fCost.CompareTo(nodeToCompare.fCost);
        if (compare == 0)
        {
            compare = hCost.CompareTo(nodeToCompare.hCost);
        }
        return -compare;
    }

    public int HeapIndex { get; set; }
}
