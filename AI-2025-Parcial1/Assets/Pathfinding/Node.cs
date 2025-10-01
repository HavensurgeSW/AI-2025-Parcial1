using UnityEngine;


namespace KarplusParcial1.Graph.Core
{

    public class Node<Coordinate> : INode, INode<Coordinate>
    {
        private Coordinate coordinate;
        private Vector2Int nearestMine;
        private bool hasNearestMine = false;
        private bool blocked = false;
        private bool road = false;

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
            return blocked;
        }
        public bool IsRoad()
        {
            return road;
        }
        public void SetRoad(bool isRoad)
        {
            road = isRoad;
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
}