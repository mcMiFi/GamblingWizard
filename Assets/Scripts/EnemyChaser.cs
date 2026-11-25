using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(EnemyHealth))]
public sealed class EnemyChaser : MonoBehaviour
{
    [SerializeField] private PlayerHealth target;
    [SerializeField] [Min(0.1f)] private float moveSpeed = 3f;
    [SerializeField] [Min(0.1f)] private float gravity = 9.81f;
    [SerializeField] [Min(0.1f)] private float attackRange = 1.1f;
    [SerializeField] [Min(0.1f)] private float damage = 10f;
    [SerializeField] [Min(0.1f)] private float damageCooldown = 1f;

    private static readonly int BaseColorProperty = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorProperty = Shader.PropertyToID("_Color");

    private CharacterController characterController;
    private float verticalVelocity;
    private float nextDamageTime;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.GetComponent<PlayerHealth>();
        }

        if (target == null)
        {
            Debug.LogError("EnemyChaser could not find a PlayerHealth component on the Player.", this);
            enabled = false;
            return;
        }

        ApplyEnemyTint();
    }

    private void Update()
    {
        if (target == null || target.IsDead)
            return;

        Vector3 direction = target.transform.position - transform.position;
        direction.y = 0f;
        float distance = direction.magnitude;
        bool inAttackRange = distance <= attackRange;
        Vector3 horizontalVelocity = Vector3.zero;

        if (!inAttackRange && distance > 0.001f)
        {
            direction /= distance;
            horizontalVelocity = direction * moveSpeed;
            transform.rotation = Quaternion.LookRotation(direction);
        }

        if (characterController.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;
        else
            verticalVelocity -= gravity * Time.deltaTime;

        Vector3 velocity = horizontalVelocity + Vector3.up * verticalVelocity;
        characterController.Move(velocity * Time.deltaTime);

        if (inAttackRange && Time.time >= nextDamageTime)
        {
            target.TakeDamage(damage);
            nextDamageTime = Time.time + damageCooldown;
        }
    }

    private void ApplyEnemyTint()
    {
        Renderer enemyRenderer = GetComponent<Renderer>();
        if (enemyRenderer == null)
            return;

        MaterialPropertyBlock properties = new MaterialPropertyBlock();
        enemyRenderer.GetPropertyBlock(properties);
        Color enemyColor = new Color(0.75f, 0.12f, 0.12f, 1f);
        properties.SetColor(BaseColorProperty, enemyColor);
        properties.SetColor(ColorProperty, enemyColor);
        enemyRenderer.SetPropertyBlock(properties);
    }
}
