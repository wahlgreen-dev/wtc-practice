using Il2CppSpeed;
using Il2CppSpeed.Level;
using Object = UnityEngine.Object;

namespace WtcPractice;

public static class  LevelWorld
{
    private static LevelGameplayState gameplay;

    private static bool Exists => GameContext.exists;

    private static GameFlow Flow => Exists ? GameContext.gameFlow : null;

    public static bool InLevel => Flow != null && Flow.isInLevel;

    public static string ContentId
    {
        get
        {
            if (!InLevel)
                return null;

            if (gameplay == null)
                gameplay = Object.FindObjectOfType<LevelGameplayState>();

            if (gameplay == null)
                return null;

            LevelInstance level = gameplay.level;
            return level == null ? null : level.contentId;
        }
    }

    public static void Forget() => gameplay = null;
}
