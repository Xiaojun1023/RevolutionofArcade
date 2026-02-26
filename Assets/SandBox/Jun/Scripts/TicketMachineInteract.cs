using UnityEngine;

public class TicketMachineInteract : MonoBehaviour
{
    public int coinsPerPress = 1;
    public KeyCode interactKey = KeyCode.E;
    public SimplePromptUI promptUI;

    [Range(0f, 1f)]
    public float faceDotThreshold = 0.7f;

    private CoinWallet walletInRange;
    private Transform playerTransform;
    private bool isPromptVisible;

    void Start()
    {
        if (promptUI != null)
            promptUI.Hide();
    }

    void Update()
    {
        if (walletInRange == null || playerTransform == null)
        {
            SetPrompt(false);
            return;
        }

        bool facing = IsPlayerFacingMachine();

        SetPrompt(facing);

        if (facing && Input.GetKeyDown(interactKey))
        {
            walletInRange.AddCoins(coinsPerPress);

            if (promptUI != null)
                promptUI.Show("Coin purchase successful");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        walletInRange = other.GetComponent<CoinWallet>();
        playerTransform = other.transform;
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        walletInRange = null;
        playerTransform = null;
        SetPrompt(false);
    }

    private bool IsPlayerFacingMachine()
    {
        Vector3 toMachine = (transform.position - playerTransform.position).normalized;
        float dot = Vector3.Dot(playerTransform.forward, toMachine);
        return dot >= faceDotThreshold;
    }

    private void SetPrompt(bool shouldShow)
    {
        if (promptUI == null) return;

        if (shouldShow && !isPromptVisible)
        {
            promptUI.Show("Press E to buy machine coins");
            isPromptVisible = true;
        }
        else if (!shouldShow && isPromptVisible)
        {
            promptUI.Hide();
            isPromptVisible = false;
        }
    }
}