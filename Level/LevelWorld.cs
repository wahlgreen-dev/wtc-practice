using Il2CppSpeed;
using Il2CppSpeed.Level;
using Object = UnityEngine.Object;

namespace WtcPractice;

public class LevelWorld
{
    private LevelGameplayState gameplay;

    private static GameFlow Flow => GameContext.exists ? GameContext.gameFlow : null;

    public bool InLevel => Flow != null && Flow.isInLevel;

    public string GetContentId()
    {
        if (!InLevel || !EnsureGameplay())
            return null;

        return gameplay.level?.contentId;
    }

    public bool IsTimerRunning()
    {
        LevelTimer timer = GetTimer();
        return timer != null && timer._running;
    }

    public float GetTime()
    {
        LevelTimer timer = GetTimer();
        return timer == null ? 0f : timer.time;
    }

    public void SetTime(float time)
    {
        LevelTimer timer = GetTimer();
        if (timer != null)
            timer.time = time;
    }

    public void Forget() => gameplay = null;

    private LevelTimer GetTimer()
    {
        if (!InLevel || !EnsureGameplay())
            return null;

        return gameplay._levelTimer;
    }

    private bool EnsureGameplay()
    {
        if (gameplay == null)
            gameplay = Object.FindObjectOfType<LevelGameplayState>();

        return gameplay != null;
    }
}
