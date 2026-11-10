using MelonLoader;
using UnityEngine;

namespace WtcPractice;

public class LevelSession
{
    private const float ClearHoldSeconds = 0.5f;

    private readonly MelonLogger.Instance logger;
    private readonly LevelWorld world = new();
    private readonly PracticeCamera practiceCamera = new();
    private readonly PracticeControls controls = new();
    private readonly PracticeHud hud;
    private readonly CarState car;
    private readonly CheckpointFlags flags;

    private readonly CheckpointList checkpoints = new();

    private Vector3? pendingWarp;
    private bool canPractice;
    private float clearHeldSeconds;

    public LevelSession(MelonLogger.Instance logger)
    {
        this.logger = logger;
        hud = new PracticeHud(logger, controls);
        car = new CarState(logger);
        flags = new CheckpointFlags(logger);
    }

    public bool Frozen { get; private set; }

    public bool RunInvalidated { get; private set; }

    public void Update()
    {
        canPractice = world.IsTimerRunning();

        if (!canPractice)
            Frozen = false;

        DropCheckpointIfLevelChanged();
        flags.Update(world.InLevel, checkpoints);

        if (controls.SavePressed())
            SaveCheckpoint();

        if (controls.RestorePressed() && TeleportToCurrent())
            hud.Say("Teleported");

        if (controls.PreviousPressed())
            StepCheckpoint(-1);

        if (controls.NextPressed())
            StepCheckpoint(1);

        UpdateClearHold();

        if (controls.FreezePressed())
            ToggleFreeze();

        if (controls.HudPressed())
            hud.Toggle();
    }

    public void LateUpdate()
    {
        if (!pendingWarp.HasValue)
            return;

        practiceCamera.OnCarWarped(pendingWarp.Value);
        pendingWarp = null;
    }

    public void DrawHud()
    {
        hud.Draw(canPractice, !RunInvalidated, clearHeldSeconds / ClearHoldSeconds);
    }

    public void OnSceneInitialized(string sceneName)
    {
        flags.OnSceneInitialized(sceneName);
        Frozen = false;
        RunInvalidated = false;
        world.Forget();
        practiceCamera.Forget();
        hud.ClearStatus();
    }

    public void OnNewAttempt()
    {
        RunInvalidated = false;
    }

    private void SaveCheckpoint()
    {
        if (!canPractice)
            return;

        string levelId = world.GetContentId();
        if (string.IsNullOrEmpty(levelId))
        {
            logger.Warning("Level not found yet");
            return;
        }

        CarSnapshot? captured = car.TryCapture();
        if (!captured.HasValue)
            return;

        checkpoints.Add(new Checkpoint(levelId, world.GetTime(), captured.Value));
        flags.Sync(checkpoints);
        SayPosition();
    }

    private bool TeleportToCurrent()
    {
        Checkpoint checkpoint = checkpoints.Current;
        if (!canPractice || checkpoint == null || !checkpoint.BelongsTo(world.GetContentId()))
            return false;

        if (!car.TryRestore(checkpoint.Car, out Vector3 warp))
            return false;

        world.SetTime(checkpoint.Time);
        pendingWarp = warp;
        RunInvalidated = true;
        return true;
    }

    private void StepCheckpoint(int direction)
    {
        if (!canPractice || !checkpoints.Step(direction))
            return;

        flags.Sync(checkpoints);

        if (TeleportToCurrent())
            SayPosition();
    }

    private void UpdateClearHold()
    {
        if (!canPractice || checkpoints.Count == 0 || !controls.ClearHeld())
        {
            clearHeldSeconds = 0f;
            return;
        }

        clearHeldSeconds += Time.unscaledDeltaTime;
        if (clearHeldSeconds < ClearHoldSeconds)
            return;

        checkpoints.Clear();
        clearHeldSeconds = 0f;
        hud.Say("Checkpoints cleared");
    }

    private void SayPosition()
    {
        hud.Say($"Checkpoint {checkpoints.Position}/{checkpoints.Count}");
    }

    private void ToggleFreeze()
    {
        if (!canPractice)
            return;

        Frozen = !Frozen;

        if (Frozen)
            RunInvalidated = true;

        hud.Say(Frozen ? "Frozen" : "Running");
    }

    private void DropCheckpointIfLevelChanged()
    {
        Checkpoint current = checkpoints.Current;
        if (current == null)
            return;

        string levelId = world.GetContentId();
        if (string.IsNullOrEmpty(levelId) || current.BelongsTo(levelId))
            return;

        checkpoints.Clear();
    }
}
