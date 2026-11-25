using UnityEngine;

public sealed class EnemyHealth : MonoBehaviour
{
    [Min(1f)] [SerializeField] private float maxHealth = 100f;

    public float CurrentHealth { get; private set; }
    public float MaxHealth => maxHealth;
    public bool IsDead { get; private set; }

    private void Awake()
    {
        maxHealth = Mathf.Max(1f, maxHealth);
        CurrentHealth = maxHealth;
    }

    public bool TakeDamage(float amount)
    {
        if (amount <= 0f || IsDead)
            return false;

        CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
        if (CurrentHealth <= 0f)
        {
            IsDead = true;
            Destroy(gameObject);
        }

        return true;
    }
}
