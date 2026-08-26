using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace WtcPractice;

public static class PracticeMode
{
    public const Key SaveKey = Key.Digit1;
    public const Key RestoreKey = Key.Digit2;
    public const Key ToggleHudKey = Key.F1;

    public const string SaveButton = "LB";
    public const string RestoreButton = "RB";

    private static CarSnapshot? slot;
    private static Vector3? pendingWarp;

    public static void LateTick()
    {
        if (!pendingWarp.HasValue)
            return;

        PracticeCamera.OnCarWarped(pendingWarp.Value);
        pendingWarp = null;
    }

    public static bool CanPractice { get; private set; }

    public static void Tick()
    {
        CanPractice = LevelWorld.IsTimerRunning();

        DropSlotIfLevelChanged();

        Keyboard keyboard = Keyboard.current;
        Gamepad gamepad = Gamepad.current;

        if (Pressed(keyboard, SaveKey) || Pressed(gamepad?.leftShoulder))
            SaveState();

        if (Pressed(keyboard, RestoreKey) || Pressed(gamepad?.rightShoulder))
            RestoreState();

        if (Pressed(keyboard, ToggleHudKey))
            PracticeHud.Toggle();
    }

    private static bool Pressed(Keyboard keyboard, Key key)
    {
        if (keyboard == null || !keyboard[key].wasPressedThisFrame)
            return false;

        GameInput.RememberKeyboard();
        return true;
    }

    private static bool Pressed(ButtonControl button)
    {
        if (button == null || !button.wasPressedThisFrame)
            return false;

        GameInput.RememberGamepad();
        return true;
    }

    public static void Forget()
    {
        LevelWorld.Forget();
        PracticeCamera.Forget();
        PracticeHud.ClearStatus();
    }

    private static void SaveState()
    {
        if (!LevelWorld.InLevel)
        {
            PracticeCore.Log.Msg("[practice] not in a level");
            return;
        }

        if (!LevelWorld.IsTimerRunning())
        {
            PracticeCore.Log.Msg("[practice] wait until the launch is done");
            return;
        }

        string levelId = LevelWorld.GetContentId();
        if (string.IsNullOrEmpty(levelId))
        {
            PracticeCore.Log.Warning("[practice] level not found yet");
            return;
        }

        CarSnapshot? captured = CarState.TryCapture(levelId, LevelWorld.GetTime());
        if (!captured.HasValue)
            return;

        slot = captured;
        PracticeHud.Say("Checkpoint set");
        PracticeCore.Log.Msg("[practice] checkpoint set");
    }

    private static void RestoreState()
    {
        if (!LevelWorld.InLevel)
        {
            PracticeCore.Log.Msg("[practice] not in a level");
            return;
        }

        if (!LevelWorld.IsTimerRunning())
        {
            PracticeCore.Log.Msg("[practice] wait until the launch is done");
            return;
        }

        if (!slot.HasValue)
        {
            PracticeCore.Log.Msg($"[practice] no checkpoint yet, press {SaveKey} first");
            return;
        }

        if (!slot.Value.Matches(LevelWorld.GetContentId()))
        {
            PracticeCore.Log.Msg($"[practice] checkpoint belongs to {slot.Value.LevelId}");
            return;
        }

        if (!CarState.TryRestore(slot.Value, out Vector3 warp))
            return;

        LevelWorld.SetTime(slot.Value.Time);
        pendingWarp = warp;

        PracticeHud.Say("Teleported");
        PracticeCore.Log.Msg("[practice] teleported to checkpoint");
        RunIntegrity.MarkDirty("teleported to a checkpoint");
    }

    private static void DropSlotIfLevelChanged()
    {
        if (!slot.HasValue)
            return;

        string levelId = LevelWorld.GetContentId();
        if (string.IsNullOrEmpty(levelId) || slot.Value.Matches(levelId))
            return;

        PracticeCore.Log.Msg($"[practice] left {slot.Value.LevelId}, checkpoint dropped");
        slot = null;
    }
}
