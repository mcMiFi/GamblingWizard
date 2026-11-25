using UnityEngine;

[CreateAssetMenu(fileName = "Card", menuName = "Gambling Wizard/Cards/Card Definition")]
public sealed class CardDefinition : ScriptableObject
{
    [SerializeField] private string id;
    [SerializeField] private string displayName;
    [TextArea] [SerializeField] private string description;
    [SerializeField] private Color color = Color.white;
    [Min(0f)] [SerializeField] private float damage = 25f;
    [Min(0.1f)] [SerializeField] private float projectileSpeed = 18f;
    [Min(0.01f)] [SerializeField] private float projectileRadius = 0.2f;
    [Min(0.1f)] [SerializeField] private float projectileLifetime = 5f;

    public string Id => id;
    public string DisplayName => displayName;
    public string Description => description;
    public Color Color => color;
    public float Damage => damage;
    public float ProjectileSpeed => projectileSpeed;
    public float ProjectileRadius => projectileRadius;
    public float ProjectileLifetime => projectileLifetime;
}
