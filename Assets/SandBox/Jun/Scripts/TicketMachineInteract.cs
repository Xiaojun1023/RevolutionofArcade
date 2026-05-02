using UnityEngine;

public class TicketMachineInteract : MonoBehaviour
{
    public int coinsPerPress = 1;
    public KeyCode interactKey = KeyCode.E;
    public SimplePromptUI promptUI;

    [Range(0f, 1f)]
    public float faceDotThreshold = 0.7f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip purchaseSfx;

    private CoinWallet walletInRange;
    private Transform playerTransform;
    private bool isPromptVisible;

    public float purchaseCooldown = 0.2f;
    private float nextPurchaseTime;

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

        if (facing && Input.GetKeyDown(interactKey) && Time.time >= nextPurchaseTime)
        {
            nextPurchaseTime = Time.time + purchaseCooldown;

            walletInRange.AddCoins(coinsPerPress);

            if (promptUI != null)
                promptUI.Show("PURCHASE SUCCESSFUL");

            PlayPurchaseSfx();
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
            promptUI.Show("PRESS [E] TO BUY COINS");
            isPromptVisible = true;
        }
        else if (!shouldShow && isPromptVisible)
        {
            promptUI.Hide();
            isPromptVisible = false;
        }
    }

    private void PlayPurchaseSfx()
    {
        if (purchaseSfx == null) return;

        if (audioSource != null)
            audioSource.PlayOneShot(purchaseSfx);
        else
            AudioSource.PlayClipAtPoint(purchaseSfx, transform.position);
    }
}