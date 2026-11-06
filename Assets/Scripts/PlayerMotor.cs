using UnityEngine;

public class PlayerMotor : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 playerVelocity;
    public float playerSpeed = 5.0f;
    public float gravity = -9.81f;
    private bool grounded;
    public float jumpHeight = 1.5f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    public void ProcessMove(Vector2 input)
    {
        grounded = controller.isGrounded;
        Vector3 moveDirection = Vector3.zero;
        moveDirection.x = input.x;
        moveDirection.z = input.y;
        playerVelocity.y += gravity * Time.deltaTime;
        controller.Move(transform.TransformDirection(moveDirection) * playerSpeed * Time.deltaTime);
        if (grounded && playerVelocity.y < 0)
            playerVelocity.y = -2f;
        controller.Move(playerVelocity * Time.deltaTime);
    }

    public void Jump()
    {
        if (controller.isGrounded)
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -3.0f * gravity);
    }
}
