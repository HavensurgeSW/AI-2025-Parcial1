public interface INode
{
    public bool IsBlocked();
    public bool IsRoad();
}

public interface INode<Coordinate> 
{
    public void SetCoordinate(Coordinate coordinateType);
    public Coordinate GetCoordinate();
}
