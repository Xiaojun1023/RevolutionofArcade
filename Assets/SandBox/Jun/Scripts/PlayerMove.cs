using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMove : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float gravity = -9.8f;
    public float jumpHeight = 2f;

    private CharacterController controller;
    private float yVelocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        float h = Input.GetAxis("Horizontal"); // A/D
        float v = Input.GetAxis("Vertical");   // W/S

        Vector3 move = transform.right * h + transform.forward * v;

        if (controller.isGrounded && yVelocity < 0f)
        {
            yVelocity = -2f;
        }

        if (controller.isGrounded && Input.GetButtonDown("Jump"))
        {
            yVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        yVelocity += gravity * Time.deltaTime;
        move.y = yVelocity;

        controller.Move(move * moveSpeed * Time.deltaTime);
    }
}