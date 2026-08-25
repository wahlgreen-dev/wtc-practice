using Il2CppSpeed;
using Il2CppSpeed.Level;
using Object = UnityEngine.Object;

namespace WtcPractice;

public static class LevelWorld
{
    private static LevelGameplayState gameplay;

    private static bool Exists => GameContext.exists;

    private static GameFlow Flow => Exists ? GameContext.gameFlow : null;

    public static bool InLevel => Flow != null && Flow.isInLevel;

    private static bool EnsureGameplay()
    {
        if (gameplay == null)
            gameplay = Object.FindObjectOfType<LevelGameplayState>();

        return gameplay != null;
    }

    public static string GetContentId()
    {
        if (!InLevel)
            return null;

        if (!EnsureGameplay())
            return null;

        return gameplay.level?.contentId;
    }

    public static bool IsTimerRunning()
    {
        LevelTimer timer = GetTimer();

        if (timer == null)
            return false;

        return timer._running;
    }

    public static float GetTime()
    {
        LevelTimer timer = GetTimer();

        if (timer == null)
            return 0f;

        return timer.time;
    }

    public static bool SetTime(float time)
    {
        LevelTimer timer = GetTimer();

        if (timer == null)
            return false;

        timer.time = time;
        return true;
    }

    private static LevelTimer GetTimer()
    {
        if (!InLevel)
            return null;

        if (!EnsureGameplay())
            return null;

        return gameplay._levelTimer;
    }

    public static void Forget() => gameplay = null;
}
