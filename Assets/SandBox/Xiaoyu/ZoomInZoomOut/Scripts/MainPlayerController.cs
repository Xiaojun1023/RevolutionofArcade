using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class MainPlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Transform cameraTransform;

    CharacterController characterController;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        if (cameraTransform == null && Camera.main != null) 
            cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        if (GameInput.Instance == null) 
            return;

        Vector2 input = GameInput.Instance.ReadMainMove();

        Vector3 forward = cameraTransform ? cameraTransform.forward : Vector3.forward;
        Vector3 right = cameraTransform ? cameraTransform.right : Vector3.right;

        forward.y = 0; 
        right.y = 0;
        forward.Normalize(); 
        right.Normalize();

        Vector3 move = (forward * input.y + right * input.x);
        if (move.sqrMagnitude > 1f) 
            move.Normalize();
        move *= moveSpeed;

        characterController.Move(move * Time.deltaTime);
    }
}
