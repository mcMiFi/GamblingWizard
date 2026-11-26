using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class CardDeck
{
    private readonly List<CardDefinition> cards = new List<CardDefinition>();

    public int RemainingCount => cards.Count;

    public CardDeck(IReadOnlyList<CardDefinition> cardDefinitions, int size)
    {
        for (int i = 0; i < size; i++)
            cards.Add(cardDefinitions[i % cardDefinitions.Count]);

        Shuffle();
    }

    public CardDefinition Draw()
    {
        if (cards.Count == 0)
            return null;

        int lastIndex = cards.Count - 1;
        CardDefinition card = cards[lastIndex];
        cards.RemoveAt(lastIndex);
        return card;
    }

    private void Shuffle()
    {
        for (int i = cards.Count - 1; i > 0; i--)
        {
            int swapIndex = UnityEngine.Random.Range(0, i + 1);
            CardDefinition card = cards[i];
            cards[i] = cards[swapIndex];
            cards[swapIndex] = card;
        }
    }
}

public sealed class CardHandController : MonoBehaviour
{
    [Header("Rules")]
    [Min(1)] [SerializeField] private int deckSize = 20;
    [Min(1)] [SerializeField] private int maxRightHandSize = 4;
    [SerializeField] private CardDatabase cardDatabase;

    [Header("Card layout")]
    [SerializeField] private Vector2 cardSize = new Vector2(150f, 210f);
    [SerializeField] [Range(0f, 30f)] private float fanAngle = 18f;
    [SerializeField] private float handBottomOffset = 45f;

    private readonly List<CardDefinition> rightHand = new List<CardDefinition>();
    private CardDeck deck;
    private GUIStyle cardStyle;
    private GUIStyle labelStyle;
    private GUIStyle smallLabelStyle;
    private Texture2D whiteTexture;

    public IReadOnlyList<CardDefinition> RightHand => rightHand;
    public int RemainingCards => deck == null ? 0 : deck.RemainingCount;
    public int MaxRightHandSize => maxRightHandSize;
    public int DeckSize => deckSize;
    public CardDatabase Database => cardDatabase;

    public event Action<CardDefinition> CardDrafted;
    public event Action<CardDefinition> CardUtilized;
    public event Action<IReadOnlyList<CardDefinition>> CardsUtilized;
    public event Action<CardDefinition> CardDiscarded;

    private void Awake()
    {
        if (!HasValidCardDatabase())
            return;

        ResetDeck();
    }

    private void Update()
    {
        Mouse mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            UtilizeHand();

        if (mouse != null && mouse.rightButton.wasPressedThisFrame)
            DraftCard();

        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.qKey.wasPressedThisFrame)
                DiscardLastCard();

