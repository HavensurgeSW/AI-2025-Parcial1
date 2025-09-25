
using UnityEngine;

public class Townhall 
{
    public int goldStored = 0;

    public Vector2Int Position { get; }

    public Townhall(Vector2Int position)
    {
        Position = position;
    }
    public int Deposit(int amount)
    {
        goldStored += amount;
        return amount;
    }
}