using System;
using UnityEngine;
using MelonLoader;

namespace WtcPractice;

public class PracticeHud
{
    private const float Scale = 1.5f;
    private const float LineHeight = 18f * Scale;
    private const float PanelWidth = 220f * Scale;
    private const float PanelX = 14f;
    private const float PanelY = 0.38f;
    private const float StatusSeconds = 2.5f;
    private const float Padding = 10f;
    private const float StatusGap = LineHeight * 0.5f;
    private const float BoxWidth = 130f * Scale;
    private const float OutlineThickness = 2f;

    private const int FallbackFontSize = 12;
    private const int MaxFailures = 3;

    private static readonly Color StatusColor = new(0.655f, 0.910f, 0.741f, 1f);
    private static readonly Color BackdropColor = new(0f, 0f, 0f, 0.55f);
    private static readonly Color ValidRunColor = new(0.30f, 0.85f, 0.45f, 1f);
    private static readonly Color InvalidRunColor = new(0.95f, 0.25f, 0.25f, 1f);

    private readonly MelonLogger.Instance logger;
    private readonly PracticeControls controls;

    private GUIStyle backdropStyle;
    private GUIStyle validOutlineStyle;
    private GUIStyle invalidOutlineStyle;
    private GUIStyle label;
    private bool visible = true;
    private int failures;
    private bool disabled;
    private string status;
    private float statusUntil;

    public PracticeHud(MelonLogger.Instance logger, PracticeControls controls)
    {
        this.logger = logger;
        this.controls = controls;
    }

    public void Toggle()
    {
        visible = !visible;
    }

    public void Say(string text)
    {
        status = text;
        statusUntil = Time.unscaledTime + StatusSeconds;
    }

    public void ClearStatus()
    {
        status = null;
    }

    public void Draw(bool canPractice, bool runValid)
    {
        if (disabled || !visible || !canPractice)
            return;

        try
        {
            EnsureStyle();
            DrawPanel(runValid);
        }
        catch (Exception e)
        {
            failures++;
            logger.Error($"HUD draw failed ({failures}/{MaxFailures}): {e}");

            if (failures >= MaxFailures)
            {
                disabled = true;
                logger.Error("HUD switched off after repeated failures, the keys still work");
            }
        }
    }

    private void DrawPanel(bool runValid)
    {
        (string Button, string Action)[] rows = controls.Rows();
        bool hasStatus = HasStatus();
        float y = Screen.height * PanelY;
        float contentHeight = rows.Length * LineHeight + (hasStatus ? StatusGap + LineHeight : 0f);

        DrawBox(y, contentHeight, runValid);

        foreach ((string button, string action) in rows)
            y = Row(y, button, action);

        if (hasStatus)
            DrawStatus(y + StatusGap);
    }

    private void DrawBox(float top, float contentHeight, bool runValid)
    {
        EnsureBoxStyles();

        float x = PanelX - Padding;
        float y = top - Padding;
        float width = BoxWidth + Padding * 2f;
        float height = contentHeight + Padding * 2f;

        GUI.Label(new Rect(x, y, width, height), string.Empty, backdropStyle);

        GUIStyle outline = runValid ? validOutlineStyle : invalidOutlineStyle;
        GUI.Label(new Rect(x, y, width, OutlineThickness), string.Empty, outline);
        GUI.Label(new Rect(x, y + height - OutlineThickness, width, OutlineThickness), string.Empty, outline);
        GUI.Label(new Rect(x, y, OutlineThickness, height), string.Empty, outline);
        GUI.Label(new Rect(x + width - OutlineThickness, y, OutlineThickness, height), string.Empty, outline);
    }

    private void EnsureBoxStyles()
    {
        if (backdropStyle != null && backdropStyle.normal.background != null)
            return;

        backdropStyle = SolidStyle(BackdropColor);
        validOutlineStyle = SolidStyle(ValidRunColor);
        invalidOutlineStyle = SolidStyle(InvalidRunColor);
    }

    private static GUIStyle SolidStyle(Color color)
    {
        Texture2D texture = new(1, 1, TextureFormat.RGBA32, false)
        {
            hideFlags = HideFlags.HideAndDontSave
        };

        texture.SetPixel(0, 0, color);
        texture.Apply();

        GUIStyle style = new();
        style.normal.background = texture;
        return style;
    }

    private bool HasStatus()
    {
        if (string.IsNullOrEmpty(status))
            return false;

        if (Time.unscaledTime <= statusUntil)
            return true;

        status = null;
        return false;
    }

    private void DrawStatus(float y)
    {
        Color previous = GUI.contentColor;

        GUI.contentColor = StatusColor;
        GUI.Label(new Rect(PanelX, y, PanelWidth, LineHeight), status, label);
        GUI.contentColor = previous;
    }

    private float Row(float y, string button, string verb)
    {
        GUI.Label(new Rect(PanelX, y, PanelWidth, LineHeight), $"{button}. {verb}", label);
        return y + LineHeight;
    }

    private static Font InheritedFont()
    {
        if (GUI.skin == null || GUI.skin.label == null)
            return null;

        return GUI.skin.label.font;
    }

    private static int BaseFontSize(Font font)
    {
        if (font == null || font.fontSize <= 0)
            return FallbackFontSize;

        return font.fontSize;
    }

    private void EnsureStyle()
    {
        if (label != null)
            return;

        Font inherited = InheritedFont();
        int baseSize = BaseFontSize(inherited);

        label = new GUIStyle
        {
            font = inherited,
            fontSize = Mathf.RoundToInt(baseSize * Scale),
            alignment = TextAnchor.MiddleLeft,
            wordWrap = false
        };

        label.normal.textColor = Color.white;
    }
}
