using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMove : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 5f;
    public float acceleration = 12f;
    public float deceleration = 16f;
    public float airControl = 0.5f;

    [Header("Jump / Gravity")]
    public float gravity = -20f;
    public float jumpHeight = 1.8f;
    public float groundedStickForce = -2f;

    private CharacterController controller;
    private Vector3 currentHorizontalVelocity;
    private float yVelocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 inputDir = (transform.right * h + transform.forward * v).normalized;
        Vector3 targetHorizontalVelocity = inputDir * moveSpeed;

        float control = controller.isGrounded ? 1f : airControl;
        float accelRate = inputDir.magnitude > 0.01f ? acceleration : deceleration;
        accelRate *= control;

        currentHorizontalVelocity = Vector3.MoveTowards(
            currentHorizontalVelocity,
            targetHorizontalVelocity,
            accelRate * Time.deltaTime
        );

        if (controller.isGrounded)
        {
            if (yVelocity < 0f)
                yVelocity = groundedStickForce;

            if (Input.GetButtonDown("Jump"))
                yVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        yVelocity += gravity * Time.deltaTime;

        Vector3 finalMove = currentHorizontalVelocity;
        finalMove.y = yVelocity;

        controller.Move(finalMove * Time.deltaTime);
    }
}