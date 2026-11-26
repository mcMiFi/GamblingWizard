using UnityEngine;

public enum CardSpellType
{
    Fireball = 0,
    Regeneration = 1,
    ChainLightning = 2,
    KillDash = 3
}

public enum CardSpellCategory
{
    Attack = 0,
    Heal = 1,
    Movement = 2,
    Special = 3
}

[CreateAssetMenu(fileName = "Card", menuName = "Gambling Wizard/Cards/Card Definition")]
public sealed class CardDefinition : ScriptableObject
{
    [SerializeField] private string id;
    [SerializeField] private string displayName;
    [TextArea] [SerializeField] private string description;
    [SerializeField] private CardSpellType spellType;
    [SerializeField] private CardSpellCategory category;
    [SerializeField] private Color color = Color.white;
    [Min(0f)] [SerializeField] private float damage = 25f;
    [Min(0f)] [SerializeField] private float healingAmount = 15f;
    [Min(0.1f)] [SerializeField] private float projectileSpeed = 18f;
    [Min(0.01f)] [SerializeField] private float projectileRadius = 0.2f;
    [Min(0.1f)] [SerializeField] private float projectileLifetime = 5f;
    [Range(0, 4)] [SerializeField] private int maxChainTargets = 4;
    [Min(0.1f)] [SerializeField] private float chainRange = 4f;
    [Min(0f)] [SerializeField] private float chainDelay = 0.2f;
    [Min(0.01f)] [SerializeField] private float chainVfxDuration = 0.25f;
    [Min(0.1f)] [SerializeField] private float dashLengthMultiplier = 6f;
    [Min(0.1f)] [SerializeField] private float dashWidthMultiplier = 4f;
    [Min(0.01f)] [SerializeField] private float dashDuration = 0.2f;

    public string Id => id;
    public string DisplayName => displayName;
    public string Description => description;
    public CardSpellType SpellType => spellType;
    public CardSpellCategory Category => category;
    public Color Color => color;
    public float Damage => damage;
    public float HealingAmount => healingAmount;
    public float ProjectileSpeed => projectileSpeed;
    public float ProjectileRadius => projectileRadius;
    public float ProjectileLifetime => projectileLifetime;
    public int MaxChainTargets => Mathf.Clamp(maxChainTargets, 0, 4);
    public float ChainRange => chainRange;
    public float ChainDelay => chainDelay;
    public float ChainVfxDuration => chainVfxDuration;
    public float DashLengthMultiplier => dashLengthMultiplier;
    public float DashWidthMultiplier => dashWidthMultiplier;
    public float DashDuration => dashDuration;
}
