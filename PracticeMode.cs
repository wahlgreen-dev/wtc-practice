using UnityEngine;
using UnityEngine.InputSystem;

namespace WtcPractice;

public static class PracticeMode
{
    public const Key SaveKey = Key.Digit1;
    public const Key RestoreKey = Key.Digit2;
    public const Key ToggleHudKey = Key.F1;

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
        if (keyboard == null)
            return;

        if (keyboard[SaveKey].wasPressedThisFrame)
            SaveState();

        if (keyboard[RestoreKey].wasPressedThisFrame)
            RestoreState();

        if (keyboard[ToggleHudKey].wasPressedThisFrame)
            PracticeHud.Toggle();
    }

    public static void Forget()
    {
        LevelWorld.Forget();
        PracticeCamera.Forget();
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
