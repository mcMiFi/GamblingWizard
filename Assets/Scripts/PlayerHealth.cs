using System;
using UnityEngine;

public sealed class PlayerHealth : MonoBehaviour
{
    [Min(1f)] [SerializeField] private float maxHealth = 100f;
    [Min(0f)] [SerializeField] private float startingHealth = 100f;

    public float CurrentHealth { get; private set; }
    public float MaxHealth => maxHealth;
    public float NormalizedHealth => maxHealth <= 0f ? 0f : CurrentHealth / maxHealth;
    public bool IsDead => CurrentHealth <= 0f;

    public event Action<float, float> HealthChanged;
    public event Action Died;

    private void Awake()
    {
        maxHealth = Mathf.Max(1f, maxHealth);
        CurrentHealth = Mathf.Clamp(startingHealth, 0f, maxHealth);
    }

    public bool TakeDamage(float amount)
    {
        if (amount <= 0f || IsDead)
            return false;

        float previousHealth = CurrentHealth;
        CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
        if (Mathf.Approximately(previousHealth, CurrentHealth))
            return false;

        HealthChanged?.Invoke(CurrentHealth, maxHealth);
        if (CurrentHealth <= 0f)
            Died?.Invoke();

        return true;
    }

    public bool Heal(float amount)
    {
        if (amount <= 0f || IsDead)
            return false;

        float previousHealth = CurrentHealth;
        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
        if (Mathf.Approximately(previousHealth, CurrentHealth))
            return false;

        HealthChanged?.Invoke(CurrentHealth, maxHealth);
        return true;
    }
}
