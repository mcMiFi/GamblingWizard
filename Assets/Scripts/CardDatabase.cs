using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardDatabase", menuName = "Gambling Wizard/Cards/Card Database")]
public sealed class CardDatabase : ScriptableObject
{
    [SerializeField] private List<CardDefinition> cards = new List<CardDefinition>();
    [SerializeField] private List<ComboDefinition> combos = new List<ComboDefinition>();

    public IReadOnlyList<CardDefinition> Cards => cards;
    public IReadOnlyList<ComboDefinition> Combos => combos;
}
