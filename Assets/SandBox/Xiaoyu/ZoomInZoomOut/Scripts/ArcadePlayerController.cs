using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ArcadePlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;

    CharacterController characterController;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        enabled = false; 
    }

    void Update()
    {
        if (GameInput.Instance == null) 
            return;

        Vector2 input = GameInput.Instance.ReadArcadeMove();

        Vector3 move = new Vector3(input.x, 0f, input.y);
        if (move.sqrMagnitude > 1f) 
            move.Normalize();
        move *= moveSpeed;

        characterController.Move(move * Time.deltaTime);
    }
}
