using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public sealed class GameplayHUD : MonoBehaviour
{
    private const float ReferenceWidth = 960f;
    private const float ReferenceHeight = 540f;

    [Header("SVG artwork (imported as Texture2D)")]
    [SerializeField] private Texture2D fullCrosshair;
    [SerializeField] private Texture2D crosshairOverride;
    [SerializeField] private Texture2D deckIcon;
    [SerializeField] private Texture2D healthBarFrame;
    [SerializeField] private Texture2D healthBarBackground;
    [SerializeField] private Texture2D healthBarFill;
    [SerializeField] private Texture2D healthBarFillLow;

    [Header("Layout")]
    [SerializeField] private float crosshairSize = 24f;
    [SerializeField] [Range(0f, 1f)] private float lowHealthThreshold = 0.25f;

    private PlayerHealth playerHealth;
    private CardHandController cardHand;
    private GUIStyle healthTextStyle;
    private GUIStyle deckTextStyle;

    public Texture2D CrosshairTexture => crosshairOverride != null
        ? crosshairOverride
        : fullCrosshair;

    private void Awake()
    {
#if UNITY_EDITOR
        ResolveMissingArtwork();
#endif

        playerHealth = GetComponent<PlayerHealth>();
        cardHand = GetComponent<CardHandController>();

        if (playerHealth == null)
            Debug.LogError("GameplayHUD requires a PlayerHealth component.", this);

        if (cardHand == null)
            Debug.LogError("GameplayHUD requires a CardHandController component.", this);

        if (fullCrosshair == null || deckIcon == null || healthBarFrame == null ||
            healthBarBackground == null || healthBarFill == null || healthBarFillLow == null)
        {
            Debug.LogError("GameplayHUD requires the SVG textures assigned in the Inspector.", this);
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!ResolveMissingArtwork())
            return;

        EditorUtility.SetDirty(this);
        if (gameObject.scene.IsValid())
            EditorSceneManager.MarkSceneDirty(gameObject.scene);
    }

    private bool ResolveMissingArtwork()
    {
        bool changed = false;
        changed |= AssignIfMissing(ref fullCrosshair, "Assets/UI/svg/crosshair_full.svg");
        changed |= AssignIfMissing(ref deckIcon, "Assets/UI/svg/icon_card.svg");
        changed |= AssignIfMissing(ref healthBarFrame, "Assets/UI/svg/healthbar_frame.svg");
        changed |= AssignIfMissing(ref healthBarBackground, "Assets/UI/svg/healthbar_bg.svg");
        changed |= AssignIfMissing(ref healthBarFill, "Assets/UI/svg/healthbar_fill.svg");
        changed |= AssignIfMissing(ref healthBarFillLow, "Assets/UI/svg/healthbar_fill_low.svg");
        return changed;
    }

    private static bool AssignIfMissing(ref Texture2D texture, string assetPath)
    {
        if (texture != null)
            return false;

        texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
        return texture != null;
    }
#endif

    private void OnGUI()
    {
        if (playerHealth == null || cardHand == null || fullCrosshair == null ||
            deckIcon == null || healthBarFrame == null || healthBarBackground == null ||
            healthBarFill == null || healthBarFillLow == null)
        {
            return;
        }

        float scale = Mathf.Min(Screen.width / ReferenceWidth, Screen.height / ReferenceHeight);
        EnsureStyles(scale);
        DrawHealthBar(scale);
        DrawDeckCounter(scale);
        DrawCrosshair(scale);
    }

    public void SetCrosshairTexture(Texture2D texture)
    {
        crosshairOverride = texture;
    }

    private void DrawHealthBar(float scale)
    {
        float x = 31f * scale;
        float y = 27f * scale;
        Rect frameRect = new Rect(x, y, 424f * scale, 48f * scale);
        Rect fillRect = new Rect(x + 12f * scale, y + 8f * scale, 400f * scale, 32f * scale);

        GUI.DrawTexture(fillRect, healthBarBackground, ScaleMode.StretchToFill, true);

        fillRect.width *= Mathf.Clamp01(playerHealth.NormalizedHealth);
        Texture2D fillTexture = playerHealth.NormalizedHealth <= lowHealthThreshold
            ? healthBarFillLow
            : healthBarFill;
        if (fillRect.width > 0f)
            GUI.DrawTexture(fillRect, fillTexture, ScaleMode.StretchToFill, true);

        GUI.DrawTexture(frameRect, healthBarFrame, ScaleMode.StretchToFill, true);
        GUI.Label(
            new Rect(x + 12f * scale, y + 51f * scale, 160f * scale, 36f * scale),
            $"{Mathf.RoundToInt(playerHealth.CurrentHealth)}/{Mathf.RoundToInt(playerHealth.MaxHealth)}",
            healthTextStyle);
    }

    private void DrawDeckCounter(float scale)
    {
        float iconWidth = 64f * scale;
        float iconHeight = 88f * scale;
        float x = Screen.width - 31f * scale - iconWidth;
        float y = 27f * scale;

        GUI.DrawTexture(
            new Rect(x, y, iconWidth, iconHeight),
            deckIcon,
            ScaleMode.StretchToFill,
            true);

        GUI.Label(
            new Rect(x - 25f * scale, y + 88f * scale, 114f * scale, 34f * scale),
            $"{cardHand.RemainingCards}/{cardHand.DeckSize}",
            deckTextStyle);
    }

    private void DrawCrosshair(float scale)
    {
        Texture2D texture = CrosshairTexture;
        if (texture == null)
            return;

        float size = crosshairSize * scale;
        GUI.DrawTexture(
            new Rect((Screen.width - size) * 0.5f, (Screen.height - size) * 0.5f, size, size),
            texture,
            ScaleMode.ScaleToFit,
            true);
    }

    private void EnsureStyles(float scale)
    {
        if (healthTextStyle == null)
        {
            healthTextStyle = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = new Color(1f, 0.82f, 0.38f, 1f) }
            };
            deckTextStyle = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(1f, 0.82f, 0.38f, 1f) }
            };
        }

        healthTextStyle.fontSize = Mathf.Max(12, Mathf.RoundToInt(22f * scale));
        deckTextStyle.fontSize = Mathf.Max(12, Mathf.RoundToInt(22f * scale));
    }
}
