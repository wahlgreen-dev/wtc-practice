using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace WtcPractice;

public static class PracticeHud
{
    private const float Scale = 1.5f;
    private const float LineHeight = 18f * Scale;
    private const float KeyColumn = 30f * Scale;
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
        PracticeCore.Log.Msg($"[hud] {(visible ? "shown" : "hidden")}");
    }

    public static void Draw(bool inLevel)
    {
        if (disabled || !visible || !inLevel)
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
                PracticeCore.Log.Error("[hud] switched off after repeated failures — the keys still work");
            }
        }
    }

    private static void DrawPanel()
    {
        float y = Screen.height * PanelY;

        y = Row(y, Name(PracticeMode.SaveKey), "SAVE STATE");
        Row(y, Name(PracticeMode.RestoreKey), "RESTORE");
    }

    private static float Row(float y, string key, string verb)
    {
        Line(PanelX, y, key);
        return Line(PanelX + KeyColumn, y, verb);
    }

    private static float Line(float x, float y, string text)
    {
        GUI.Label(new Rect(x, y, PanelWidth, LineHeight), text, label);
        return y + LineHeight;
    }

    private static string Name(Key key)
    {
        string raw = key.ToString();
        return raw.StartsWith("Digit", StringComparison.Ordinal) ? raw.Substring(5) : raw;
    }

    private static void EnsureStyle()
    {
        if (label != null)
            return;

        Font inherited = GUI.skin != null && GUI.skin.label != null ? GUI.skin.label.font : null;
        int baseSize = inherited != null && inherited.fontSize > 0 ? inherited.fontSize : FallbackFontSize;

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
