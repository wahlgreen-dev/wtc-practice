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

    private const int FallbackFontSize = 12;
    private const int MaxFailures = 3;

    private static GUIStyle label;
    private static bool visible = true;
    private static int failures;
    private static bool disabled;

    public static void Toggle()
    {
        visible = !visible;
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

        y = Row(y, PracticeMode.SaveKey, "CHECKPOINT");
        Row(y, PracticeMode.RestoreKey, "TELEPORT");
    }

    private static float Row(float y, Key key, string verb)
    {
        GUI.Label(new Rect(PanelX, y, PanelWidth, LineHeight), $"{Name(key)}. {verb}", label);
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
