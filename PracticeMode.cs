using UnityEngine.InputSystem;

namespace WtcPractice;

public static class PracticeMode
{
    public const Key SaveKey = Key.Digit1;
    public const Key RestoreKey = Key.Digit2;
    public const Key ToggleHudKey = Key.F1;

    private static CarSnapshot? slot;

    public static void Tick()
    {
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
    }

    private static void SaveState()
    {
        if (!LevelWorld.InLevel)
        {
            PracticeCore.Log.Msg("[practice] not in a level");
            return;
        }

        string levelId = LevelWorld.ContentId;
        if (string.IsNullOrEmpty(levelId))
        {
            PracticeCore.Log.Warning("[practice] level not found yet");
            return;
        }

        CarSnapshot? captured = CarState.TryCapture(levelId);
        if (!captured.HasValue)
            return;

        slot = captured;
        PracticeCore.Log.Msg("[practice] state saved");
    }

    private static void RestoreState()
    {
        if (!LevelWorld.InLevel)
        {
            PracticeCore.Log.Msg("[practice] not in a level");
            return;
        }

        if (!slot.HasValue)
        {
            PracticeCore.Log.Msg($"[practice] nothing saved — press {SaveKey} first");
            return;
        }

        if (!slot.Value.Matches(LevelWorld.ContentId))
        {
            PracticeCore.Log.Msg($"[practice] saved state belongs to {slot.Value.LevelId}");
            return;
        }

        if (!CarState.TryRestore(slot.Value))
            return;

        PracticeCore.Log.Msg("[practice] state restored");
        RunIntegrity.MarkDirty("restored a saved state");
    }

    private static void DropSlotIfLevelChanged()
    {
        if (!slot.HasValue)
            return;

        string levelId = LevelWorld.ContentId;
        if (string.IsNullOrEmpty(levelId) || slot.Value.Matches(levelId))
            return;

        PracticeCore.Log.Msg($"[practice] left {slot.Value.LevelId} — saved state dropped");
        slot = null;
    }
}
