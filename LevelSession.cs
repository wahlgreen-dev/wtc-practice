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

    private Checkpoint checkpoint;
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

        if (controls.RestorePressed())
            RestoreCheckpoint();

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
        hud.Draw(canPractice);
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

        checkpoint = new Checkpoint(levelId, world.GetTime(), captured.Value);
        hud.Say("Checkpoint set");
    }

    private void RestoreCheckpoint()
    {
        if (!canPractice || checkpoint == null || !checkpoint.BelongsTo(world.GetContentId()))
            return;

        if (!car.TryRestore(checkpoint.Car, out Vector3 warp))
            return;

        world.SetTime(checkpoint.Time);
        pendingWarp = warp;
        RunInvalidated = true;
        hud.Say("Teleported");
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
        if (checkpoint == null)
            return;

        string levelId = world.GetContentId();
        if (string.IsNullOrEmpty(levelId) || checkpoint.BelongsTo(levelId))
            return;

        checkpoint = null;
    }
}
