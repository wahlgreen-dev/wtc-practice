using System;
using System.Collections.Generic;
using UnityEngine;
using MelonLoader;

namespace WtcPractice;

public class PracticeHud
{
    private const float Scale = 2f;
    private const float TextScale = 1.7f;
    private const float LineHeight = 18f * Scale;
    private const float PanelWidth = 220f * Scale;
    private const float PanelX = 14f;
    private const float PanelY = 0.38f;
    private const float StatusSeconds = 2.5f;
    private const float Padding = 10f;
    private const float RightPadding = 24f;
    private const float StatusGap = LineHeight * 0.5f;
    private const float OutlineThickness = 2f;
    private const float IconGap = 8f;
    private const float EstimatedCharacterWidth = 0.6f;
    private const string WidestUsualStatus = "Checkpoint 22/22";

    private const int FallbackFontSize = 12;
    private const int MaxFailures = 3;

    private static readonly Color BackdropColor = new(0f, 0f, 0f, 0.55f);
    private static readonly Color ValidRunColor = new(0.30f, 0.85f, 0.45f, 1f);
    private static readonly Color InvalidRunColor = new(0.95f, 0.25f, 0.25f, 1f);
    private static readonly Color ClearProgressColor = new(1f, 1f, 1f, 0.3f);

    private readonly MelonLogger.Instance logger;
    private readonly PracticeControls controls;
    private readonly PromptIcons icons;
    private readonly Dictionary<string, float> textWidths = new();

    private GUIStyle backdropStyle;
    private GUIStyle validOutlineStyle;
    private GUIStyle invalidOutlineStyle;
    private GUIStyle clearProgressStyle;
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
        icons = new PromptIcons();
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

    public void Draw(bool canPractice, bool runValid, float clearProgress)
    {
        if (disabled || !visible || !canPractice)
            return;

        try
        {
            EnsureStyle();
            DrawPanel(runValid, clearProgress);
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

    private void DrawPanel(bool runValid, float clearProgress)
    {
        (string Icon, string Action)[] rows = controls.Rows();
        bool hasStatus = HasStatus();
        float y = Screen.height * PanelY;
        float contentHeight = rows.Length * LineHeight + (hasStatus ? StatusGap + LineHeight : 0f);

        float contentWidth = Mathf.Max(TextWidth(WidestUsualStatus), hasStatus ? TextWidth(status) : 0f);
        foreach ((string _, string action) in rows)
            contentWidth = Mathf.Max(contentWidth, LineHeight + IconGap + TextWidth(action));

        DrawBox(y, contentWidth, contentHeight, runValid);

        if (clearProgress > 0f)
            GUI.Label(new Rect(PanelX, y + (rows.Length - 1) * LineHeight, contentWidth * clearProgress, LineHeight), string.Empty, clearProgressStyle);

        foreach ((string icon, string action) in rows)
            y = Row(y, icon, action);

        if (hasStatus)
            DrawStatus(y + StatusGap);
    }

    private float TextWidth(string text)
    {
        if (textWidths.TryGetValue(text, out float width))
            return width;

        width = MeasureText(text);
        textWidths[text] = width;
        return width;
    }

    private float MeasureText(string text)
    {
        try
        {
            return label.CalcSize(new GUIContent(text)).x;
        }
        catch (Exception)
        {
            return text.Length * label.fontSize * EstimatedCharacterWidth;
        }
    }

    private void DrawBox(float top, float contentWidth, float contentHeight, bool runValid)
    {
        EnsureBoxStyles();

        float x = PanelX - Padding;
        float y = top - Padding;
        float width = contentWidth + Padding + RightPadding;
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
        clearProgressStyle = SolidStyle(ClearProgressColor);
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
        GUI.Label(new Rect(PanelX, y, PanelWidth, LineHeight), status, label);
    }

    private float Row(float y, string icon, string action)
    {
        GUI.Label(new Rect(PanelX, y, LineHeight, LineHeight), string.Empty, icons.Get(icon));
        GUI.Label(new Rect(PanelX + LineHeight + IconGap, y, PanelWidth, LineHeight), action, label);
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
            fontSize = Mathf.RoundToInt(baseSize * TextScale),
            alignment = TextAnchor.MiddleLeft,
            wordWrap = false
        };

        label.normal.textColor = Color.white;
    }
}
