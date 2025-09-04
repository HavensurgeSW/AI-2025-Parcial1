

public class Townhall 
{
    public int goldStored = 0;

    public int Deposit(int amount)
    {
        goldStored += amount;
        return amount;
    }
}