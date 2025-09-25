using UnityEngine;

public class GoldMine
{
    public int maxGold;
    public int currentGold;
    public int foodStored;

    public Vector2Int Position { get; }

    public GoldMine() { 
        maxGold = 1000;
        currentGold = maxGold;
        Debug.Log("GoldMine created with no position. DEFAULT CONSTRUCTOR");
    }
    public GoldMine(int maxGold, Vector2Int position)
    {
        this.maxGold = maxGold;
        this.currentGold = maxGold;
        this.Position = position;
        this.foodStored = 100;
    }
    public int Mine(int amount)
    {
        int mined = Mathf.Min(amount, currentGold);
        currentGold -= mined;
        return mined;
    }
    public int RetrieveFood(int amount)
    {
        int retrieved = Mathf.Min(amount, foodStored);
        foodStored -= retrieved;
        return retrieved;
    }
    public void CallExist()
    {
        Debug.Log("Mine exists");
    }

    public bool isDepleted => currentGold <= 0;
}