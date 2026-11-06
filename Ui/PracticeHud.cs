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
    private const float ContentHeight = LineHeight * 3.5f;
    private const float BoxWidth = 130f * Scale;

    private const int FallbackFontSize = 12;
    private const int MaxFailures = 3;

    private static readonly Color StatusColor = new(0.655f, 0.910f, 0.741f, 1f);
    private static readonly Color BackdropColor = new(0f, 0f, 0f, 0.55f);

    private readonly MelonLogger.Instance logger;
    private readonly PracticeControls controls;

    private Texture2D backdrop;
    private GUIStyle backdropStyle;
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

    public void Draw(bool canPractice)
    {
        if (disabled || !visible || !canPractice)
            return;

        try
        {
            EnsureStyle();
            DrawPanel();
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

    private void DrawPanel()
    {
        float y = Screen.height * PanelY;

        DrawBackdrop(y);

        y = Row(y, controls.SaveLabel, "CHECKPOINT");
        y = Row(y, controls.RestoreLabel, "TELEPORT");
        y = Row(y, controls.FreezeLabel, "FREEZE");

        DrawStatus(y + LineHeight * 0.5f);
    }

    private void DrawBackdrop(float top)
    {
        EnsureBackdrop();

        float x = PanelX - Padding;
        float y = top - Padding;
        float width = BoxWidth + Padding * 2f;
        float height = ContentHeight + Padding * 2f;

        GUI.Label(new Rect(x, y, width, height), string.Empty, backdropStyle);
    }

    private void EnsureBackdrop()
    {
        if (backdropStyle != null && backdrop != null)
            return;

        backdrop = new Texture2D(1, 1, TextureFormat.RGBA32, false)
        {
            hideFlags = HideFlags.HideAndDontSave
        };

        backdrop.SetPixel(0, 0, BackdropColor);
        backdrop.Apply();

        backdropStyle = new GUIStyle();
        backdropStyle.normal.background = backdrop;
    }

    private void DrawStatus(float y)
    {
        if (string.IsNullOrEmpty(status))
            return;

        if (Time.unscaledTime > statusUntil)
        {
            status = null;
            return;
        }

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
