using System;
using Il2CppSpeed;
using Il2CppSpeed.Input;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace WtcPractice;

public class PracticeControls
{
    private const Key SaveKey = Key.Digit1;
    private const Key RestoreKey = Key.Digit2;
    private const Key FreezeKey = Key.Digit3;
    private const Key HudKey = Key.F1;

    private const string SaveButton = "LB";
    private const string RestoreButton = "RB";

    private bool lastPressWasGamepad;

    public string SaveLabel => UsingGamepad() ? SaveButton : Name(SaveKey);

    public string RestoreLabel => UsingGamepad() ? RestoreButton : Name(RestoreKey);

    public string FreezeLabel => Name(FreezeKey);

    public bool SavePressed() => Pressed(SaveKey) || Pressed(Gamepad.current?.leftShoulder);

    public bool RestorePressed() => Pressed(RestoreKey) || Pressed(Gamepad.current?.rightShoulder);

    public bool FreezePressed() => Pressed(FreezeKey);

    public bool HudPressed() => Pressed(HudKey);

    private bool Pressed(Key key)
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null || !keyboard[key].wasPressedThisFrame)
            return false;

        lastPressWasGamepad = false;
        return true;
    }

    private bool Pressed(ButtonControl button)
    {
        if (button == null || !button.wasPressedThisFrame)
            return false;

        lastPressWasGamepad = true;
        return true;
    }

    private bool UsingGamepad()
    {
        if (!GameContext.exists)
            return lastPressWasGamepad;

        GameInputManager manager = GameContext.gameInputManager;
        return manager == null ? lastPressWasGamepad : manager.currentControllerType == EControllerType.Gamepad;
    }

    private static string Name(Key key)
    {
        string raw = key.ToString();
        return raw.StartsWith("Digit", StringComparison.Ordinal) ? raw.Substring(5) : raw;
    }
}
