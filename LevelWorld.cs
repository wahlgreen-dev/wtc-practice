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

    public static void Forget() => gameplay = null;
}
