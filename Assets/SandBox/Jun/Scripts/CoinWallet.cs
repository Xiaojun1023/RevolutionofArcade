using UnityEngine;

public class CoinWallet : MonoBehaviour
{
    [SerializeField] private int coins = 0;

    public int Coins
    {
        get { return coins; }
    }

    public void AddCoins(int amount)
    {
        if (amount <= 0) return;

        coins += amount;
        CoinWalletEvents.RaiseChanged(coins);
    }

    public bool TrySpendCoins(int amount)
    {
        if (amount <= 0) return true;

        if (coins < amount)
            return false;

        coins -= amount;
        CoinWalletEvents.RaiseChanged(coins);
        return true;
    }
}