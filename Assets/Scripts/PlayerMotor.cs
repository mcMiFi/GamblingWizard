using System.Collections;
using System.Collections.Generic;
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
    private PlayerAudioController playerAudio;
    private bool isDashing;

    private void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        cameraTransform = GetComponentInChildren<Camera>()?.transform;
        playerAudio = GetComponent<PlayerAudioController>();
    }

    public void ProcessMove(Vector2 input, bool sprinting, bool crouching)
    {
        if (isDashing)
            return;

        grounded = controller.isGrounded;
        UpdateCrouch(crouching);

        Vector3 moveDirection = new Vector3(input.x, 0.0f, input.y);
        moveDirection = Vector3.ClampMagnitude(moveDirection, 1.0f);
        Vector3 positionBeforeMove = transform.position;
        float speed = controller.height < StandingHeight
            ? crouchSpeed
            : sprinting && moveDirection.sqrMagnitude > 0.0f ? sprintSpeed : playerSpeed;
        controller.Move(transform.TransformDirection(moveDirection) * speed * Time.deltaTime);

        if (grounded && playerVelocity.y < 0.0f)
            playerVelocity.y = -2f;
        else
            playerVelocity.y += gravity * Time.deltaTime;

        controller.Move(playerVelocity * Time.deltaTime);

        Vector3 actualMovement = transform.position - positionBeforeMove;
        actualMovement.y = 0f;
        bool isMoving = moveDirection.sqrMagnitude > 0f && actualMovement.sqrMagnitude > 0.000001f;
        bool isSprinting = sprinting && controller.height >= StandingHeight &&
            moveDirection.sqrMagnitude > 0f;
        playerAudio?.ProcessMovementAudio(
            isMoving,
            controller.isGrounded,
            isSprinting,
            controller.height < StandingHeight);

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
        if (!isDashing && controller.isGrounded && controller.height >= StandingHeight)
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -3.0f * gravity);
            playerAudio?.PlayJump();
        }
    }

    public bool TryKillDash(
        float damage,
        float lengthMultiplier,
        float widthMultiplier,
        float duration)
    {
        if (isDashing || controller == null || lengthMultiplier <= 0f ||
            widthMultiplier <= 0f || duration <= 0f)
        {
            return false;
        }

        StartCoroutine(KillDash(damage, lengthMultiplier, widthMultiplier, duration));
        return true;
    }

    private IEnumerator KillDash(
        float damage,
        float lengthMultiplier,
        float widthMultiplier,
        float duration)
    {
        isDashing = true;
        Vector3 dashDirection = transform.forward;
        dashDirection.y = 0f;
        dashDirection.Normalize();

        if (dashDirection.sqrMagnitude < 0.001f)
        {
            isDashing = false;
            yield break;
        }

        float bodyWidth = controller.radius * 2f;
        float dashDistance = bodyWidth * lengthMultiplier;
        float damageRadius = bodyWidth * widthMultiplier * 0.5f;
        HashSet<EnemyHealth> damagedEnemies = new HashSet<EnemyHealth>();
        List<CollisionIgnoreState> ignoredEnemyCollisions = IgnoreEnemyCollisions();
        float elapsed = 0f;

        try
        {
            while (elapsed < duration)
            {
                float frameDuration = Mathf.Min(Time.deltaTime, duration - elapsed);
                float distanceThisFrame = dashDistance * frameDuration / duration;
                int stepCount = Mathf.Max(1, Mathf.CeilToInt(distanceThisFrame / 0.2f));
                float stepDuration = frameDuration / stepCount;
                float stepDistance = distanceThisFrame / stepCount;

                for (int step = 0; step < stepCount; step++)
                {
                    Vector3 previousPosition = transform.position;
                    if (controller.isGrounded && playerVelocity.y < 0f)
                        playerVelocity.y = -2f;
                    else
                        playerVelocity.y += gravity * stepDuration;

                    CollisionFlags collisionFlags = controller.Move(
                        dashDirection * stepDistance +
                        Vector3.up * playerVelocity.y * stepDuration);
                    Vector3 currentPosition = transform.position;

                    DamageEnemiesAlongDash(
                        previousPosition,
                        currentPosition,
                        damageRadius,
                        damage,
                        damagedEnemies);

                    float movedDistance = Vector3.Dot(currentPosition - previousPosition, dashDirection);
                    if ((collisionFlags & CollisionFlags.Sides) != 0 &&
                        movedDistance < stepDistance - 0.01f)
                    {
                        yield break;
                    }
                }

                elapsed += frameDuration;
                yield return null;
            }
        }
        finally
        {
            RestoreEnemyCollisions(ignoredEnemyCollisions);
            isDashing = false;
        }
    }

    private List<CollisionIgnoreState> IgnoreEnemyCollisions()
    {
        List<CollisionIgnoreState> ignoredCollisions = new List<CollisionIgnoreState>();
        EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>();

        foreach (EnemyHealth enemy in enemies)
        {
            if (enemy == null)
                continue;

            Collider[] enemyColliders = enemy.GetComponentsInChildren<Collider>();
            foreach (Collider enemyCollider in enemyColliders)
            {
                if (enemyCollider == null || !enemyCollider.enabled ||
                    !enemyCollider.gameObject.activeInHierarchy)
                {
                    continue;
                }

                bool wasIgnored = Physics.GetIgnoreCollision(controller, enemyCollider);
                ignoredCollisions.Add(new CollisionIgnoreState(enemyCollider, wasIgnored));
                if (!wasIgnored)
                    Physics.IgnoreCollision(controller, enemyCollider, true);
            }
        }

        return ignoredCollisions;
    }

    private void RestoreEnemyCollisions(List<CollisionIgnoreState> ignoredCollisions)
    {
        foreach (CollisionIgnoreState state in ignoredCollisions)
        {
            if (state.Collider != null)
                Physics.IgnoreCollision(controller, state.Collider, state.WasIgnored);
        }
    }

    private static void DamageEnemiesAlongDash(
        Vector3 start,
        Vector3 end,
        float radius,
        float damage,
        HashSet<EnemyHealth> damagedEnemies)
    {
        Collider[] overlappingColliders = Physics.OverlapCapsule(
            start,
            end,
            radius,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore);

        foreach (Collider overlappingCollider in overlappingColliders)
        {
            EnemyHealth enemy = overlappingCollider.GetComponentInParent<EnemyHealth>();
            if (enemy != null && damagedEnemies.Add(enemy))
                enemy.TakeDamage(damage);
        }
    }

    private readonly struct CollisionIgnoreState
    {
        public readonly Collider Collider;
        public readonly bool WasIgnored;

        public CollisionIgnoreState(Collider collider, bool wasIgnored)
        {
            Collider = collider;
            WasIgnored = wasIgnored;
        }
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
