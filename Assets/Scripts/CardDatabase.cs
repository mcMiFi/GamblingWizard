using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardDatabase", menuName = "Gambling Wizard/Cards/Card Database")]
public sealed class CardDatabase : ScriptableObject
{
    [SerializeField] private List<CardDefinition> cards = new List<CardDefinition>();

    public IReadOnlyList<CardDefinition> Cards => cards;
}
