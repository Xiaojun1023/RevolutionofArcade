using UnityEngine;

public class ArcadeMachineCoinGate : MonoBehaviour
{
    public int coinCostToPlay = 1;
    public KeyCode interactKey = KeyCode.E;
    public SimplePromptUI promptUI;
    public ArcadeMachineSession session;

    [Range(0f, 1f)]
    public float faceDotThreshold = 0.7f;

    private CoinWallet walletInRange;
    private Transform playerTransform;
    private bool isPromptVisible;
    private string lastPrompt;

    void Start()
    {
        if (promptUI != null) promptUI.Hide();
    }

    void Update()
    {
        if (session != null && session.IsInSession)
        {
            SetPrompt(false, "");
            return;
        }

        if (walletInRange == null || playerTransform == null)
        {
            SetPrompt(false, "");
            return;
        }

        if (!IsPlayerFacingMachine())
        {
            SetPrompt(false, "");
            return;
        }

        string msg = walletInRange.Coins >= coinCostToPlay ? "Press E to insert coin" : "No Coins";
        SetPrompt(true, msg);

        if (Input.GetKeyDown(interactKey))
        {
            if (walletInRange.TrySpendCoins(coinCostToPlay))
            {
                SetPrompt(false, "");
                if (promptUI != null) promptUI.Show("Starting Game");
                if (session != null) session.TryStartSession();
            }
            else
            {
                if (promptUI != null) promptUI.Show("No Coins");
            }
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
        SetPrompt(false, "");
    }

    private bool IsPlayerFacingMachine()
    {
        Vector3 toMachine = (transform.position - playerTransform.position).normalized;
        float dot = Vector3.Dot(playerTransform.forward, toMachine);
        return dot >= faceDotThreshold;
    }

    private void SetPrompt(bool show, string msg)
    {
        if (promptUI == null) return;

        if (!show)
        {
            if (isPromptVisible)
            {
                promptUI.Hide();
                isPromptVisible = false;
                lastPrompt = "";
            }
            return;
        }

        if (!isPromptVisible)
        {
            promptUI.Show(msg);
            isPromptVisible = true;
            lastPrompt = msg;
            return;
        }

        if (msg != lastPrompt)
        {
            promptUI.Show(msg);
            lastPrompt = msg;
        }
    }
}