using System;
using UnityEngine;

public class GoldMine
{
    public int maxGold;
    public int currentGold;
    public int foodStored;

    public Vector2Int Position { get; }
    public event Action<GoldMine> OnDepleted;

    public GoldMine() { 
        maxGold = 10;
        currentGold = maxGold;
        foodStored = 2;
    }
    public GoldMine(int maxGold, Vector2Int position)
    {
        this.maxGold = maxGold;
        this.currentGold = maxGold;
        this.Position = position;
        this.foodStored = 2;
    }
    public int Mine(int amount)
    {
        int mined = Mathf.Min(amount, currentGold);
        currentGold -= mined;
        
        if (currentGold <= 0)
        {          
            //Debug.Log($"Mine at {Position} is depleted.");
            OnDepleted?.Invoke(this);
        }

        //Debug.Log($"Mined {mined} gold from mine at {Position}. Remaining gold: {currentGold}");

        return mined;
    }
    public int RetrieveFood(int amount)
    {
        int retrieved = Mathf.Min(amount, foodStored);
        foodStored -= retrieved;
        Debug.Log("Food remaining: " + foodStored);
        return retrieved;
    }
    public void CallExist()
    {
        Debug.Log("Mine exists");
    }

    public bool isDepleted => currentGold <= 0;
}