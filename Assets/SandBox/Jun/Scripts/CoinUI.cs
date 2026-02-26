using TMPro;
using UnityEngine;

public class CoinUI : MonoBehaviour
{
    public TextMeshProUGUI coinText;
    public CoinWallet playerWallet;

    void OnEnable()
    {
        CoinWalletEvents.OnCoinsChanged += HandleCoinsChanged;

        if (playerWallet != null)
            HandleCoinsChanged(playerWallet.Coins);
    }

    void OnDisable()
    {
        CoinWalletEvents.OnCoinsChanged -= HandleCoinsChanged;
    }

    void HandleCoinsChanged(int coins)
    {
        if (coinText != null)
            coinText.text = coins.ToString("D2");
    }
}