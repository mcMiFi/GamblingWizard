using System.Collections.Generic;
using UnityEngine;

public enum ComboEffectType
{
    KillDash = 0
}

[CreateAssetMenu(fileName = "Combo", menuName = "Gambling Wizard/Cards/Combo Definition")]
public sealed class ComboDefinition : ScriptableObject
{
    [SerializeField] private string id;
    [SerializeField] private string displayName;
    [TextArea] [SerializeField] private string description;
    [SerializeField] private List<CardDefinition> requiredCards = new List<CardDefinition>();
    [SerializeField] private ComboEffectType effectType;
    [Min(0f)] [SerializeField] private float damage = 150f;
    [Min(0.1f)] [SerializeField] private float dashLengthMultiplier = 24f;
    [Min(0.1f)] [SerializeField] private float dashWidthMultiplier = 12f;
    [Min(0.01f)] [SerializeField] private float dashDuration = 0.65f;

    public string Id => id;
    public string DisplayName => displayName;
    public string Description => description;
    public IReadOnlyList<CardDefinition> RequiredCards => requiredCards;
    public ComboEffectType EffectType => effectType;
    public float Damage => damage;
    public float DashLengthMultiplier => dashLengthMultiplier;
    public float DashWidthMultiplier => dashWidthMultiplier;
    public float DashDuration => dashDuration;
}
