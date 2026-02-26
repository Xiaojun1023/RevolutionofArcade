using System;

public static class CoinWalletEvents
{
    public static event Action<int> OnCoinsChanged;

    public static void RaiseChanged(int coins)
    {
        if (OnCoinsChanged != null)
            OnCoinsChanged.Invoke(coins);
    }
}