using UnityEngine;

namespace KarplusParcial1.RTSElements
{
    public class Townhall
    {
        public int goldStored = 0;

        public Vector2Int Position { get; }

        public Townhall(Vector2Int position)
        {
            Position = position;
        }

        public Townhall()
        {

        }
        public int Deposit(int amount)
        {
            goldStored += amount;
            return amount;
        }
    }
}   