using UnityEngine;

public class GoldMine : MonoBehaviour
{
    public int maxGold = 1000;
    public int currentGold = 1000;
    public int foodStored = 0;

    public int Mine(int amount)
    {
        int mined = Mathf.Min(amount, currentGold);
        currentGold -= mined;
        return mined;
    }

    public bool isDepleted => currentGold <= 0;
}