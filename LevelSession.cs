using MelonLoader;
using UnityEngine;

namespace WtcPractice;

public class LevelSession
{
    private readonly MelonLogger.Instance logger;
    private readonly LevelWorld world = new();
    private readonly PracticeCamera practiceCamera = new();
    private readonly PracticeControls controls = new();
    private readonly PracticeHud hud;
    private readonly CarState car;

    private readonly CheckpointList checkpoints = new();

    private Vector3? pendingWarp;
    private bool canPractice;

    public LevelSession(MelonLogger.Instance logger)
    {
        this.logger = logger;
        hud = new PracticeHud(logger, controls);
        car = new CarState(logger);
    }

    public bool Frozen { get; private set; }

    public bool RunInvalidated { get; private set; }

    public void Update()
    {
        canPractice = world.IsTimerRunning();

        if (!canPractice)
            Frozen = false;

        DropCheckpointIfLevelChanged();

        if (controls.SavePressed())
            SaveCheckpoint();

        if (controls.RestorePressed() && TeleportToCurrent())
            hud.Say("Teleported");

        if (controls.PreviousPressed())
            StepCheckpoint(-1);

        if (controls.NextPressed())
            StepCheckpoint(1);

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
        hud.Draw(canPractice, !RunInvalidated);
    }

    public void OnSceneInitialized()
    {
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
        if (canPractice && checkpoints.Step(direction) && TeleportToCurrent())
            SayPosition();
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
