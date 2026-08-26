using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace WtcPractice;

public static class PracticeHud
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

    private static Texture2D backdrop;
    private static GUIStyle backdropStyle;
    private static GUIStyle label;
    private static bool visible = true;
    private static int failures;
    private static bool disabled;
    private static string status;
    private static float statusUntil;

    public static void Toggle()
    {
        visible = !visible;
    }

    public static void Say(string text)
    {
        status = text;
        statusUntil = Time.unscaledTime + StatusSeconds;
    }

    public static void ClearStatus()
    {
        status = null;
    }

    public static void Draw(bool canPractice)
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
            PracticeCore.Log.Error($"[hud] draw failed ({failures}/{MaxFailures}): {e}");

            if (failures >= MaxFailures)
            {
                disabled = true;
                PracticeCore.Log.Error("[hud] switched off after repeated failures, the keys still work");
            }
        }
    }

    private static void DrawPanel()
    {
        float y = Screen.height * PanelY;
        bool gamepad = GameInput.UsingGamepad();
        string save = gamepad ? PracticeMode.SaveButton : Name(PracticeMode.SaveKey);
        string restore = gamepad ? PracticeMode.RestoreButton : Name(PracticeMode.RestoreKey);

        DrawBackdrop(y);

        y = Row(y, save, "CHECKPOINT");
        y = Row(y, restore, "TELEPORT");

        DrawStatus(y + LineHeight * 0.5f);
    }

    private static void DrawBackdrop(float top)
    {
        EnsureBackdrop();

        float x = PanelX - Padding;
        float y = top - Padding;
        float width = BoxWidth + Padding * 2f;
        float height = ContentHeight + Padding * 2f;

        GUI.Label(new Rect(x, y, width, height), string.Empty, backdropStyle);
    }

    private static void EnsureBackdrop()
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

    private static void DrawStatus(float y)
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

    private static float Row(float y, string button, string verb)
    {
        GUI.Label(new Rect(PanelX, y, PanelWidth, LineHeight), $"{button}. {verb}", label);
        return y + LineHeight;
    }

    private static string Name(Key key)
    {
        string raw = key.ToString();
        return raw.StartsWith("Digit", StringComparison.Ordinal) ? raw.Substring(5) : raw;
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

    private static void EnsureStyle()
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
