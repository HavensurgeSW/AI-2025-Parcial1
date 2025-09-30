using UnityEngine;

public class Node<Coordinate> : INode, INode<Coordinate>
{
    private Coordinate coordinate;
    private Vector2Int nearestMine;
    private bool hasNearestMine = false;

    public void SetCoordinate(Coordinate coordinate)
    {
        this.coordinate = coordinate;
    }

    public Coordinate GetCoordinate()
    {
        return coordinate;
    }

    public bool IsBlocked()
    {
        return false;
    }
    public void SetNearestMine(Vector2Int mineCoordinate)
    {
        nearestMine = mineCoordinate;
        hasNearestMine = true;
    }
    public Vector2Int GetNearestMine()
    {
        return nearestMine;
    }

    public bool HasNearestMine()
    {
        return hasNearestMine;
    }

    public void ClearNearestMine()
    {
        hasNearestMine = false;
        nearestMine = default;
    }
}