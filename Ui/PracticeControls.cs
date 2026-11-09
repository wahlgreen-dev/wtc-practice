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
    private const Key PreviousKey = Key.Digit4;
    private const Key NextKey = Key.Digit5;
    private const Key HudKey = Key.F1;

    private const string SaveIcon = "xbox_lb_outline";
    private const string RestoreIcon = "xbox_rb_outline";
    private const string FreezeIcon = "xbox_dpad_down_outline";
    private const string PreviousIcon = "xbox_dpad_left_outline";
    private const string NextIcon = "xbox_dpad_right_outline";

    private bool lastPressWasGamepad;

    public (string Icon, string Action)[] Rows()
    {
        bool gamepad = UsingGamepad();

        return new[]
        {
            (gamepad ? SaveIcon : KeyIcon(SaveKey), "Checkpoint"),
            (gamepad ? RestoreIcon : KeyIcon(RestoreKey), "Teleport"),
            (gamepad ? FreezeIcon : KeyIcon(FreezeKey), "Freeze"),
            (gamepad ? PreviousIcon : KeyIcon(PreviousKey), "Previous"),
            (gamepad ? NextIcon : KeyIcon(NextKey), "Next"),
        };
    }

    public bool SavePressed() => Pressed(SaveKey) || Pressed(Gamepad.current?.leftShoulder);

    public bool RestorePressed() => Pressed(RestoreKey) || Pressed(Gamepad.current?.rightShoulder);

    public bool FreezePressed() => Pressed(FreezeKey) || Pressed(Gamepad.current?.dpad.down);

    public bool PreviousPressed() => Pressed(PreviousKey) || Pressed(Gamepad.current?.dpad.left);

    public bool NextPressed() => Pressed(NextKey) || Pressed(Gamepad.current?.dpad.right);

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

    private static string KeyIcon(Key key) => "keyboard_" + key.ToString().Replace("Digit", string.Empty) + "_outline";
}
