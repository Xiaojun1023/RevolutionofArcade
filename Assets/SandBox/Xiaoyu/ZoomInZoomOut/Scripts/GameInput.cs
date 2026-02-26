using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    [Header("Input Actions Asset")]
    public InputActionAsset actions;

    [Header("Map / Action Names (must match .inputactions)")]
    public string mainMapName = "Main";
    public string arcadeMapName = "Arcade";

    public string mainMoveActionName = "Move";
    public string interactActionName = "Interact";

    public string arcadeMoveActionName = "Move";
    public string exitActionName = "Exit";

    InputActionMap mainMap;
    InputActionMap arcadeMap;

    InputAction mainMove;
    InputAction interact;

    InputAction arcadeMove;
    InputAction exit;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (actions == null)
        {
            Debug.LogError("[GameInput] actions is NULL. Please assign GameInputActions in Inspector.");
            return;
        }

        mainMap = actions.FindActionMap(mainMapName, true);
        arcadeMap = actions.FindActionMap(arcadeMapName, true);

        mainMove = mainMap.FindAction(mainMoveActionName, true);
        interact = mainMap.FindAction(interactActionName, true);

        arcadeMove = arcadeMap.FindAction(arcadeMoveActionName, true);
        exit = arcadeMap.FindAction(exitActionName, true);

        SwitchToMain();
    }

    public void SwitchToMain()
    {
        arcadeMap.Disable();
        mainMap.Enable();
    }

    public void SwitchToArcade()
    {
        mainMap.Disable();
        arcadeMap.Enable();
    }

    public Vector2 ReadMainMove() => mainMove != null ? mainMove.ReadValue<Vector2>() : Vector2.zero;
    public bool MainInteractPressedThisFrame() => interact != null && interact.WasPressedThisFrame();

    public Vector2 ReadArcadeMove() => arcadeMove != null ? arcadeMove.ReadValue<Vector2>() : Vector2.zero;
    public bool ArcadeExitPressedThisFrame() => exit != null && exit.WasPressedThisFrame();
}
