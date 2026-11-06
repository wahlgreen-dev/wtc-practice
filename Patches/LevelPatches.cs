using HarmonyLib;
using Il2CppSpeed.Level;

namespace WtcPractice;

[HarmonyPatch(typeof(LevelIntroState), nameof(LevelIntroState.OnEnterState))]
internal static class LevelIntroPatch
{
    private static void Postfix()
    {
        PracticeMod.Instance.Session.OnNewAttempt();
    }
}

[HarmonyPatch(typeof(LevelInstance), nameof(LevelInstance.PrepareForRestart))]
internal static class LevelRestartPatch
{
    private static void Postfix()
    {
        PracticeMod.Instance.Session.OnNewAttempt();
    }
}
