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

    [Header("Prototype visuals")]
    [SerializeField] private bool showControls = true;
    [SerializeField] private bool showLeftHand = true;

    private readonly List<CardData> rightHand = new List<CardData>();
    private CardDeck deck;
    private GUIStyle cardStyle;
    private GUIStyle labelStyle;
    private GUIStyle smallLabelStyle;
    private GUIStyle panelStyle;
    private Texture2D whiteTexture;
    private string statusMessage;

    public IReadOnlyList<CardData> RightHand => rightHand;
    public int RemainingCards => deck == null ? 0 : deck.RemainingCount;
    public int MaxRightHandSize => maxRightHandSize;

    public event Action<CardData> CardDrafted;
    public event Action<CardData> CardUtilized;

    private void Awake()
    {
        ResetDeck();
    }

    private void Update()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null)
            return;

        if (mouse.rightButton.wasPressedThisFrame)
            DraftCard();

        if (mouse.leftButton.wasPressedThisFrame)
            UtilizeCard();
    }

    public bool DraftCard()
    {
        if (rightHand.Count >= maxRightHandSize)
        {
            statusMessage = $"Right hand full ({maxRightHandSize}/{maxRightHandSize})";
            return false;
        }

        CardData card = deck.Draw();
        if (card == null)
        {
            statusMessage = "The deck is empty";
            return false;
        }

        rightHand.Add(card);
        statusMessage = $"Drafted {card.DisplayName}";
        CardDrafted?.Invoke(card);
        return true;
    }

    public bool UtilizeCard()
    {
        if (rightHand.Count == 0)
        {
            statusMessage = "There are no cards in the right hand";
            return false;
        }

        int lastIndex = rightHand.Count - 1;
        CardData card = rightHand[lastIndex];
        rightHand.RemoveAt(lastIndex);
        statusMessage = $"Utilized {card.DisplayName} (spell logic not added yet)";
        CardUtilized?.Invoke(card);
        return true;
    }

    public void ResetDeck()
    {
        deck = new CardDeck(Mathf.Max(1, deckSize));
        rightHand.Clear();
        statusMessage = "Right click to draft a card";
    }

    private void OnGUI()
    {
        EnsureStyles();

        DrawDeckCounter();
        DrawHands();

        if (showControls)
        {
            GUI.Label(
                new Rect(24f, 24f, 420f, 32f),
                "RMB  Draft card     LMB  Utilize card",
                labelStyle);
        }

        GUI.Label(
            new Rect(24f, 58f, 620f, 30f),
            statusMessage,
            smallLabelStyle);
    }

    private void DrawDeckCounter()
    {
        Rect counterRect = new Rect(Screen.width - 250f, 24f, 226f, 76f);
        GUI.Box(counterRect, GUIContent.none, panelStyle);
        GUI.Label(
            new Rect(counterRect.x + 14f, counterRect.y + 10f, 200f, 28f),
            $"DECK  {RemainingCards}/{deckSize}",
            labelStyle);
        GUI.Label(
            new Rect(counterRect.x + 14f, counterRect.y + 40f, 200f, 22f),
            $"RIGHT HAND  {rightHand.Count}/{maxRightHandSize}",
            smallLabelStyle);
    }

    private void DrawHands()
    {
        float bottom = Screen.height - handBottomOffset;
        Rect leftHandRect = new Rect(28f, bottom - 145f, 210f, 120f);
        Rect rightHandRect = new Rect(Screen.width - 300f, bottom - 145f, 270f, 120f);

        if (showLeftHand)
        {
            GUI.Box(leftHandRect, GUIContent.none, panelStyle);
            GUI.Label(
                new Rect(leftHandRect.x + 16f, leftHandRect.y + 14f, leftHandRect.width - 32f, 25f),
                "LEFT HAND",
                smallLabelStyle);
            GUI.Label(
                new Rect(leftHandRect.x + 16f, leftHandRect.y + 48f, leftHandRect.width - 32f, 48f),
                "EMPTY",
                labelStyle);
        }

        GUI.Box(rightHandRect, GUIContent.none, panelStyle);
        GUI.Label(
            new Rect(rightHandRect.x + 16f, rightHandRect.y + 14f, rightHandRect.width - 32f, 25f),
            "RIGHT HAND",
            smallLabelStyle);

        DrawRightHandCards(bottom);
    }

    private void DrawRightHandCards(float bottom)
    {
        if (rightHand.Count == 0)
        {
            GUI.Label(
                new Rect(Screen.width - 284f, bottom - 96f, 238f, 42f),
                "EMPTY - draft a card",
                smallLabelStyle);
            return;
        }

        float centerX = Screen.width * 0.5f;
        float spacing = Mathf.Min(cardSize.x * 0.72f, 170f);
        float startX = centerX - spacing * (rightHand.Count - 1) * 0.5f;

        for (int i = 0; i < rightHand.Count; i++)
        {
            float normalized = rightHand.Count == 1
                ? 0f
                : (i / (float)(rightHand.Count - 1) - 0.5f) * 2f;
            float angle = normalized * fanAngle;
            Rect cardRect = new Rect(
                startX + i * spacing - cardSize.x * 0.5f,
                bottom - cardSize.y,
                cardSize.x,
                cardSize.y);

            Matrix4x4 previousMatrix = GUI.matrix;
            GUIUtility.RotateAroundPivot(-angle, new Vector2(cardRect.center.x, cardRect.yMax));
            DrawCard(cardRect, rightHand[i], i + 1);
            GUI.matrix = previousMatrix;
        }
    }

    private void DrawCard(Rect cardRect, CardData card, int index)
    {
        Color previousColor = GUI.color;
        GUI.color = card.Color;
        GUI.DrawTexture(cardRect, whiteTexture);
        GUI.color = Color.white;

        Rect innerRect = new Rect(
            cardRect.x + 8f,
            cardRect.y + 8f,
            cardRect.width - 16f,
            cardRect.height - 16f);
        GUI.Box(innerRect, GUIContent.none, cardStyle);
        GUI.Label(
            new Rect(cardRect.x + 18f, cardRect.y + 22f, cardRect.width - 36f, 28f),
            $"CARD {index}",
            smallLabelStyle);
        GUI.Label(
            new Rect(cardRect.x + 18f, cardRect.y + 72f, cardRect.width - 36f, 70f),
            card.DisplayName,
            labelStyle);
        GUI.Label(
            new Rect(cardRect.x + 18f, cardRect.yMax - 38f, cardRect.width - 36f, 24f),
            "READY",
            smallLabelStyle);

        GUI.color = previousColor;
    }

    private void EnsureStyles()
    {
        if (whiteTexture == null)
        {
            whiteTexture = new Texture2D(1, 1);
            whiteTexture.SetPixel(0, 0, Color.white);
            whiteTexture.Apply();
        }

        if (cardStyle != null)
            return;

        cardStyle = new GUIStyle(GUI.skin.box)
        {
            normal = { background = whiteTexture },
            border = new RectOffset(3, 3, 3, 3)
        };
        labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 20,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white }
        };
        smallLabelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white }
        };
        panelStyle = new GUIStyle(GUI.skin.box)
        {
            normal = { background = MakePanelTexture() }
        };
    }

    private Texture2D MakePanelTexture()
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, new Color(0.03f, 0.02f, 0.07f, 0.82f));
        texture.Apply();
        return texture;
    }
}