            if (keyboard.rKey.wasPressedThisFrame)
                RefillDeck();
        }
    }

    public bool DraftCard()
    {
        if (rightHand.Count >= maxRightHandSize)
            return false;

        if (deck == null)
            return false;

        CardDefinition card = deck.Draw();
        if (card == null)
            return false;

        rightHand.Add(card);
        CardDrafted?.Invoke(card);
        return true;
    }

    public bool UtilizeCard()
    {
        if (rightHand.Count == 0)
            return false;

        int lastIndex = rightHand.Count - 1;
        CardDefinition card = rightHand[lastIndex];
        rightHand.RemoveAt(lastIndex);
        CardUtilized?.Invoke(card);
        return true;
    }

    public bool UtilizeHand()
    {
        if (rightHand.Count == 0)
            return false;

        CardDefinition[] cards = rightHand.ToArray();
        rightHand.Clear();
        CardsUtilized?.Invoke(cards);
        return true;
    }

    public bool DiscardLastCard()
    {
        if (rightHand.Count == 0)
            return false;

        int lastIndex = rightHand.Count - 1;
        CardDefinition card = rightHand[lastIndex];
        rightHand.RemoveAt(lastIndex);
        CardDiscarded?.Invoke(card);
        return true;
    }

    public int UtilizeAllCards()
    {
        int utilizedCount = rightHand.Count;
        return UtilizeHand() ? utilizedCount : 0;
    }

    public void ResetDeck()
    {
        if (!HasValidCardDatabase())
            return;

        deck = new CardDeck(cardDatabase.Cards, Mathf.Max(1, deckSize));
        rightHand.Clear();
    }

    public void RefillDeck()
    {
        if (!HasValidCardDatabase())
            return;

        deck = new CardDeck(cardDatabase.Cards, Mathf.Max(1, deckSize));
    }

    private bool HasValidCardDatabase()
    {
        if (cardDatabase == null || cardDatabase.Cards == null || cardDatabase.Cards.Count == 0)
        {
            Debug.LogError("CardHandController requires a CardDatabase with at least one card.", this);
            return false;
        }

        for (int i = 0; i < cardDatabase.Cards.Count; i++)
        {
            if (cardDatabase.Cards[i] == null)
            {
                Debug.LogError($"CardDatabase contains a missing card definition at index {i}.", this);
                return false;
            }
        }

        return true;
    }

    private void OnGUI()
    {
        float scale = Mathf.Min(Screen.width / 960f, Screen.height / 540f);
        EnsureStyles(scale);
        DrawRightHandCards(Screen.height - handBottomOffset * scale, scale);
    }

    private void DrawRightHandCards(float bottom, float scale)
    {
        if (rightHand.Count == 0)
            return;

        float centerX = Screen.width * 0.5f;
        Vector2 scaledCardSize = cardSize * scale;
        float spacing = Mathf.Min(scaledCardSize.x * 0.72f, 170f * scale);
        float startX = centerX - spacing * (rightHand.Count - 1) * 0.5f;

        for (int i = 0; i < rightHand.Count; i++)
        {
            float normalized = rightHand.Count == 1
                ? 0f
                : (i / (float)(rightHand.Count - 1) - 0.5f) * 2f;
            float angle = normalized * fanAngle;
            Rect cardRect = new Rect(
                startX + i * spacing - scaledCardSize.x * 0.5f,
                bottom - scaledCardSize.y,
                scaledCardSize.x,
                scaledCardSize.y);

            Matrix4x4 previousMatrix = GUI.matrix;
            GUIUtility.RotateAroundPivot(-angle, new Vector2(cardRect.center.x, cardRect.yMax));
            DrawCard(cardRect, rightHand[i], i + 1, scale);
            GUI.matrix = previousMatrix;
        }
    }

    private void DrawCard(Rect cardRect, CardDefinition card, int index, float scale)
    {
        Color previousColor = GUI.color;
        GUI.color = card.Color;
        GUI.DrawTexture(cardRect, whiteTexture);
        GUI.color = Color.white;

        Rect innerRect = new Rect(
            cardRect.x + 8f * scale,
            cardRect.y + 8f * scale,
            cardRect.width - 16f * scale,
            cardRect.height - 16f * scale);
        GUI.Box(innerRect, GUIContent.none, cardStyle);
        GUI.Label(
            new Rect(cardRect.x + 18f * scale, cardRect.y + 22f * scale, cardRect.width - 36f * scale, 28f * scale),
            $"CARD {index}",
            smallLabelStyle);
        GUI.Label(
            new Rect(cardRect.x + 18f * scale, cardRect.y + 72f * scale, cardRect.width - 36f * scale, 70f * scale),
            card.DisplayName,
            labelStyle);
        GUI.Label(
            new Rect(cardRect.x + 18f * scale, cardRect.yMax - 38f * scale, cardRect.width - 36f * scale, 24f * scale),
            GetCardEffectLabel(card),
            smallLabelStyle);

        GUI.color = previousColor;
    }

    private static string GetCardEffectLabel(CardDefinition card)
    {
        switch (card.SpellType)
        {
            case CardSpellType.Regeneration:
                return $"HEAL {card.HealingAmount:0} HP";
            case CardSpellType.ChainLightning:
                return $"{card.Damage:0} DMG + {card.MaxChainTargets} JUMPS";
            case CardSpellType.KillDash:
                return $"{card.Damage:0} DMG / DASH";
            default:
                return $"{card.Damage:0} DAMAGE";
        }
    }

    private void EnsureStyles(float scale)
    {
        if (whiteTexture == null)
        {
            whiteTexture = new Texture2D(1, 1);
            whiteTexture.SetPixel(0, 0, Color.white);
            whiteTexture.Apply();
        }

        if (cardStyle == null)
        {
            cardStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = whiteTexture },
                border = new RectOffset(3, 3, 3, 3)
            };
            labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };
            smallLabelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };
        }

        labelStyle.fontSize = Mathf.Max(12, Mathf.RoundToInt(20f * scale));
        smallLabelStyle.fontSize = Mathf.Max(10, Mathf.RoundToInt(14f * scale));
    }
}
