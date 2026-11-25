using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public sealed class CardData
{
    [SerializeField] private string id;
    [SerializeField] private string displayName;
    [SerializeField] private Color color;

    public string Id => id;
    public string DisplayName => displayName;
    public Color Color => color;

    public CardData(string id, string displayName, Color color)
    {
        this.id = id;
        this.displayName = displayName;
        this.color = color;
    }
}

public sealed class CardDeck
{
    private readonly List<CardData> cards = new List<CardData>();

    public int RemainingCount => cards.Count;

    public CardDeck(int size)
    {
        for (int i = 0; i < size; i++)
        {
            cards.Add(new CardData(
                $"card_{i + 1}",
                $"Spell {i + 1}",
                Color.HSVToRGB(Mathf.Repeat(i * 0.13f, 1f), 0.55f, 0.95f)));
        }

        Shuffle();
    }

    public CardData Draw()
    {
        if (cards.Count == 0)
            return null;

        int lastIndex = cards.Count - 1;
        CardData card = cards[lastIndex];
        cards.RemoveAt(lastIndex);
        return card;
    }

    private void Shuffle()
    {
        for (int i = cards.Count - 1; i > 0; i--)
        {
            int swapIndex = UnityEngine.Random.Range(0, i + 1);
            CardData card = cards[i];
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

    [Header("Card layout")]
    [SerializeField] private Vector2 cardSize = new Vector2(150f, 210f);
    [SerializeField] [Range(0f, 30f)] private float fanAngle = 18f;
    [SerializeField] private float handBottomOffset = 45f;

    private readonly List<CardData> rightHand = new List<CardData>();
    private CardDeck deck;
    private GUIStyle cardStyle;
    private GUIStyle labelStyle;
    private GUIStyle smallLabelStyle;
    private Texture2D whiteTexture;

    public IReadOnlyList<CardData> RightHand => rightHand;
    public int RemainingCards => deck == null ? 0 : deck.RemainingCount;
    public int MaxRightHandSize => maxRightHandSize;
    public int DeckSize => deckSize;

    public event Action<CardData> CardDrafted;
    public event Action<CardData> CardUtilized;

    private void Awake()
    {
        ResetDeck();
    }

    private void Update()
    {
        Mouse mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            UtilizeAllCards();

        if (mouse != null && mouse.rightButton.wasPressedThisFrame)
            DraftCard();

        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.rKey.wasPressedThisFrame)
            RefillDeck();
    }

    public bool DraftCard()
    {
        if (rightHand.Count >= maxRightHandSize)
            return false;

        CardData card = deck.Draw();
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
        CardData card = rightHand[lastIndex];
        rightHand.RemoveAt(lastIndex);
        CardUtilized?.Invoke(card);
        return true;
    }

    public int UtilizeAllCards()
    {
        if (rightHand.Count == 0)
            return 0;

        int utilizedCount = 0;
        while (rightHand.Count > 0)
        {
            UtilizeCard();
            utilizedCount++;
        }

        return utilizedCount;
    }

    public void ResetDeck()
    {
        deck = new CardDeck(Mathf.Max(1, deckSize));
        rightHand.Clear();
    }

    public void RefillDeck()
    {
        deck = new CardDeck(Mathf.Max(1, deckSize));
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

    private void DrawCard(Rect cardRect, CardData card, int index, float scale)
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
            "READY",
            smallLabelStyle);

        GUI.color = previousColor;
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
