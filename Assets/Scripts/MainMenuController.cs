using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class MainMenuController : MonoBehaviour
{
    private const string GameplaySceneName = "SampleScene";

    private GUIStyle titleStyle;
    private GUIStyle subtitleStyle;
    private GUIStyle buttonStyle;
    private GUIStyleState buttonNormalStyle;
    private GUIStyleState buttonHoverStyle;
    private GUIStyleState buttonActiveStyle;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnGUI()
    {
        float scale = Mathf.Min(Screen.width / 960f, Screen.height / 540f);
        EnsureStyles(scale);

        GUI.color = new Color(0.035f, 0.025f, 0.07f, 1f);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        float panelWidth = Mathf.Min(640f * scale, Screen.width - 40f * scale);
        float panelHeight = 390f * scale;
        Rect panel = new Rect(
            (Screen.width - panelWidth) * 0.5f,
            (Screen.height - panelHeight) * 0.5f,
            panelWidth,
            panelHeight);

        GUI.color = new Color(0.08f, 0.055f, 0.13f, 0.96f);
        GUI.DrawTexture(panel, Texture2D.whiteTexture);
        GUI.color = Color.white;

        float contentWidth = Mathf.Min(400f * scale, panel.width - 36f * scale);
        float contentX = panel.center.x - contentWidth * 0.5f;
        GUI.Label(
            new Rect(contentX, panel.y + 48f * scale, contentWidth, 80f * scale),
            "GAMBLING WIZARD",
            titleStyle);
        GUI.Label(
            new Rect(contentX, panel.y + 126f * scale, contentWidth, 34f * scale),
            "A hand of magic. A game of chance.",
            subtitleStyle);

        float buttonHeight = 64f * scale;
        float buttonGap = 18f * scale;
        float buttonWidth = Mathf.Min(320f * scale, contentWidth);
        float buttonX = panel.center.x - buttonWidth * 0.5f;
        float playY = panel.y + 210f * scale;

        if (GUI.Button(
            new Rect(buttonX, playY, buttonWidth, buttonHeight),
            "PLAY",
            buttonStyle))
        {
            SceneManager.LoadScene(GameplaySceneName);
        }

        if (GUI.Button(
            new Rect(buttonX, playY + buttonHeight + buttonGap, buttonWidth, buttonHeight),
            "QUIT",
            buttonStyle))
        {
            Application.Quit();
        }
    }

    private void EnsureStyles(float scale)
    {
        if (titleStyle == null)
        {
            titleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 0.78f, 0.32f, 1f) }
            };
            subtitleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.82f, 0.78f, 0.91f, 1f) }
            };
            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                border = new RectOffset(8, 8, 8, 8)
            };

            buttonNormalStyle = new GUIStyleState
            {
                textColor = new Color(1f, 0.85f, 0.56f, 1f),
                background = CreateColorTexture(new Color(0.27f, 0.12f, 0.42f, 1f))
            };
            buttonHoverStyle = new GUIStyleState
            {
                textColor = Color.white,
                background = CreateColorTexture(new Color(0.42f, 0.2f, 0.58f, 1f))
            };
            buttonActiveStyle = new GUIStyleState
            {
                textColor = Color.white,
                background = CreateColorTexture(new Color(0.62f, 0.32f, 0.2f, 1f))
            };

            buttonStyle.normal = buttonNormalStyle;
            buttonStyle.hover = buttonHoverStyle;
            buttonStyle.active = buttonActiveStyle;
        }

        titleStyle.fontSize = Mathf.Max(22, Mathf.RoundToInt(42f * scale));
        subtitleStyle.fontSize = Mathf.Max(12, Mathf.RoundToInt(18f * scale));
        buttonStyle.fontSize = Mathf.Max(16, Mathf.RoundToInt(24f * scale));
    }

    private static Texture2D CreateColorTexture(Color color)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }
}
