using UnityEngine;

public class PlayerMotor : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 playerVelocity;
    public float playerSpeed = 5.0f;
    public float sprintSpeed = 8.0f;
    public float crouchSpeed = 2.5f;
    public float gravity = -9.81f;
    private bool grounded;
    public float jumpHeight = 1.5f;
    public float crouchHeight = 1.0f;
    public float crouchTransitionSpeed = 8.0f;
    private const float StandingHeight = 2.0f;
    private const float StandingCameraHeight = 0.6f;
    private const float CrouchingCameraHeight = 0.1f;
    private Transform cameraTransform;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        cameraTransform = GetComponentInChildren<Camera>()?.transform;
    }

    public void ProcessMove(Vector2 input, bool sprinting, bool crouching)
    {
        grounded = controller.isGrounded;
        UpdateCrouch(crouching);

        Vector3 moveDirection = new Vector3(input.x, 0.0f, input.y);
        moveDirection = Vector3.ClampMagnitude(moveDirection, 1.0f);
        float speed = controller.height < StandingHeight
            ? crouchSpeed
            : sprinting && moveDirection.sqrMagnitude > 0.0f ? sprintSpeed : playerSpeed;
        controller.Move(transform.TransformDirection(moveDirection) * speed * Time.deltaTime);

        if (grounded && playerVelocity.y < 0.0f)
            playerVelocity.y = -2f;
        else
            playerVelocity.y += gravity * Time.deltaTime;

        controller.Move(playerVelocity * Time.deltaTime);

        if (cameraTransform != null)
        {
            float targetHeight = controller.height < StandingHeight
                ? CrouchingCameraHeight
                : StandingCameraHeight;
            Vector3 cameraPosition = cameraTransform.localPosition;
            cameraPosition.y = Mathf.MoveTowards(
                cameraPosition.y,
                targetHeight,
                crouchTransitionSpeed * Time.deltaTime);
            cameraTransform.localPosition = cameraPosition;
        }
    }

    public void Jump()
    {
        if (controller.isGrounded && controller.height >= StandingHeight)
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -3.0f * gravity);
    }

    private void UpdateCrouch(bool crouching)
    {
        if (crouching)
        {
            controller.height = crouchHeight;
            controller.center = Vector3.down * (StandingHeight - crouchHeight) * 0.5f;
        }
        else if (controller.height < StandingHeight && CanStand())
        {
            controller.height = StandingHeight;
            controller.center = Vector3.zero;
        }
    }

    private bool CanStand()
    {
        Vector3 bottom = transform.TransformPoint(controller.center + Vector3.down * (controller.height * 0.5f - controller.radius));
        Vector3 top = bottom + Vector3.up * (StandingHeight - controller.radius * 2.0f);
        bool wasEnabled = controller.enabled;
        controller.enabled = false;
        bool blocked = Physics.CheckCapsule(
            bottom,
            top,
            controller.radius,
            Physics.AllLayers,
            QueryTriggerInteraction.Ignore);
        controller.enabled = wasEnabled;
        return !blocked;
    }
}
